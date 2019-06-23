namespace BMP2Tile
{
    internal class TilemapEntry
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
}