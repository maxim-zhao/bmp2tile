using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace BMP2Tile;

public class Converter: IDisposable
{
    #region Fields

    private readonly Action<string, LogLevel> _logger;
    private Palette _palette;
    private List<Tile> _tiles;
    private Tilemap _tilemap;
    private readonly Dictionary<string, ICompressorImpl> _compressors = [];

    private bool _removeDuplicates = true;
    private bool _useMirroring = true;
    private bool _adjacentBelow;
    private uint _tileOffset;
    private bool _highPriority;
    private bool _useSpritePalette;
    private bool _fullPalette;
    private string _filename;
    private string _tilesFilename;
    private string _tilemapFilename;
    private Bitmap _bitmap;
    private bool _optimized;
    private readonly ICompressorImpl _includeTextWriter = new IncludeTextWriter();
    private readonly HashSet<byte> _paletteIndicesUsed = [];
    private int _spriteWidth;
    private int _spriteHeight;
    private (int left, int top, int width, int height) _tilemapCrop;
    private int _firstTileReplacement = -1;
    private readonly Dictionary<int, Color> _paletteOverrides = [];
    private readonly HashSet<int> _excludedIndices = [];

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
            _tiles = null;
            _tilemap = null;
        }
    }

    public bool UseSpritePalette
    {
        set
        {
            _useSpritePalette = value;
            _tilemap = null;
            _tiles = null;
        }
    }

    public bool HighPriority
    {
        set
        {
            _highPriority = value;
            _tilemap = null;
            _tiles = null;
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

    public string RawTilesFilename
    {
        set
        {
            _tilesFilename = value;
            _filename = null;
            _tiles = null;

            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }
    }

    public string RawTilemapFilename
    {
        set
        {
            _tilemapFilename = value;
            _filename = null;
            _tilemap = null;

            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }
    }

    public Palette.Formats PaletteFormat { private get; set; }
    public string DllPath { private get; set; } = AppContext.BaseDirectory;

    #endregion

    #region Public methods

    public void SaveTiles(string filename)
    {
        var tilesToSave = GetProcessedTiles();

        Log("Saving tiles...", LogLevel.Verbose);

        var compressor = GetCompressor(filename);
        var sw = Stopwatch.StartNew();
        var compressed = compressor.CompressTiles(tilesToSave, Chunky).ToArray();
        File.WriteAllBytes(filename, compressed);
        sw.Stop();

        var before = tilesToSave.Count * 32;
        var ratio = (before - compressed.Length) / (double)before;

        Log($"Saved tiles in format \"{compressor.Name}\" to {filename} in {sw.Elapsed}. Compression ratio {ratio:P}");
    }

    private List<Tile> GetProcessedTiles()
    {
        GetTiles();
        if (_removeDuplicates)
        {
            Optimize();
        }

        // Duplicate the list so we can manipulate it without losing data
        var tilesToSave = new List<Tile>(_tiles);

        if (_firstTileReplacement > -1)
        {
            tilesToSave.RemoveAt(0);
        }

        return tilesToSave;
    }

    public string GetTilesAsText()
    {
        return Encoding.UTF8.GetString(_includeTextWriter.CompressTiles(GetProcessedTiles(), Chunky).ToArray());
    }

    public void SaveTilemap(string filename)
    {
        var tilemap = GetProcessedTilemap();

        Log("Compressing tilemap...", LogLevel.Verbose);
        var compressor = GetCompressor(filename);
        var sw = Stopwatch.StartNew();
        var compressed = compressor.CompressTilemap(tilemap).ToArray();
        File.WriteAllBytes(filename, compressed);
        sw.Stop();

        var before = tilemap.Height * tilemap.Width * 2;
        var ratio = (before - compressed.Length) / (double)before;

        Log($"Saved tilemap in format \"{compressor.Name}\" to {filename} in {sw.Elapsed}. Compression ratio {ratio:P}");
    }

    private Tilemap GetProcessedTilemap()
    {
        GetTilemap();
        if (_removeDuplicates)
        {
            Optimize();
        }

        // Clone the tilemap so we can alter it non-destructively
        var tilemap = _tilemap.Clone();

        if (_tilemapCrop.width > 0)
        {
            Log($"Cropping tilemap to {_tilemapCrop.width}x{tilemap.Height} at {_tilemapCrop.left}, {_tilemapCrop.top}");
            tilemap.Crop(
                _tilemapCrop.left / 8, 
                _tilemapCrop.top / 8, 
                _tilemapCrop.width / 8, 
                _tilemapCrop.height / 8);
        }

        if (_firstTileReplacement > -1)
        {
            // Change all entries using the first tile to use the replacement instead
            foreach (var entry in tilemap)
            {
                if (entry.TileIndex == _tileOffset)
                {
                    entry.TileIndex = _firstTileReplacement;
                }
                else
                {
                    // All other entries need to use an index decremented by 1
                    --entry.TileIndex;
                }
            }
        }

        return tilemap;
    }

    public string GetTilemapAsText()
    {
        return Encoding.UTF8.GetString(_includeTextWriter.CompressTilemap(GetProcessedTilemap()).ToArray());
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

    public string GetPaletteAsText()
    {
        GetPalette();
        return _palette.ToString(PaletteFormat);
    }

    public List<List<Color>> GetPalettes()
    {
        GetPalette();
        var convertedPalette = _palette.ForDisplay(PaletteFormat);
        return
        [
            _bitmap.Palette.Entries.Take(convertedPalette.Count).ToList(),
            convertedPalette
        ];
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
        Log($"Looking for compressors in {DllPath}...", LogLevel.Verbose);

        // ReSharper disable once StringLiteralTypo
        foreach (var filename in Directory.EnumerateFiles(DllPath, "gfxcomp_*.dll"))
        {
            try
            {
                var compressor = new CompressionDllWrapper(filename);
                if (compressor.Capabilities == CompressorCapabilities.None)
                {
                    Log($"Loaded {filename} but found no compressors");
                    compressor.Dispose();
                    continue;
                }

                var key = "." + compressor.Extension.ToLowerInvariant();
                if (!_compressors.TryAdd(key, compressor))
                {
                    compressor.Dispose();
                    throw new AppException($"Compressor {_compressors[key].Name} already registered for extension {key}");
                }

                Log($"Added \"{compressor.Name}\" ({compressor.Extension}) from {filename}", LogLevel.Verbose);
            }
            catch (Exception ex)
            {
                Log($"Failed to load {filename}: {ex.Message}");
            }
        }

        Log($"Added {_compressors.Count} compressors", LogLevel.Verbose);
    }

    private void GetBitmap()
    {
        if (_bitmap != null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_filename))
        {
            throw new AppException("Filename has not been specified");
        }

        try
        {
            Log($"Loading {_filename}...");

            if (_filename.EndsWith(".bin"))
            {
                // Raw tiles
                // We convert to a tall bitmap
                var data = File.ReadAllBytes(_filename);
                if (data.Length % 32 != 0)
                {
                    throw new Exception($"{_filename} is {data.Length} bytes, not a multiple of 32!");
                }

                var height = data.Length / 32 * 8;
                _bitmap = new Bitmap(8, height, PixelFormat.Format8bppIndexed);
                var bitmapData = _bitmap.LockBits(
                    new Rectangle(0, 0, 8, height), 
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format8bppIndexed);
                // Convert the planar data into a chunky bitmap...
                var row = new byte[4];
                var chunky = new byte[8];
                for (var y = 0; y < height; ++y)
                {
                    // Get the data for a row of pixels
                    Array.Copy(data, y*4, row, 0, 4);
                    // Convert to chunky
                    for (var x = 0; x < 8; ++x)
                    {
                        var pixel = 0;
                        var bitPosition = 7 - x;
                        for (var b = 0; b < 4; ++b)
                        {
                            // Get bit from bitplane
                            var bitplane = row[b];
                            // Mask
                            var bit = (bitplane >> bitPosition) & 1;
                            // Merge into pixel at right place
                            pixel |= bit << b;
                        }
                        // Then set it
                        chunky[x] = (byte)pixel;
                    }
                    // Then copy to the image
                    Marshal.Copy(chunky, 0, bitmapData.Scan0 + bitmapData.Stride * y, 8);
                }
                _bitmap.UnlockBits(bitmapData);
                _bitmap.Palette.Entries[0] = Color.Red;
                _bitmap.Palette.Entries[1] = Color.White;
            }
            else
            {
                // Loading this way avoids locking the file
                var converter = new ImageConverter();
                _bitmap = (Bitmap) converter.ConvertFrom(File.ReadAllBytes(_filename));
            }

            if (_bitmap == null)
            {
                throw new AppException($"Failed to load \"{_filename}\"");
            }

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

            if (_spriteWidth > 0 && _spriteHeight > 0)
            {
                RearrangeSpriteSheet();
            }
        }
        catch (AppException)
        {
            _bitmap = null;
            throw;
        }
    }

    private void Optimize()
    {
        if (_optimized || !_removeDuplicates)
        {
            return;
        }

        // If we are in tilemap-only or tiles-only mode then we can't optimize
        if ((_tilemapFilename != null && _tilesFilename == null) ||
            (_tilemapFilename == null && _tilesFilename != null))
        {
            Log("Skipping optimize because we do not have both tiles and tilemap", LogLevel.Verbose);
            return;
        }

        GetTiles();
        GetTilemap();

        Log("Optimizing...", LogLevel.Verbose);

        var tileCountBefore = _tiles.Count;

        // We pass through the tiles and clear out any duplicates
        for (var i = 0; i < _tiles.Count; ++i)
        {
            // Compare tile i to the ones following it
            // Replace duplicates with this one
            var thisTile = _tiles[i];
            var thisTileIndex = (int)(i + _tileOffset);
            for (var j = i + 1; j < _tiles.Count; /* increment in loop */)
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

        if (_tilemapFilename != null)
        {
            GetTilemapFromFile();
        }
        else if (_filename != null)
        {
            GetTilemapFromBitmap();
        }
        else
        {
            throw new AppException("No tilemap can be created because there's no filename");
        }

        if (_tilemap == null)
        {
            throw new AppException("No tilemap could be generated");
        }
    }

    public void GetTilemapFromFile()
    {
        var filename = _tilemapFilename;
        var width = 1;
        // Filename might be a filename, or might be <filename>:<width>, where <width> is the tilemap width
        var m = Regex.Match(filename, "^(?<filename>.+):(?<width>\\d+)$");
        if (m.Success)
        {
            filename = m.Groups["filename"].Value;
            width = Convert.ToInt32(m.Groups["width"].Value);
        }

        Log($"Loading {filename}...");
        var data = File.ReadAllBytes(filename);
        if (data.Length % 2 != 0)
        {
            throw new AppException($"File \"{filename}\" is not a multiple of 2 bytes cannot be loaded as a tilemap");
        }

        var numEntries = data.Length / 2;
        var height = numEntries / width;
        // Sanity-check the width
        if (width * height != numEntries)
        {
            throw new AppException($"File \"{filename}\" is not a multiple of {width * 2} bytes so it cannot have a width of {width}");
        }
        _tilemap = new Tilemap(width, height);
        for (var x = 0; x < width; ++x)
        for (var y = 0; y < height; ++y)
        {
            // Get the two bytes
            var offset = (y * width + x) * 2;
            var word = data[offset] | (data[offset + 1] << 8);
            // Put into the tilemap
            _tilemap[x, y] = new Tilemap.Entry
            {
                TileIndex =         word & 0b0_0001_1111_1111,
                HFlip =            (word & 0b0_0010_0000_0000) != 0,
                VFlip =            (word & 0b0_0100_0000_0000) != 0,
                UseSpritePalette = (word & 0b0_1000_0000_0000) != 0,
                HighPriority =     (word & 0b1_0000_0000_0000) != 0
            };
        }
    }

    public void GetTilemapFromBitmap()
    {
        GetBitmap();
        GetTiles();

        Log("Creating tilemap", LogLevel.Verbose);

        // This is an unoptimised tilemap
        // So we just need to fill space
        var tilemap = new Tilemap(_bitmap.Width / 8, _bitmap.Height / 8);

        var i = (int)_tileOffset;
        // We fill space by using the same function used to extract the tiles
        foreach (var point in GetTileCoordinates(_bitmap.Width, _bitmap.Height))
        {
            // Skip excluded indices
            while (_excludedIndices.Contains(i))
            {
                ++i;
            }
            tilemap[point.X / 8, point.Y / 8] = new Tilemap.Entry
            {
                TileIndex = i,
                HFlip = false,
                VFlip = false,
                HighPriority = _highPriority,
                UseSpritePalette = _useSpritePalette || _tiles[i - (int)_tileOffset].UseSpritePalette
            };
            ++i;
        }

        _tilemap = tilemap;

        Log($"Created {_tilemap.Width}x{tilemap.Height} tilemap", LogLevel.Verbose);
    }

    private void GetTiles()
    {
        if (_tiles != null)
        {
            return;
        }

        if (_tilesFilename != null)
        {
            GetTilesFromFile();
        }
        else if (_filename != null)
        {
            GetTilesFromBitmap();
        }
        else
        {
            throw new AppException("No tiles can be created because there's no filename");
        }
    }

    private void GetTilesFromFile()
    {
        Log($"Reading tiles from {_tilesFilename}", LogLevel.Verbose);
        var data = File.ReadAllBytes(_tilesFilename);
        if (data.Length % 32 != 0)
        {
            throw new AppException($"File \"{_tilesFilename}\" is not a multiple of 32 bytes cannot be loaded as tiles");
        }

        _tiles = [];
        // We need to convert the data to 8x8 arrays of 4-bit numbers
        for (var tileOffset = 0; tileOffset < data.Length; tileOffset += 32)
        {
            // Tiles are 64 bytes of tile indices - not planar.
            // The file data is planar so we have to convert it.
            // This is ugly...
            var tile = new byte[8 * 8];

            for (var row = 0; row < 8; ++row)
            {
                // Point to the data for this row
                var offset = tileOffset + row * 4;
                // Iterate over the bitplanes
                for (var bitplane = 0; bitplane < 4; ++bitplane)
                {
                    // Iterate over the pixels
                    for (var x = 0; x < 8; ++x)
                    {
                        // Get the bit
                        var bit = (data[offset + bitplane] >> (7 - x)) & 1;
                        // Merge into the value
                        tile[row * 8 + x] |= (byte)(bit << bitplane);
                    }
                }
            }

            _tiles.Add(new Tile(tile));
        }
    }

    private void GetTilesFromBitmap()
    {
        GetBitmap();

        Log("Generating tiles from image", LogLevel.Verbose);

        BitmapData bitmapData = null;
        try
        {
            // In Windows, we can just specify 8bpp here and GDI+ extends 4bpp images for us.
            // However, this is not implemented in Wine so we convert it ourselves.
            bitmapData = _bitmap.LockBits(
                new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
                ImageLockMode.ReadOnly,
                _bitmap.PixelFormat);

            // We want to find some info about the palette as we go here, so we reset the values now.
            _paletteIndicesUsed.Clear();

            // We want to split the image to 8x8 chunks in the required order
            _tiles = GetTileCoordinates(_bitmap.Width, _bitmap.Height)
                .Select(coordinate => GetTile(coordinate, bitmapData))
                .ToList();

            _optimized = false;

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

    private Tile GetTile(Point coordinate, BitmapData bitmapData)
    {
        var tileData = new byte[8 * 8];

        switch (bitmapData.PixelFormat)
        {
            case PixelFormat.Format8bppIndexed:
                for (var y = 0; y < 8; ++y)
                {
                    // Copy data 8 pixels at a time
                    Marshal.Copy(
                        bitmapData.Scan0 + bitmapData.Stride * (coordinate.Y + y) + coordinate.X,
                        tileData,
                        y * 8,
                        8);
                }

                // Then we sanity-check the range
                switch (tileData.Max())
                {
                    case > 31:
                        throw new AppException("Image uses more than the first 32 palette indices");
                    case > 15:
                        SelectTilePalette(tileData);
                        break;
                }

                break;
            case PixelFormat.Format4bppIndexed:
                for (var y = 0; y < 8; ++y)
                {
                    // We need to extend from 4bpp to 8bpp
                    var data = bitmapData.Scan0 + bitmapData.Stride * (coordinate.Y + y) + coordinate.X / 2;
                    for (var i = 0; i < 4; ++i)
                    {
                        var b = Marshal.ReadByte(data, i);
                        tileData[y * 8 + i * 2 + 0] = (byte)((b >> 4) & 0xf);
                        tileData[y * 8 + i * 2 + 1] = (byte)((b >> 0) & 0xf);
                    }
                }
                break;
            case PixelFormat.Format1bppIndexed:
                for (var y = 0; y < 8; ++y)
                {
                    // We need to extend from 1bpp to 8bpp
                    var data = bitmapData.Scan0 + bitmapData.Stride * (coordinate.Y + y) + coordinate.X / 8;
                    var b = Marshal.ReadByte(data);
                    for (var i = 0; i < 8; ++i)
                    {
                        tileData[y * 8 + i] = (byte)((b >> (7 - i)) & 0b1);
                    }
                }
                break;
            default:
                throw new AppException($"Unsupported bitmap format {bitmapData.PixelFormat}");
        }

        _paletteIndicesUsed.UnionWith(tileData);
        return new Tile(tileData);
    }

    private void SelectTilePalette(IList<byte> tileData)
    {
        // If all indices are high, we are good
        if (tileData.Min() > 15)
        {
            return;
        }

        // Else we try to remap to either the low or high palette.
        var palettes = _bitmap.Palette.Entries.Take(32).ToList();
        if (RestrictTilePalette(tileData, palettes, 16, 31))
        {
            // It fits in the high 16, so we return true to indicate to use the sprite palette
            return;
        }

        if (RestrictTilePalette(tileData, palettes, 0, 15))
        {
            // It fits in the low 16, so we return false to indicate not to use the sprite palette
            return;
        }

        // Else it's a failure
        throw new AppException("Image uses colors from both palettes");
    }

    private static bool RestrictTilePalette(IList<byte> tileData, List<Color> palette, int minimumIndex, int maximumIndex)
    {
        var range = Math.Min(maximumIndex, palette.Count - 1) - minimumIndex + 1;
        for (var i = 0; i < tileData.Count; i++)
        {
            var b = tileData[i];
            if (b < minimumIndex || b > maximumIndex)
            {
                // Try to remap to a color in the preferred range
                var preferredIndex = palette.IndexOf(palette[b], minimumIndex, range);
                if (preferredIndex == -1)
                {
                    // Failed to find a match
                    return false;
                }

                b = (byte)preferredIndex;
            }
            tileData[i] = b;
        }

        // Success if we get to the end
        return true;
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

            for (var y = 0; y < height; y += 16)
            for (var x = 0; x < width; x += 8)
            {
                yield return new Point(x, y);
                yield return new Point(x, y+8);
            }
        }
        else
        {
            for (var y = 0; y < height; y += 8)
            for (var x = 0; x < width; x += 8)
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
        if (_bitmap.Palette == null || _bitmap.Palette.Entries.Length == 0)
        {
            throw new AppException("Image is not paletted. You must provide a 4- or 8-bit paletted image.");
        }

        // We want to find the highest index used in the data
        // We do that by getting the tiles, as this also "corrects" mixed-palette-range tiles.
        GetTiles();
        var highestIndexUsed = _paletteIndicesUsed.Max();
        if (highestIndexUsed > 31)
        {
            throw new AppException($"Image uses colors up to index {highestIndexUsed} - this must be no more than 31 (0-based). There are {_paletteIndicesUsed.Count} palette entries used.");
        }

        var paletteEntries = _bitmap.Palette.Entries.ToList();

        if (_fullPalette)
        {
            // Extend if smaller, truncate if larger
            var requiredSize = highestIndexUsed > 15 ? 32 : 16;
            if (paletteEntries.Count < requiredSize)
            {
                Log($"Extending palette to {requiredSize} entries", LogLevel.Verbose);
                paletteEntries.AddRange(Enumerable.Repeat(Color.Black, requiredSize - paletteEntries.Count));
            }
            else
            {
                paletteEntries.RemoveRange(requiredSize, paletteEntries.Count - requiredSize);
            }
        }
        else if (paletteEntries.Count > highestIndexUsed + 1)
        {
            Log($"Truncating palette to {highestIndexUsed} entries", LogLevel.Verbose);
            paletteEntries.RemoveRange(highestIndexUsed + 1, paletteEntries.Count - (highestIndexUsed + 1));
        }

        // Finally, apply our replacements
        foreach (var kvp in _paletteOverrides.Where(x => x.Key < paletteEntries.Count))
        {
            paletteEntries[kvp.Key] = kvp.Value;
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

    public void SpriteSheet(int spriteWidth, int spriteHeight)
    {
        if (spriteWidth <= 0 || spriteWidth % 8 != 0)
        {
            throw new AppException($"Sprite width {spriteWidth} must be a multiple of 8");
        }
        if (spriteHeight <= 0 || spriteHeight % 8 != 0)
        {
            throw new AppException($"Sprite height {spriteHeight} must be a multiple of 8");
        }

        _spriteWidth = spriteWidth;
        _spriteHeight = spriteHeight;

        _tiles = null;
        _tilemap = null;
    }

    public void RearrangeSpriteSheet()
    {
        GetBitmap();

        // Compare to the image dimensions
        if (_bitmap.Width % _spriteWidth != 0 || _bitmap.Height % _spriteHeight != 0)
        {
            throw new AppException($"Image is not a multiple of {_spriteWidth}x{_spriteHeight} pixels, cannot split");
        }

        Log("Rearranging image as a sprite sheet...", LogLevel.Verbose);

        var sourceColumns = _bitmap.Width / _spriteWidth;
        var sourceRows = _bitmap.Height / _spriteHeight;
        // Figure out the new image size and make it
        var numSprites = sourceRows * sourceColumns;
        Log($"New image is {_spriteWidth}x{numSprites * _spriteHeight}", LogLevel.Verbose);
        var newImage = new Bitmap(_spriteWidth, numSprites * _spriteHeight, _bitmap.PixelFormat);

        // Transfer the data across
        BitmapData oldBitmapData = null;
        BitmapData newBitmapData = null;
        try
        {
            oldBitmapData = _bitmap.LockBits(
                new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
                ImageLockMode.ReadOnly,
                _bitmap.PixelFormat);
            newBitmapData = newImage.LockBits(
                new Rectangle(0, 0, newImage.Width, newImage.Height),
                ImageLockMode.ReadWrite,
                newImage.PixelFormat);

            // Copy from the old one
            for (var i = 0; i < numSprites; ++i)
            {
                // Figure out the source rect
                var sourceRow = i / sourceColumns;
                var sourceColumn = i % sourceColumns;
                // Copy it to the new image
                CopyBitmap(
                    oldBitmapData,
                    newBitmapData,
                    0,
                    i * _spriteHeight,
                    sourceColumn * _spriteWidth,
                    sourceRow * _spriteHeight,
                    _spriteWidth,
                    _spriteHeight);
                Log($"Copied sprite {i+1}/{numSprites}", LogLevel.Verbose);
            }
            Log($"Rearranged image from {_bitmap.Width}x{_bitmap.Height} with {numSprites} {_spriteWidth}x{_spriteHeight} sprites to {newImage.Width}x{newImage.Height}");
        }
        finally
        {
            if (oldBitmapData != null)
            {
                _bitmap.UnlockBits(oldBitmapData);
            }
            if (newBitmapData != null)
            {
                newImage.UnlockBits(newBitmapData);
            }
        }
        // Copy the palette too
        newImage.Palette = _bitmap.Palette;

        // Finally, swap the new image in and dispose the old one
        _bitmap.Dispose();
        _bitmap = newImage;
    }

    private static void CopyBitmap(BitmapData source, BitmapData dest, int destX, int destY, int sourceX, int sourceY, int width, int height)
    {
        var sourceOffset = source.Scan0 + sourceY * source.Stride + sourceX;
        var destOffset = dest.Scan0 + destY * dest.Stride + destX;
        var tempArray = new byte[width];
        var bytes = dest.PixelFormat switch
        {
            PixelFormat.Format8bppIndexed => width,
            PixelFormat.Format4bppIndexed => width / 2,
            PixelFormat.Format1bppIndexed => width / 8,
            _ => throw new AppException($"Can't handle non-paletted images. Image is {dest.PixelFormat}")
        };
        for (var i = 0; i < height; ++i)
        {
            Marshal.Copy(sourceOffset, tempArray, 0, bytes);
            Marshal.Copy(tempArray, 0, destOffset, bytes);
            sourceOffset += source.Stride;
            destOffset += dest.Stride;
        }
    }

    public void CropTo(int left, int top, int width, int height)
    {
        if (left % 8 != 0 ||
            top % 8 != 0 ||
            width % 8 != 0 ||
            height % 8 != 0)
        {
            throw new ArgumentException("Crop area must be multiples of 8");
        }

        _tilemapCrop = (left, top, width, height);
        _tilemap = null;
    }

    public void ReplaceFirstTileWith(int index)
    {
        _firstTileReplacement = index;
        _tiles = null;
        _tilemap = null;
    }

    public void ClearPaletteOverrides()
    {
        _paletteOverrides.Clear();
        _palette = null;
    }

    public void AddPaletteOverride(int index, Color colour)
    {
        _paletteOverrides[index] = colour;
        _palette = null;
    }

    public void ExcludeTileIndex(int index)
    {
        _excludedIndices.Add(index);
    }
}