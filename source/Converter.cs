using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace BMP2Tile
{
    internal class Converter: IDisposable
    {
        #region Fields

        private readonly Action<string> _logger;
        private Palette _palette;
        private List<Tile> _tiles;
        private Tilemap _tilemap;
        private readonly Dictionary<string, ICompressor> _compressors = new Dictionary<string, ICompressor>();

        private bool _removeDuplicates = true;
        private bool _useMirroring = true;
        private bool _adjacentBelow;
        private uint _tileOffset;
        private bool _highPriority;
        private bool _useSpritePalette;
        private bool _fullPalette;
        private string _filename;
        private Bitmap _bitmap;
        private bool _optimized;

        #endregion

        public Converter(Action<string> logger)
        {
            _logger = logger;
        }

        #region Properties

        public bool RemoveDuplicates
        {
            set 
            { 
                _removeDuplicates = value;
                _tiles = null;
                _tilemap = null;
            }
        }

        public bool UseMirroring
        {
            set
            {
                _useMirroring = value; 
                _tiles = null;
                _tilemap = null;
            }
        }

        public bool AdjacentBelow
        {
            set
            {
                _adjacentBelow = value; 
                _tiles = null;
                _tilemap = null;
            }
        }

        public bool Chunky { private get; set; } // Encode-time property -> no need to clear state

        public uint TileOffset
        {
            set 
            { 
                _tileOffset = value;
                _tilemap = null;
            }
        }

        public bool UseSpritePalette
        {
            set
            {
                _useSpritePalette = value;
                _tilemap = null;
            }
        }

        public bool HighPriority
        {
            set
            {
                _highPriority = value;
                _tilemap = null;
            }
        }

        public bool FullPalette
        {
            set
            {
                _fullPalette = value;
                _palette = null;
            }
        }

        public string Filename
        {
            set
            {
                _filename = value;
                _tiles = null;
                _tilemap = null;

                if (_bitmap != null)
                {
                    _bitmap.Dispose();
                    _bitmap = null;
                }
            }
        }

        public Palette.Formats PaletteFormat { private get; set; }
        public int LogLevel { private get; set; }

        #endregion

        #region Public methods

        public void SaveTiles(string filename)
        {
            GetTiles();
            if (_removeDuplicates)
            {
                Optimize();
            }

            Log("Saving tiles...", true);

            var compressor = GetCompressor(filename);
            var bytes = compressor.CompressTiles(_tiles, Chunky);
            File.WriteAllBytes(filename, bytes.ToArray());

            Log($"Saved tiles in format \"{compressor.Name}\" to {filename}");
        }

        public void SaveTilemap(string filename)
        {
            GetTilemap();
            if (_removeDuplicates)
            {
                Optimize();
            }

            var compressor = GetCompressor(filename);
            Log("Compressing tilemap...", true);
            var bytes = compressor.CompressTilemap(_tilemap);
            File.WriteAllBytes(filename, bytes.ToArray());

            _logger($"Saved tilemap in format \"{compressor.Name}\" to {filename}");
        }

        public void SavePalette(string filename)
        {
            GetPalette();
            Log("Saving palette...", true);
            if (Path.GetExtension(filename.ToLowerInvariant()) == ".inc")
            {
                File.WriteAllText(filename, _palette.ToString(PaletteFormat));
            }
            else
            {
                File.WriteAllBytes(filename, _palette.GetValue(PaletteFormat).ToArray());
            }

            Log($"Saved palette to {filename}");
        }

        #endregion

        private ICompressor GetCompressor(string filename)
        {
            GetCompressors();
            var extension = Path.GetExtension(filename)?.ToLowerInvariant();
            if (extension == null)
            {
                throw new Exception($"Failed to get extension from filename: {filename}");
            }

            if (_compressors.TryGetValue(extension, out var result))
            {
                return result;
            }

            throw new Exception($"Failed to find handler for extension {extension} (filename {filename})");
        }

        private void GetCompressors()
        {
            if (_compressors.Count > 0)
            {
                return;
            }

            Log("Discovering compressors", true);

            // We add some C#-based ones...
            _compressors.Add(".inc", new IncludeTextWriter());

            // We enumerate files in the program folder
            var path = AppContext.BaseDirectory;

            // ReSharper disable once StringLiteralTypo
            foreach (var filename in Directory.EnumerateFiles(path, "gfxcomp_*.dll"))
            {
                try
                {
                    var compressor = new CompressionDllWrapper(filename);
                    if (compressor.Capabilities == CompressorCapabilities.None)
                    {
                        Log($"Loaded {filename} but found no compressors");
                        continue;
                    }
                    _compressors["." + compressor.Extension.ToLowerInvariant()] = compressor;
                    Log($"Added \"{compressor.Name}\" ({compressor.Extension}) from {filename}", true);
                }
                catch (Exception ex)
                {
                    Log($"Failed to load {filename}: {ex.Message}");
                }
            }

            Log($"Added {_compressors.Count} compressors", true);
        }

        private void GetBitmap()
        {
            if (_bitmap != null)
            {
                return;
            }

            Log($"Loading {_filename}...");

            _bitmap = new Bitmap(_filename);

            // Check the dimensions
            if (_bitmap.Width % 8 != 0)
            {
                throw new Exception($"Image's width is not a multiple of 8: {_bitmap.Width}");
            }

            // BUG: this may be changed later, so we can't check it now
            if (_adjacentBelow)
            {
                if (_bitmap.Height % 16 != 0)
                {
                    throw new Exception($"Image's height is not a multiple of 16: {_bitmap.Height}");
                }
            }
            else
            {
                if (_bitmap.Height % 8 != 0)
                {
                    throw new Exception($"Image's height is not a multiple of 8: {_bitmap.Height}");
                }
            }

            Log($"Loaded bitmap from {_filename}", true);
        }

        private void Optimize()
        {
            if (_optimized || !_removeDuplicates)
            {
                return;
            }

            GetTiles();
            GetTilemap();

            Log("Optimizing...", true);

            var tileCountBefore = _tiles.Count;

            // We pass through the tiles and clear out any duplicates
            for (int i = 0; i < _tiles.Count; ++i)
            {
                // Compare tile i to the ones following it
                // Replace duplicates with this one
                var thisTile = _tiles[i];
                for (int j = i + 1; j < _tiles.Count; /* increment in loop */)
                {
                    var comparedTile = _tiles[j];
                    var comparison = thisTile.Compare(comparedTile, _useMirroring);
                    if (comparison == Tile.Match.None)
                    {
                        ++j;
                        continue;
                    }
                    // We have a match, so we want to remove it from the collection...
                    _tiles.RemoveAt(j);
                    // ...and replace all entries referencing it in the tilemap, plus move all higher indices down by 1
                    foreach (var entry in _tilemap)
                    {
                        if (entry.TileIndex == j)
                        {
                            entry.TileIndex = i;
                            switch (comparison)
                            {
                                case Tile.Match.HFlip:
                                    entry.HFlip = true;
                                    break;
                                case Tile.Match.VFlip:
                                    entry.VFlip = true;
                                    break;
                                case Tile.Match.BothFlip:
                                    entry.HFlip = true;
                                    entry.VFlip = true;
                                    break;
                            }
                        }
                        else if (entry.TileIndex > j)
                        {
                            --entry.TileIndex;
                        }
                    }
                }
            }

            Log($"Reduced from {tileCountBefore} to {_tiles.Count} tiles", true);

            _optimized = true;
        }

        private void GetTilemap()
        {
            if (_tilemap != null)
            {
                return;
            }

            GetBitmap();

            Log("Creating tilemap", true);

            // This is an unoptimised tilemap
            // So we just need to fill space
            var tilemap = new Tilemap(_bitmap.Width / 8, _bitmap.Height / 8);

            var i = (int)_tileOffset;
            // We fill space by using the same function used to extract the tiles
            foreach (var point in GetTileCoordinates(_bitmap.Width, _bitmap.Height))
            {
                tilemap[point.X / 8, point.Y / 8] = new Tilemap.Entry
                {
                    TileIndex = i++,
                    HFlip = false,
                    VFlip = false,
                    HighPriority = _highPriority,
                    UseSpritePalette = _useSpritePalette
                };
            }

            _tilemap = tilemap;

            Log($"Created {_tilemap.Width}x{tilemap.Height} tilemap", true);
        }

        private void GetTiles()
        {
            if (_tiles != null)
            {
                // No need to regenerate
                return;
            }

            GetBitmap();

            Log("Generating tiles from image", true);

            BitmapData bitmapData = null;
            try
            {
                bitmapData = _bitmap.LockBits(
                    new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format8bppIndexed); // TODO is this converting? 4bpp: OK >8bpp: ???

                var tiles = new List<Tile>();

                // We want to split the image to 8x8 chunks in the required order
                foreach (var coordinate in GetTileCoordinates(_bitmap.Width, _bitmap.Height))
                {
                    var tileData = new byte[8 * 8];
                    for (int y = 0; y < 8; ++y)
                    {
                        // Copy data 8 pixels at a time
                        Marshal.Copy(
                            bitmapData.Scan0 + bitmapData.Stride * (coordinate.Y + y) + coordinate.X,
                            tileData,
                            y * 8,
                            8);
                    }

                    tiles.Add(new Tile(tileData));
                }

                _optimized = false;

                _tiles = tiles;

                Log($"Created {_tiles.Count} tiles", true);
            }
            finally
            {
                if (bitmapData != null)
                {
                    _bitmap.UnlockBits(bitmapData);
                }
            }
        }

        private IEnumerable<Point> GetTileCoordinates(int width, int height)
        {
            // We generate the tile coordinates in row-major order, optionally with "double height sprite" ordering
            if (_adjacentBelow)
            {
                for (int y = 0; y < height; y += 16)
                for (int x = 0; x < width; x += 8)
                {
                    yield return new Point(x, y);
                    yield return new Point(x, y+8);
                }
            }
            else
            {
                for (int y = 0; y < height; y += 8)
                for (int x = 0; x < width; x += 8)
                {
                    yield return new Point(x, y);
                }
            }
        }

        private void GetPalette()
        {
            if (_palette != null)
            {
                return;
            }

            GetBitmap();

            // First we read out the palette
            // TODO check what we get on non-paletted images - I'm assuming null
            if (_bitmap.Palette == null)
            {
                throw new Exception("Image is not paletted. You must provide a 4- or 8-bit paletted image.");
            }

            // We want to find the highest index used in the data
            // We do that by getting the tiles...
            GetTiles();
            var highestIndexUsed = _tiles.SelectMany(tile => tile.Indices).Max();
            if (highestIndexUsed > 15)
            {
                var numIndicesUsed = _tiles.SelectMany(tile => tile.Indices).Distinct().Count();
                throw new Exception($"Image uses colours up to index {highestIndexUsed} - this must be no more than 15. There are {numIndicesUsed} palette entries used.");
            }

            var paletteEntries = _bitmap.Palette.Entries.ToList();

            if (_fullPalette)
            {
                // Extend to 16 if smaller
                if (paletteEntries.Count < 16)
                {
                    Log("Extending palette to 16 entries", true);
                    paletteEntries.AddRange(Enumerable.Repeat(Color.Black, 16 - paletteEntries.Count));
                }
            }
            else if (paletteEntries.Count > highestIndexUsed)
            {
                Log($"Truncating palette to {highestIndexUsed} entries", true);
                paletteEntries.RemoveRange(highestIndexUsed + 1, paletteEntries.Count - (highestIndexUsed + 1));
            }

            _palette = new Palette(paletteEntries);
        }

        private void Log(string message, bool ifVerbose = false)
        {
            if (ifVerbose && LogLevel < 2)
            {
                return;
            }

            if (LogLevel < 1)
            {
                return;
            }

            _logger(message);
        }

        public void Dispose()
        {
            foreach (var compressor in _compressors.Values)
            {
                compressor.Dispose();
            }
            _compressors.Clear();
        }
    }
}
