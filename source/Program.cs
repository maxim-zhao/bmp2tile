﻿using System;

namespace BMP2Tile
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            try
            {
                using (var converter = new Converter(message => Console.Out.WriteLine(message)))
                {
                    // We parse the args in order.
                    // This is different from "BMP2Tile Classic" which would accumulate the settings and
                    // then act on them at the end; but I think this way is better.

                    // Since some take multiple parameters, we use lambdas to handle the "continuation".
                    Action<string> nextArgHandler = null;

                    foreach (var arg in args)
                    {
                        if (nextArgHandler != null)
                        {
                            nextArgHandler(arg);
                            nextArgHandler = null;
                            continue;
                        }

                        // ReSharper disable StringLiteralTypo
                        // ReSharper disable AccessToDisposedClosure
                        switch (arg.ToLowerInvariant())
                        {
                            case "-removedupes":
                            case "-removeduplicates":
                                converter.RemoveDuplicates = true;
                                break;
                            case "-noremovedupes":
                            case "-noremoveduplicates":
                                converter.RemoveDuplicates = false;
                                break;
                            case "-usemirroring":
                            case "-mirror":
                                converter.UseMirroring = true;
                                break;
                            case "-nomirror":
                            case "-nomirroring":
                                converter.UseMirroring = false;
                                break;
                            case "-8x8":
                                converter.AdjacentBelow = false;
                                break;
                            case "-8x16":
                                converter.AdjacentBelow = true;
                                break;
                            case "-planar":
                                converter.Chunky = false;
                                break;
                            case "-chunky":
                                converter.Chunky = true;
                                break;
                            case "-tileoffset":
                                nextArgHandler = s => converter.TileOffset = Convert.ToUInt32(s);
                                break;
                            case "-spritepalette":
                                converter.UseSpritePalette = true;
                                break;
                            case "-infrontofsprites":
                                converter.HighPriority = true;
                                break;
                            case "-palcl123":
                                converter.PaletteFormat = Palette.Formats.MasterSystemConstants;
                                break;
                            case "-palsms":
                                converter.PaletteFormat = Palette.Formats.MasterSystem;
                                break;
                            case "-palgg":
                                converter.PaletteFormat = Palette.Formats.GameGear;
                                break;
                            case "-fullpalette":
                                converter.FullPalette = true;
                                break;
                            case "-savetiles":
                                nextArgHandler = s => converter.SaveTiles(s);
                                break;
                            case "-savetilemap":
                                nextArgHandler = s => converter.SaveTilemap(s);
                                break;
                            case "-savepalette":
                                nextArgHandler = s => converter.SavePalette(s);
                                break;
                            case "-exit":
                                return 0;
                            default:
                                converter.Filename = arg;
                                break;
                        }
                        // ReSharper restore AccessToDisposedClosure // Use of converter in lambdas
                        // ReSharper restore StringLiteralTypo // Parameter names
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }

            // If we get here then we are in GUI mode...
            // TODO
            return 1;
        }
    }
}