using System.Collections.Generic;

namespace BMP2Tile
{
    internal class Tile
    {
        public byte[] Data { get; set; }

        public IEnumerable<byte> GetValue(bool asChunky)
        {
            if (asChunky)
            {
                // Our data is chunky - each byte corresponds to one pixel. We just need to pack nibbles into bytes.
                for (int i = 0; i < Data.Length; i += 2)
                {
                    yield return (byte) (((Data[i] & 0xf) << 4) | (Data[i + 1] & 0xf));
                }
            }
            else
            {
                // We want to convert to chunky, where each group of four bytes is the bitplanes of one row of pixels
                for (int rowOffset = 0; rowOffset < Data.Length; rowOffset += 8)
                {
                    // For this row of pixels, we want to select one bitplane at a time, least significant first
                    for (int shift = 0; shift < 4; ++shift)
                    {
                        // We then collect one bit from each pixel, left to right
                        var rowValue = 0;
                        for (int pixelOffset = 0; pixelOffset < 8; ++pixelOffset)
                        {
                            // Get bit for this pixel
                            var bit = (Data[rowOffset + pixelOffset] >> shift) & 1;
                            // Accumulate it
                            rowValue <<= 1;
                            rowValue |= bit;
                        }
                        // This gives us one bitplane byte
                        yield return (byte)rowValue;
                    }
                }
            }
        }
    }
}