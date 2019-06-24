using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BMP2Tile
{
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