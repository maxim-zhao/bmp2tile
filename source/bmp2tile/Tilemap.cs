using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BMP2Tile;

internal class Tilemap : IEnumerable<Tilemap.Entry>
{
    internal class Entry
    {
        public int TileIndex { get; set; }
        public bool HFlip { get; set; }
        public bool VFlip { get; set; }
        public bool HighPriority { get; set; }
        public bool UseSpritePalette { get; set; }

        public int GetValue()
        {
            var result = TileIndex & 0b111111111;
            if (HFlip)
            {
                result |= 1 << 9;
            }

            if (VFlip)
            {
                result |= 1 << 10;
            }

            if (UseSpritePalette)
            {
                result |= 1 << 11;
            }

            if (HighPriority)
            {
                result |= 1 << 12;
            }

            return result;
        }

        public Entry Clone()
        {
            return new Entry
            {
                HFlip = HFlip,
                HighPriority = HighPriority,
                TileIndex = TileIndex,
                UseSpritePalette = UseSpritePalette,
                VFlip = VFlip
            };
        }
    }

    public int Width { get; }
    public int Height { get; }

    public Tilemap(int width, int height)
    {
        Width = width;
        Height = height;
        _tilemap = new Entry[width, height];
    }

    public Entry this[int x, int y]
    {
        get => _tilemap[x, y];
        set => _tilemap[x, y] = value;
    }

    private Entry[,] _tilemap;

    public IEnumerator<Entry> GetEnumerator()
    {
        return _tilemap.Cast<Entry>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _tilemap.GetEnumerator();
    }

    /// <summary>
    /// Crops to the tile area given
    /// </summary>
    public void Crop(int left, int top, int width, int height)
    {
        var newTilemap = new Entry[width, height];
        for (var y = 0; y < height; ++y)
        for (var x = 0; x < width; ++x)
        {
            newTilemap[x, y] = _tilemap[x + left, y + height];
        }

        _tilemap = newTilemap;
    }

    public Tilemap Clone()
    {
        var result = new Tilemap(Width, Height);
        for (var y = 0; y < Height; ++y)
        for (var x = 0; x < Width; ++x)
        {
            result[x, y] = _tilemap[x, y].Clone();
        }

        return result;
    }
}