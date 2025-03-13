using System;
using System.Collections.Generic;
using System.Linq;

namespace BMP2Tile
{
    internal static class Program
    {
        private static bool _verbose;

        static int Main(string[] args)
        {
            try
            {
                using (var converter = new Converter((message, level) =>
                       {
                           switch (level)
                           {
                               case Converter.LogLevel.Verbose:
                                   if (_verbose)
                                   {
                                       Console.Out.WriteLine(message);
                                   }

                                   break;
                               case Converter.LogLevel.Normal:
                                   Console.Out.WriteLine(message);
                                   break;
                               case Converter.LogLevel.Error:
                                   Console.Error.WriteLine(message);
                                   break;
                           }
                       }))
                {
                    // We parse the args in order.
                    // This is different from "BMP2Tile Classic" which would accumulate the settings and
                    // then act on them at the end; but I think this way is better.

                    // ReSharper disable AccessToDisposedClosure
                    // ReSharper disable StringLiteralTypo
                    var parser = new ArgParser(
                        filename => converter.Filename = filename,
                        () => GetCompressorInfo(converter));
                    parser.Add(
                        new[] { "loadimage" },
                        "Load the image specified. Can be specified without the action.",
                        s => converter.Filename = s,
                        "filename");
                    parser.Add(
                        new[] { "removeduplicates", "removedupes" },
                        "Remove duplicate tiles (default)",
                        _ => converter.RemoveDuplicates = true);
                    parser.Add(
                        new[] { "noremoveduplicates", "noremovedupes" },
                        "Do not remove duplicate tiles",
                        _ => converter.RemoveDuplicates = false);
                    parser.Add(
                        new[] { "usemirroring", "mirror" },
                        "Use tile mirroring to remove duplicates (default)",
                        _ => converter.UseMirroring = true);
                    parser.Add(
                        new[] { "nomirroring", "nomirror" },
                        "Use tile mirroring to remove duplicates",
                        _ => converter.UseMirroring = false);
                    parser.Add(
                        new[] { "8x8" },
                        "Treat image as 8x8 tiles (default)",
                        _ => converter.AdjacentBelow = false);
                    parser.Add(
                        new[] { "8x16" },
                        "Treat image as 8x16 tiles (does not work will with duplicate removal)",
                        _ => converter.AdjacentBelow = true);
                    parser.Add(
                        new[] { "planar" },
                        "Convert tiles to 4bpp planar format (e.g. for SMS, GG) (default)",
                        _ => converter.Chunky = false);
                    parser.Add(
                        new[] { "chunky" },
                        "Convert tiles to 4bpp chunky format (e.g. for GBA, MD)",
                        _ => converter.Chunky = true);
                    parser.Add(
                        new[] { "tileoffset" },
                        "Tile offset for first tile found (default is 0)",
                        x => converter.TileOffset = Convert.ToUInt32(x),
                        "offset");
                    parser.Add(
                        new[] { "spritepalette" },
                        "Set tilemap data to use sprite palette (default off)",
                        _ => converter.UseSpritePalette = true);
                    parser.Add(
                        new[] { "infrontofsprites" },
                        "Set tilemap data to be in front of sprites",
                        _ => converter.HighPriority = true);
                    parser.Add(
                        new[] { "palcl123" },
                        "Emit palette data using cl123-style constants (for SMS)",
                        _ => converter.PaletteFormat = Palette.Formats.MasterSystemConstants);
                    parser.Add(
                        new[] { "smspalette", "palsms" },
                        "Emit palette in SMS format (6bpp)",
                        _ => converter.PaletteFormat = Palette.Formats.MasterSystem);
                    parser.Add(
                        new[] { "ggpalette", "palgg" },
                        "Emit palette in GG format (12bpp)",
                        _ => converter.PaletteFormat = Palette.Formats.GameGear);
                    parser.Add(
                        new[] { "fullpalette" },
                        "Emit 16 palette entries regardless of image palette size",
                        _ => converter.FullPalette = true);
                    parser.Add(
                        new[] { "minimumpalette" },
                        "Emit only as many palette entries as needed to include values used in the image",
                        _ => converter.FullPalette = false);
                    parser.Add(
                        new[] { "savetiles" },
                        "Save tiles to the given filename. Extension selects compression.",
                        s => converter.SaveTiles(s),
                        "filename");
                    parser.Add(
                        new[] { "savetilemap" },
                        "Save tilemap to the given filename. Extension selects compression.",
                        s => converter.SaveTilemap(s),
                        "filename");
                    parser.Add(
                        new[] { "savepalette" },
                        "Save tilemap to the given filename. Format is controlled by other palette-related parameters.",
                        s => converter.SavePalette(s),
                        "filename");
                    parser.Add(
                        new[] { "exit" },
                        "Exit the program. Later actions are ignored.",
                        s => Environment.Exit(0));
                    parser.Add(
                        new[] { "verbose", "v" },
                        "Enable verbose logging",
                        _ => _verbose = true);
                    parser.Add(
                        new[] { "quiet", "q" },
                        "Disable verbose logging",
                        _ => _verbose = false);
                    // ReSharper restore StringLiteralTypo
                    // ReSharper restore AccessToDisposedClosure

                    return parser.Parse(args);
                }
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
}
