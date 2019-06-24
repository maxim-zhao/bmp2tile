using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BMP2Tile
{
    internal class Palette
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
            switch (format)
            {
                case Formats.MasterSystem:
                    return _entries.Select(ToMasterSystem);
                case Formats.GameGear:
                    return _entries.Select(ToGameGear).SelectMany(BitConverter.GetBytes);
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        internal string ToString(Formats format)
        {
            switch (format)
            {
                case Formats.MasterSystem:
                    return _entries.Select(ToMasterSystem).Aggregate(".db", (s, e) => s + " $" + e.ToString("X2"));
                case Formats.GameGear:
                    return _entries.Select(ToGameGear).Aggregate(".dw", (s, e) => s + " $" + e.ToString("X3"));
                case Formats.MasterSystemConstants:
                    return _entries.Select(ToMasterSystemConstant).Aggregate(".db", (s, e) => s + " " + e);
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        private int ToMasterSystemChannel(byte b)
        {
            // Some typical colours used for SMS palettes include...
            // eSMS = 0, 57, 123, 189
            // Meka = 0, 85, 170, 255
            //     or 0, 65, 130, 195
            // So we pick some boundaries to flatten that
            return b < 56 ? 0 : b < 122 ? 1 : b < 188 ? 2 : 3;
        }

        private byte ToMasterSystem(Color c)
        {
            return (byte) ((ToMasterSystemChannel(c.B) << 4) | (ToMasterSystemChannel(c.G) << 2) | ToMasterSystemChannel(c.R));
        }

        private string ToMasterSystemConstant(Color c)
        {
            return $"cl{ToMasterSystemChannel(c.R)}{ToMasterSystemChannel(c.G)}{ToMasterSystemChannel(c.B)}";
        }

        private short ToGameGear(Color c)
        {
            // We just truncate to 4 bits per channel
            var to4Bit = new Func<byte, int>(b => (byte)((b >> 4) & 0xf));

            return (short)((to4Bit(c.B) << 8) | (to4Bit(c.G) << 4) | to4Bit(c.R));
        }
    }
}