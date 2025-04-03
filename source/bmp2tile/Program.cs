using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BMP2Tile;

internal static class Program
{
    private static bool _verbose;

    public static int Main(string[] args)
    {
        try
        {
            using var converter = new Converter((message, level) =>
            {
                switch (level)
                {
                    case Converter.LogLevel.Verbose:
                        if (_verbose)
                        {
                            Console.Out.WriteLine(message);
                        }

                        break;
                    default:
                    case Converter.LogLevel.Normal:
                        Console.Out.WriteLine(message);
                        break;
                    case Converter.LogLevel.Error:
                        Console.Error.WriteLine(message);
                        break;
                }
            });
            // We parse the args in order.
            // This is different from "BMP2Tile Classic" which would accumulate the settings and
            // then act on them at the end; but I think this way is better.


            // ReSharper disable AccessToDisposedClosure
            return new ArgParser(
                    filename => converter.Filename = filename,
                    () => GetCompressorInfo(converter))
                // ReSharper disable StringLiteralTypo
                .Add(
                    ["loadimage"],
                    "Load the image specified. Can be specified without the action.",
                    s => converter.Filename = s,
                    "filename")
                .Add(
                    ["loadtiles"],
                    "Load raw data as tiles",
                    s => converter.RawTilesFilename = s,
                    "filename")
                .Add(
                    ["loadtilemap"],
                    "Load raw data as tilemap. Suffix with width after a colon, e.g. \":32\", to specify the tilemap width if needed.",
                    s => converter.RawTilemapFilename = s,
                    "filename")
                .Add(
                    ["spritesheet"],
                    "Process the image from a spritesheet to a vertical stack of sprites of the given size, e.g. \"32x16\"",
                    s =>
                    {
                        var match = Regex.Match(s, @"^(?<width>\d+)x(?<height>\d+)$");
                        if (!match.Success)
                        {
                            throw new AppException($"Unable to parse sprite sheet dimensions \"{s}\"");
                        }

                        converter.SpriteSheet(Convert.ToInt32(match.Groups["width"].Value), Convert.ToInt32(match.Groups["height"].Value));
                    },
                    "dimensions")
                .Add(
                    ["removeduplicates", "removedupes"],
                    "Remove duplicate tiles (default)",
                    _ => converter.RemoveDuplicates = true)
                .Add(
                    ["noremoveduplicates", "noremovedupes"],
                    "Do not remove duplicate tiles",
                    _ => converter.RemoveDuplicates = false)
                .Add(
                    ["usemirroring", "mirror"],
                    "Use tile mirroring to remove duplicates (default)",
                    _ => converter.UseMirroring = true)
                .Add(
                    ["nomirroring", "nomirror"],
                    "Use tile mirroring to remove duplicates",
                    _ => converter.UseMirroring = false)
                .Add(
                    ["8x8"],
                    "Treat image as 8x8 tiles (default)",
                    _ => converter.AdjacentBelow = false)
                .Add(
                    ["8x16"],
                    "Treat image as 8x16 tiles (does not work well with duplicate removal)",
                    _ => converter.AdjacentBelow = true)
                .Add(
                    ["planar"],
                    "Convert tiles to 4bpp planar format (e.g. for SMS, GG) (default)",
                    _ => converter.Chunky = false)
                .Add(
                    ["chunky"],
                    "Convert tiles to 4bpp chunky format (e.g. for GBA, MD)",
                    _ => converter.Chunky = true)
                .Add(
                    ["tileoffset"],
                    "Tile offset for first tile found (default is 0)",
                    x => converter.TileOffset = Convert.ToUInt32(x),
                    "offset")
                .Add(
                    ["spritepalette"],
                    "Set tilemap data to use sprite palette (default off)",
                    _ => converter.UseSpritePalette = true)
                .Add(
                    ["infrontofsprites"],
                    "Set tilemap data to be in front of sprites",
                    _ => converter.HighPriority = true)
                .Add(
                    ["palcl123"],
                    "Emit palette data using cl123-style constants (for SMS)",
                    _ => converter.PaletteFormat = Palette.Formats.MasterSystemConstants)
                .Add(
                    ["smspalette", "palsms"],
                    "Emit palette in SMS format (6bpp)",
                    _ => converter.PaletteFormat = Palette.Formats.MasterSystem)
                .Add(
                    ["ggpalette", "palgg"],
                    "Emit palette in GG format (12bpp)",
                    _ => converter.PaletteFormat = Palette.Formats.GameGear)
                .Add(
                    ["fullpalette"],
                    "Emit 16 palette entries regardless of image palette size",
                    _ => converter.FullPalette = true)
                .Add(
                    ["minimumpalette"],
                    "Emit only as many palette entries as needed to include values used in the image",
                    _ => converter.FullPalette = false)
                .Add(
                    ["savetiles"],
                    "Save tiles to the given filename. Extension selects compression.",
                    converter.SaveTiles,
                    "filename")
                .Add(
                    ["savetilemap"],
                    "Save tilemap to the given filename. Extension selects compression.",
                    converter.SaveTilemap,
                    "filename")
                .Add(
                    ["savepalette"],
                    "Save tilemap to the given filename. Format is controlled by other palette-related parameters.",
                    converter.SavePalette,
                    "filename")
                .Add(
                    ["exit"],
                    "Exit the program. Later actions are ignored.",
                    _ => Environment.Exit(0))
                .Add(
                    ["verbose", "v"],
                    "Enable verbose logging",
                    _ => _verbose = true)
                .Add(
                    ["quiet", "q"],
                    "Disable verbose logging",
                    _ => _verbose = false)
                .Add(
                    ["dllpath"],
                    "Path to search for compression DLLs. Defaults to the app directory",
                    s => converter.DllPath = s,
                    "path")
                // ReSharper restore StringLiteralTypo
                // ReSharper restore AccessToDisposedClosure
                .Parse(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            Console.Error.WriteLine($"Guru meditation:\n{ex.StackTrace}");
            return 1;
        }
    }

    private static IList<IList<string>> GetCompressorInfo(Converter converter)
    {
        return new[]
            {
                new[] { "Available compressors:" },
                new[] { "Extension", "Name", "Capabilities" }
            }
            .Concat(
                converter.GetCompressorInfo()
                    .Select(compressor => new[]
                    {
                        compressor.Extension,
                        compressor.Name,
                        compressor.Capabilities.ToString()
                    }))
            .Cast<IList<string>>()
            .ToList();
    }
}