using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BMP2Tile
{
    public class Converter: IDisposable
    {
        #region Fields

        private readonly Action<string, LogLevel> _logger;
        private Palette _palette;
        private List<Tile> _tiles;
        private Tilemap _tilemap;
        private readonly Dictionary<string, ICompressorImpl> _compressors = new Dictionary<string, ICompressorImpl>();

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
        private readonly ICompressorImpl _includeTextWriter = new IncludeTextWriter();

        #endregion

        public enum LogLevel
        {
            Verbose,
            Normal,
            Error
        }

        public Converter(Action<string, LogLevel> logger)
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

        #endregion

        #region Public methods

        public void SaveTiles(string filename)
        {
            GetTiles();
            if (_removeDuplicates)
            {
                Optimize();
            }

            Log("Saving tiles...", LogLevel.Verbose);

            var compressor = GetCompressor(filename);
            var bytes = compressor.CompressTiles(_tiles, Chunky);
            File.WriteAllBytes(filename, bytes.ToArray());

            Log($"Saved tiles in format \"{compressor.Name}\" to {filename}");
        }

        public string GetTilesAsText()
        {
            GetTiles();
            if (_removeDuplicates)
            {
                Optimize();
            }

            return Encoding.ASCII.GetString(_includeTextWriter.CompressTiles(_tiles, Chunky).ToArray());
        }

        public void SaveTilemap(string filename)
        {
            GetTilemap();
            if (_removeDuplicates)
            {
                Optimize();
            }

            var compressor = GetCompressor(filename);
            Log("Compressing tilemap...", LogLevel.Verbose);
            var bytes = compressor.CompressTilemap(_tilemap);
            File.WriteAllBytes(filename, bytes.ToArray());

            Log($"Saved tilemap in format \"{compressor.Name}\" to {filename}");
        }

        public string GetTilemapAsText()
        {
            GetTilemap();
            if (_removeDuplicates)
            {
                Optimize();
            }

            return Encoding.ASCII.GetString(_includeTextWriter.CompressTilemap(_tilemap).ToArray());
        }

        public void SavePalette(string filename)
        {
            GetPalette();
            Log("Saving palette...", LogLevel.Verbose);
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

        public string GetPaletteAsText(Palette.Formats format)
        {
            GetPalette();
            return _palette.ToString(format);
        }

        public List<List<Color>> GetPalettes(Palette.Formats format)
        {
            GetPalette();
            var convertedPalette = _palette.ForDisplay(format);
            return new List<List<Color>>
            {
                _bitmap.Palette.Entries.Take(convertedPalette.Count).ToList(),
                convertedPalette
            };
        }

        public IEnumerable<ICompressor> GetCompressorInfo()
        {
            GetCompressors();

            return _compressors.Values;
        }

        #endregion

        private ICompressorImpl GetCompressor(string filename)
        {
            GetCompressors();
            var extension = Path.GetExtension(filename)?.ToLowerInvariant();
            if (extension == null)
            {
                throw new AppException($"Failed to get extension from filename: {filename}");
            }

            if (_compressors.TryGetValue(extension, out var result))
            {
                return result;
            }

            throw new AppException($"Failed to find handler for extension {extension} (filename {filename})");
        }

        private void GetCompressors()
        {
            if (_compressors.Count > 0)
            {
                return;
            }

            Log("Discovering compressors", LogLevel.Verbose);

            // We add some C#-based ones...
            _compressors.Add(".inc", _includeTextWriter);

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
                    Log($"Added \"{compressor.Name}\" ({compressor.Extension}) from {filename}", LogLevel.Verbose);
                }
                catch (System.Exception ex)
                {
                    Log($"Failed to load {filename}: {ex.Message}");
                }
            }

            Log($"Added {_compressors.Count} compressors", LogLevel.Verbose);
        }

        private void GetBitmap()
        {
            if (_bitmap == null)
            {
                if (string.IsNullOrEmpty(_filename))
                {
                    throw new AppException("Filename has not been specified");
                }

                try
                {
                    Log($"Loading {_filename}...");

                    _bitmap = new Bitmap(_filename);

                    Log($"Loaded bitmap from {_filename}", LogLevel.Verbose);

                    // Check the dimensions
                    if (_bitmap.Width % 8 != 0)
                    {
                        throw new AppException($"Image's width ({_bitmap.Width}) is not a multiple of 8");
                    }

                    if (_bitmap.Height % 8 != 0)
                    {
                        throw new AppException($"Image's height ({_bitmap.Height})is not a multiple of 8");
                    }
                }
                catch (AppException)
                {
                    _bitmap = null;
                    throw;
                }
            }
        }

        private void Optimize()
        {
            if (_optimized || !_removeDuplicates)
            {
                return;
            }

            GetTiles();
            GetTilemap();

            Log("Optimizing...", LogLevel.Verbose);

            var tileCountBefore = _tiles.Count;

            // We pass through the tiles and clear out any duplicates
            for (int i = 0; i < _tiles.Count; ++i)
            {
                // Compare tile i to the ones following it
                // Replace duplicates with this one
                var thisTile = _tiles[i];
                var thisTileIndex = (int)(i + _tileOffset);
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
                    var tileIndexToReplace = (int)(j + _tileOffset);
                    foreach (var entry in _tilemap)
                    {
                        if (entry.TileIndex == tileIndexToReplace)
                        {
                            entry.TileIndex = thisTileIndex;
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
                        else if (entry.TileIndex > tileIndexToReplace)
                        {
                            --entry.TileIndex;
                        }
                    }
                }
            }

            Log($"Reduced from {tileCountBefore} to {_tiles.Count} tiles");

            _optimized = true;
        }

        private void GetTilemap()
        {
            if (_tilemap != null)
            {
                return;
            }

            GetBitmap();

            Log("Creating tilemap", LogLevel.Verbose);

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

            Log($"Created {_tilemap.Width}x{tilemap.Height} tilemap", LogLevel.Verbose);
        }

        private void GetTiles()
        {
            if (_tiles != null)
            {
                // No need to regenerate
                return;
            }

            GetBitmap();

            Log("Generating tiles from image", LogLevel.Verbose);

            BitmapData bitmapData = null;
            try
            {
                // In Windows we can just specify 8bpp here and GDI+ extends 4bpp images for us.
                // However this is not implemented in Wine so we convert it ourselves.
                bitmapData = _bitmap.LockBits(
                    new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
                    ImageLockMode.ReadOnly,
                    _bitmap.PixelFormat);

                var tiles = new List<Tile>();

                // We want to split the image to 8x8 chunks in the required order
                foreach (var coordinate in GetTileCoordinates(_bitmap.Width, _bitmap.Height))
                {
                    var tileData = GetTile(coordinate, bitmapData);

                    tiles.Add(new Tile(tileData));
                }

                _optimized = false;

                _tiles = tiles;

                Log($"Created {_tiles.Count} tiles", LogLevel.Verbose);
            }
            finally
            {
                if (bitmapData != null)
                {
                    _bitmap.UnlockBits(bitmapData);
                }
            }
        }

        private byte[] GetTile(Point coordinate, BitmapData bitmapData)
        {
            var tileData = new byte[8 * 8];

            switch (bitmapData.PixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    for (int y = 0; y < 8; ++y)
                    {
                        // Copy data 8 pixels at a time
                        Marshal.Copy(
                            bitmapData.Scan0 + bitmapData.Stride * (coordinate.Y + y) + coordinate.X,
                            tileData,
                            y * 8,
                            8);
                    }
                    break;
                case PixelFormat.Format4bppIndexed:
                    for (int y = 0; y < 8; ++y)
                    {
                        // We need to extend from 4bpp to 8bpp
                        var data = bitmapData.Scan0 + bitmapData.Stride * (coordinate.Y + y) + coordinate.X / 2;
                        for (int i = 0; i < 4; ++i)
                        {
                            var b = Marshal.ReadByte(data, i);
                            tileData[y * 8 + i * 2 + 0] = (byte)((b >> 4) & 0xf);
                            tileData[y * 8 + i * 2 + 1] = (byte)((b >> 0) & 0xf);
                        }
                    }
                    break;
                case PixelFormat.Format1bppIndexed:
                    for (int y = 0; y < 8; ++y)
                    {
                        // We need to extend from 1bpp to 8bpp
                        var data = bitmapData.Scan0 + bitmapData.Stride * (coordinate.Y + y) + coordinate.X / 8;
                        var b = Marshal.ReadByte(data);
                        for (int i = 0; i < 8; ++i)
                        {
                            tileData[y * 8 + i] = (byte)((b >> (7 - i)) & 0b1);
                        }
                    }
                    break;
                default:
                    throw new AppException($"Unsupported bitmap format {bitmapData.PixelFormat}");
            }

            return tileData;
        }

        private IEnumerable<Point> GetTileCoordinates(int width, int height)
        {
            // We generate the tile coordinates in row-major order, optionally with "double height sprite" ordering
            if (_adjacentBelow)
            {
                if (_bitmap.Height % 16 != 0)
                {
                    throw new AppException($"Image's height ({_bitmap.Height}) is not a multiple of 16");
                }

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
                throw new AppException("Image is not paletted. You must provide a 4- or 8-bit paletted image.");
            }

            // We want to find the highest index used in the data
            // We do that by getting the tiles...
            GetTiles();
            var highestIndexUsed = _tiles.SelectMany(tile => tile.Indices).Max();
            if (highestIndexUsed > 15)
            {
                var numIndicesUsed = _tiles.SelectMany(tile => tile.Indices).Distinct().Count();
                throw new AppException($"Image uses colours up to index {highestIndexUsed} - this must be no more than 15. There are {numIndicesUsed} palette entries used.");
            }

            var paletteEntries = _bitmap.Palette.Entries.ToList();

            if (_fullPalette)
            {
                // Extend to 16 if smaller
                if (paletteEntries.Count < 16)
                {
                    Log("Extending palette to 16 entries", LogLevel.Verbose);
                    paletteEntries.AddRange(Enumerable.Repeat(Color.Black, 16 - paletteEntries.Count));
                }
            }
            else if (paletteEntries.Count > highestIndexUsed + 1)
            {
                Log($"Truncating palette to {highestIndexUsed} entries", LogLevel.Verbose);
                paletteEntries.RemoveRange(highestIndexUsed + 1, paletteEntries.Count - (highestIndexUsed + 1));
            }

            _palette = new Palette(paletteEntries);
        }

        private void Log(string message, LogLevel level = LogLevel.Normal)
        {
            _logger(message, level);
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
