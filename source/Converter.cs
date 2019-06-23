using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BMP2Tile
{
    internal class Tilemap : IEnumerable<TilemapEntry>
    {
        public int Width { get; }
        public int Height { get; }

        public Tilemap(int width, int height)
        {
            Width = width;
            Height = height;
            _tilemap = new TilemapEntry[width, height];
        }

        public TilemapEntry this[int x, int y]
        {
            get => _tilemap[x, y];
            set => _tilemap[x, y] = value;
        }

        private readonly TilemapEntry[,] _tilemap;

        public IEnumerator<TilemapEntry> GetEnumerator()
        {
            return _tilemap.Cast<TilemapEntry>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tilemap.GetEnumerator();
        }
    }

    internal class Converter: IDisposable
    {
        private readonly Action<string> _logger;
        private IEnumerable<byte> _palette;
        private List<Tile> _tiles;
        private Tilemap _tilemap;
        private readonly Dictionary<string, ICompressor> _compressors = new Dictionary<string, ICompressor>();

        public Converter(Action<string> logger)
        {
            _logger = logger;
        }

        public bool RemoveDuplicates { private get; set; } = true;
        public bool UseMirroring { private get; set; } = true;
        public bool AdjacentBelow { private get; set; }
        public bool Chunky { get; set; } = false;
        public uint TileOffset { private get; set; }
        public bool UseSpritePalette { private get; set; }
        public bool HighPriority { private get; set; }
        public bool FullPalette { get; set; }
        public string Filename { private get; set; }

        public enum PaletteFormats
        {
            SMS,
            GG
        }

        public PaletteFormats PaletteFormat { private get; set; }

        public void SaveTiles(string filename)
        {
            Process();
            var compressor = GetCompressor(filename);
            _logger($"Saving tiles in format \"{compressor.Name}\" to \"{filename}\"...");
            var bytes = compressor.CompressTiles(_tiles, Chunky);
            File.WriteAllBytes(filename, bytes.ToArray());
        }

        private ICompressor GetCompressor(string filename)
        {
            GetCompressors();
            var extension = Path.GetExtension(filename);
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

            _logger("Loading compressors...");

            // We add some C#-based ones...
            _compressors.Add(".inc", new IncludeTextWriter());

            // We enumerate files in the program folder
            var path = AppContext.BaseDirectory;

            foreach (var filename in Directory.EnumerateFiles(path, "gfxcomp_*.dll"))
            {
                try
                {
                    var compressor = new CompressionDllWrapper(filename);
                    if (compressor.Capabilities == CompressorCapabilities.None)
                    {
                        _logger($"Loaded {filename} but found no compressors");
                        continue;
                    }
                    _compressors["." + compressor.Extension] = compressor;
                }
                catch (Exception ex)
                {
                    _logger($"Failed to load {filename}: {ex.Message}");
                }
            }
        }

        public void SaveTilemap(string filename)
        {
            Process();
            var compressor = GetCompressor(filename);
            _logger($"Saving tilemap in format \"{compressor.Name}\" to \"{filename}\"...");
            var bytes = compressor.CompressTilemap(_tilemap);
            File.WriteAllBytes(filename, bytes.ToArray());
        }

        public void SavePalette(string filename)
        {
            Process();
            throw new NotImplementedException();
        }

        private void Process()
        {
            // Load the image
            using (var bm = new Bitmap(Filename))
            {
                // Check the dimensions
                if (bm.Width % 8 != 0)
                {
                    throw new Exception($"Image's width is not a multiple of 8: {bm.Width}");
                }

                if (AdjacentBelow)
                {
                    if (bm.Height % 16 != 0)
                    {
                        throw new Exception($"Image's height is not a multiple of 16: {bm.Height}");
                    }
                }
                else
                {
                    if (bm.Height % 8 != 0)
                    {
                        throw new Exception($"Image's height is not a multiple of 8: {bm.Height}");
                    }
                }

                _logger("Converting palette...");
                _palette = GetPalette(bm);

                _logger("Converting tiles...");
                _tiles = GetTiles(bm);

                // We check if any palette indices are too high
                var usedIndices = new HashSet<byte>(_tiles.SelectMany(t => t.Data));
                if (usedIndices.Max() > 15)
                {
                    throw new Exception($"Image has too many colours - palette entries used at indices {string.Join(", ", usedIndices.OrderBy(x => x))}");
                }

                _logger("Building tilemap...");
                _tilemap = GetTilemap(bm);

                if (RemoveDuplicates)
                {
                    _logger("Removing duplicates...");
                    var tileCountBefore = _tiles.Count;
                    Optimize();
                    _logger($"Reduced from {tileCountBefore} to {_tiles.Count} tiles");
                }
                /*
                // Debugging
                Console.Out.WriteLine($"Palette: {string.Join(" ", _palette.Select(b => b.ToString("X2")))}");
                Console.Out.WriteLine("Tiles:");
                foreach (var tile in _tiles)
                {
                    int i = 0;
                    foreach (var b in tile.GetValue(Chunky))
                    {
                        Console.Out.Write($"{b:X2} ");
                        if (++i % 8 == 0)
                        {
                            Console.Out.WriteLine();
                        }
                    }
                }
                Console.Out.WriteLine("Tilemap:");
                {
                    var width = _tilemap.GetLength(0);
                    var i = 0;
                    foreach (var tilemapEntry in _tilemap)
                    {
                        Console.Out.Write($"{tilemapEntry.GetValue():x4} ");
                        if (i++ % width == 0)
                        {
                            Console.Out.WriteLine();
                        }
                    }
                }
                */
            }
        }

        private void Optimize()
        {
            // We pass through the tiles and clear out any duplicates
            for (int i = 0; i < _tiles.Count; ++i)
            {
                // Compare tile i to the ones following it
                // Replace duplicates with this one
                var thisTile = _tiles[i];
                for (int j = i + 1; j < _tiles.Count; /* increment in loop */)
                {
                    var comparedTile = _tiles[j];
                    var comparison = CompareTile(thisTile, comparedTile);
                    if (comparison == TileMatch.NoMatch)
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
                                case TileMatch.HFlip:
                                    entry.HFlip = true;
                                    break;
                                case TileMatch.VFlip:
                                    entry.VFlip = true;
                                    break;
                                case TileMatch.BothFlip:
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
        }

        enum TileMatch
        {
            Identical,
            HFlip,
            VFlip,
            BothFlip,
            NoMatch
        }

        private TileMatch CompareTile(Tile reference, Tile candidate)
        {
            if (reference.Data.SequenceEqual(candidate.Data))
            {
                return TileMatch.Identical;
            }

            if (UseMirroring)
            {
                // Copy the data out so we can manipulate it
                var data = new byte[8 * 8];

                // First flip horizontally
                for (int y = 0; y < 8; ++y)
                {
                    for (int x = 0; x < 8; ++x)
                    {
                        data[y * 8 + x] = candidate.Data[y * 8 + (7 - x)];
                    }
                }

                if (reference.Data.SequenceEqual(data))
                {
                    return TileMatch.HFlip;
                }

                // Next flip vertically
                for (int y = 0; y < 8; ++y)
                {
                    for (int x = 0; x < 8; ++x)
                    {
                        data[y * 8 + x] = candidate.Data[(7 - y) * 8 + x];
                    }
                }

                if (reference.Data.SequenceEqual(data))
                {
                    return TileMatch.VFlip;
                }


                // Next flip both horizontally and vertically
                for (int y = 0; y < 8; ++y)
                {
                    for (int x = 0; x < 8; ++x)
                    {
                        data[y * 8 + x] = candidate.Data[(7 - y) * 8 + (7 - x)];
                    }
                }

                if (reference.Data.SequenceEqual(data))
                {
                    return TileMatch.BothFlip;
                }
            }

            return TileMatch.NoMatch;
        }

        private Tilemap GetTilemap(Image bm)
        {
            // This is an unoptimised tilemap
            // So we just need to fill space
            var result = new Tilemap(bm.Width / 8, bm.Height / 8);

            var i = (int)TileOffset;
            // We fill space by using the same function used to extract the tiles
            foreach (var point in GetTileCoordinates(bm.Width, bm.Height))
            {
                result[point.X / 8, point.Y / 8] = new TilemapEntry
                {
                    TileIndex = i++,
                    HFlip = false,
                    VFlip = false,
                    HighPriority = HighPriority,
                    UseSpritePalette = UseSpritePalette
                };
            }

            return result;
        }

        private List<Tile> GetTiles(Bitmap bm)
        {
            var data = bm.LockBits(
                new Rectangle(0, 0, bm.Width, bm.Height), 
                ImageLockMode.ReadOnly,
                PixelFormat.Format8bppIndexed); // TODO is this converting?

            var tiles = new List<Tile>();

            // We want to split the image to 8x8 chunks in the required order
            foreach (var coordinate in GetTileCoordinates(bm.Width, bm.Height))
            {
                var result = new Tile
                {
                    Data = new byte[8 * 8]
                };
                for (int y = 0; y < 8; ++y)
                {
                    // Copy data 8 pixels at a time
                    Marshal.Copy(
                        data.Scan0 + data.Stride * (coordinate.Y + y) + coordinate.X,
                        result.Data,
                        y * 8,
                        8);
                }
                tiles.Add(result);
            }

            bm.UnlockBits(data);

            return tiles;
        }

        private IEnumerable<Point> GetTileCoordinates(int width, int height)
        {
            // We generate the tile coordinates in row-major order, optionally with "double height sprite" ordering
            if (AdjacentBelow)
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

        private IEnumerable<byte> GetPalette(Image image)
        {
            // First we read out the palette
            // TODO check what we get on non-paletted images - I'm assuming null
            if (image.Palette == null)
            {
                throw new Exception("Image is not paletted. You must provide a 4- or 8-bit paletted image.");
            }

            if (image.Palette.Entries.Length > 16)
            {
                throw new Exception($"Image palette has {image.Palette.Entries.Length} entries, must be <= 17");
            }

            switch (PaletteFormat)
            {
                case PaletteFormats.SMS:
                    // We truncate to 6bpp
                    return image.Palette.Entries.Select(ToSMSColour);
                case PaletteFormats.GG:
                    // We truncate to 12bpp
                    return image.Palette.Entries.SelectMany(c =>
                    {
                        var colour = ToGGColour(c);
                        return new []{(byte) colour, (byte) (colour >> 8)};
                    });
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private byte ToSMSColour(Color c)
        {
            // Some typical colours used for SMS palettes include...
            // eSMS = 0, 57, 123, 189
            // Meka = 0, 85, 170, 255
            //     or 0, 65, 130, 195
            // So we pick some boundaries to flatten that
            var toSms = new Func<byte, int>(b => b < 56 ? 0 : b < 122 ? 1 : b < 188 ? 2 : 3);

            return (byte) ((toSms(c.B) << 4) | (toSms(c.G) << 2) | toSms(c.R));
        }

        private short ToGGColour(Color c)
        {
            // We just truncate to 4 bits per channel
            var ToGg = new Func<byte, int>(b => (byte)(b & 0xf));

            return (short) ((ToGg(c.B) << 8) | (ToGg(c.G) << 4) | ToGg(c.R));
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

    internal class IncludeTextWriter : ICompressor
    {
        public void Dispose()
        {
            // Nothing to do
        }

        public string Extension => "inc";
        public string Name => "Include file";
        public CompressorCapabilities Capabilities => CompressorCapabilities.Tiles | CompressorCapabilities.Tilemap;
        public IEnumerable<byte> CompressTiles(IList<Tile> tiles, bool asChunky)
        {
            return TextToBytes(TilesToText(tiles, asChunky));
        }

        private IEnumerable<byte> TextToBytes(IEnumerable<string> lines)
        {
            return lines.SelectMany(s => Encoding.ASCII.GetBytes(s + Environment.NewLine));
        }

        private IEnumerable<string> TilesToText(ICollection<Tile> tiles, bool asChunky)
        {
            int index = 0;
            foreach (var rawData in tiles.Select(t => t.GetValue(asChunky)))
            {
                yield return $"; Tile index ${index++:X3}";
                yield return ".db $" + string.Join(" $", rawData.Select(b => b.ToString("X2")));
            }
        }

        public IEnumerable<byte> CompressTilemap(Tilemap tilemap)
        {
            return TextToBytes(TilemapToText(tilemap));
        }

        private IEnumerable<string> TilemapToText(Tilemap tilemap)
        {
            for (var y = 0; y < tilemap.Height; ++y)
            {
                var row = ".dw";
                for (int x = 0; x < tilemap.Width; ++x)
                {
                    row += " $" + tilemap[x, y].GetValue().ToString("X4");
                }
                yield return row;
            }
        }
    }
}