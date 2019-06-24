using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BMP2Tile
{
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

        private readonly Entry[,] _tilemap;

        public IEnumerator<Entry> GetEnumerator()
        {
            return _tilemap.Cast<Entry>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tilemap.GetEnumerator();
        }
    }
}