using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BMP2Tile;

public class Palette
{
    private readonly List<Color> _entries;

    public enum Formats
    {
        MasterSystem,
        GameGear,
        MasterSystemConstants
    }

    public Palette(IEnumerable<Color> entries)
    {
        _entries = entries.ToList();
    }

    public IEnumerable<byte> GetValue(Formats format)
    {
        return format switch
        {
            Formats.MasterSystem => _entries.Select(ToMasterSystem),
            Formats.GameGear => _entries.Select(ToGameGear).SelectMany(BitConverter.GetBytes),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    internal string ToString(Formats format)
    {
        return format switch
        {
            Formats.MasterSystem => _entries.Select(ToMasterSystem).Aggregate(".db", (s, e) => s + " $" + e.ToString("X2")),
            Formats.GameGear => _entries.Select(ToGameGear).Aggregate(".dw", (s, e) => s + " $" + e.ToString("X3")),
            Formats.MasterSystemConstants => _entries.Select(ToMasterSystemConstant).Aggregate(".db", (s, e) => s + " " + e),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    private static int ToMasterSystemChannel(byte b)
    {
        // Some typical colours used for SMS palettes include...
        // eSMS = 0, 57, 123, 189
        // Meka = 0, 85, 170, 255
        //     or 0, 65, 130, 195
        // So we pick some boundaries to flatten that
        return b < 56 ? 0 : b < 122 ? 1 : b < 188 ? 2 : 3;
    }

    private static byte ToMasterSystem(Color c)
    {
        return (byte) ((ToMasterSystemChannel(c.B) << 4) | (ToMasterSystemChannel(c.G) << 2) | ToMasterSystemChannel(c.R));
    }

    private static string ToMasterSystemConstant(Color c)
    {
        return $"cl{ToMasterSystemChannel(c.R)}{ToMasterSystemChannel(c.G)}{ToMasterSystemChannel(c.B)}";
    }

    private static short ToGameGear(Color c)
    {
        // We just truncate to 4 bits per channel
        var to4Bit = new Func<byte, int>(b => (byte) ((b >> 4) & 0xf));

        return (short) ((to4Bit(c.B) << 8) | (to4Bit(c.G) << 4) | to4Bit(c.R));
    }

    public List<Color> ForDisplay(Formats format)
    {
        return format switch
        {
            Formats.MasterSystem or Formats.MasterSystemConstants => _entries.Select(ToMasterSystem).Select(FromMasterSystem).ToList(),
            Formats.GameGear => _entries.Select(ToGameGear).Select(FromGameGear).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    private static Color FromMasterSystem(byte value)
    {
        var r = (value >> 0) & 0b000011;
        var g = (value >> 2) & 0b000011;
        var b = (value >> 4) & 0b000011;
        r = r | (r << 2) | (r << 4) | (r << 6);
        g = g | (g << 2) | (g << 4) | (g << 6);
        b = b | (b << 2) | (b << 4) | (b << 6);
        return Color.FromArgb(r, g, b);
    }
    private static Color FromGameGear(short value)
    {
        var r = (value >> 0) & 0b001111;
        var g = (value >> 4) & 0b001111;
        var b = (value >> 8) & 0b001111;
        r |= r << 4;
        g |= g << 4;
        b |= b << 4;
        return Color.FromArgb(r, g, b);
    }

}