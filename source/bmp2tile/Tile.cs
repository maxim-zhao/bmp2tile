using System.Collections.Generic;
using System.Linq;

namespace BMP2Tile;

internal class Tile
{
    private readonly byte[] _data;

    public Tile(byte[] data)
    {
        // Truncate to 4bpp
        _data = data.Select(x => (byte)(x & 0xf)).ToArray();
        // Note if the tile was using the sprite palette
        UseSpritePalette = data.Any(x => x > 15);
    }

    // Note that this is only relevant to the "unoptimized" tile data, it is meaningless post-optimization
    public bool UseSpritePalette { get; }

    public IEnumerable<byte> GetValue(bool asChunky)
    {
        if (asChunky)
        {
            // Our data is chunky - each byte corresponds to one pixel. We just need to pack nibbles into bytes.
            for (var i = 0; i < _data.Length; i += 2)
            {
                yield return (byte) (((_data[i] & 0xf) << 4) | (_data[i + 1] & 0xf));
            }
        }
        else
        {
            // We want to convert to planar, where each group of four bytes is the bitplanes of one row of pixels
            for (var rowOffset = 0; rowOffset < _data.Length; rowOffset += 8)
            {
                // For this row of pixels, we want to select one bitplane at a time, least significant first
                for (var shift = 0; shift < 4; ++shift)
                {
                    // We then collect one bit from each pixel, left to right
                    var rowValue = 0;
                    for (var pixelOffset = 0; pixelOffset < 8; ++pixelOffset)
                    {
                        // Get bit for this pixel
                        var bit = (_data[rowOffset + pixelOffset] >> shift) & 1;
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

    private IEnumerable<byte> HFlipped()
    {
        for (var y = 0; y < 8; ++y)
        for (var x = 0; x < 8; ++x)
        {
            yield return _data[y * 8 + (7 - x)];
        }
    }

    private IEnumerable<byte> VFlipped()
    {
        for (var y = 0; y < 8; ++y)
        for (var x = 0; x < 8; ++x)
        {
            yield return _data[(7 - y) * 8 + x];
        }
    }

    private IEnumerable<byte> HAndVFlipped()
    {
        for (var y = 0; y < 8; ++y)
        for (var x = 0; x < 8; ++x)
        {
            yield return _data[(7 - y) * 8 + (7 - x)];
        }
    }

    public enum Match
    {
        Identical,
        HFlip,
        VFlip,
        BothFlip,
        None
    }

    public Match Compare(Tile candidate, bool useMirroring)
    {
        if (_data.SequenceEqual(candidate._data))
        {
            return Match.Identical;
        }

        if (useMirroring)
        {
            if (_data.SequenceEqual(candidate.HFlipped()))
            {
                return Match.HFlip;
            }

            if (_data.SequenceEqual(candidate.VFlipped()))
            {
                return Match.VFlip;
            }
            if (_data.SequenceEqual(candidate.HAndVFlipped()))
            {
                return Match.BothFlip;
            }
        }
        return Match.None;
    }
}