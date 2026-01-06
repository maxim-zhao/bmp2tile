using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BMP2Tile;

public static class Utilities
{
    public static string GetVersion()
    {
        var fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location).FileVersion ?? "?.?";
        while (fileVersion.EndsWith(".0"))
        {
            Console.Error.WriteLine($"{fileVersion}");
            fileVersion = fileVersion[..^2];
        }

        return fileVersion;
    }

    public static Color ParseHexColour(string s)
    {
        // Expecting #RRGGBB
        var match = Regex.Matches(s, "^#([0-9A-Fa-f]{2})([0-9A-Fa-f]{2})([0-9A-Fa-f]{2})$");
        if (match.Count != 1)
        {
            throw new Exception($"Colour must be in #RRGGBB format: {s}");
        }

        var r = int.Parse(match[0].Groups[1].Value, NumberStyles.HexNumber);
        var g = int.Parse(match[0].Groups[2].Value, NumberStyles.HexNumber);
        var b = int.Parse(match[0].Groups[3].Value, NumberStyles.HexNumber);

        return Color.FromArgb(r, g, b);
    }
}