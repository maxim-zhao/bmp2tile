using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NUnit.Framework;
using BMP2Tile;

namespace bmp2tile.Tests;

[TestFixture]
[Parallelizable]
public class ConverterTests
{
    private string _testDir;
    private Converter _conv;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _testDir = FindTestDirectory();

        if (_testDir != null)
        {
            TestContext.WriteLine($"Using test directory: {_testDir}");
            Assert.That(Directory.Exists(_testDir), Is.True);
        }
        else
        {
            Assert.Fail($"""
                         Test directory not found
                         TestDirectory={TestContext.CurrentContext.TestDirectory} 
                         WorkDirectory={TestContext.CurrentContext.WorkDirectory}
                         Assembly Location = {Assembly.GetCallingAssembly().Location}
                         """);
        }
    }

    [SetUp]
    public void SetUp()
    {
        _conv = new Converter((s, level) => { TestContext.WriteLine($"[{level}] {s}"); });
    }

    [TearDown]
    public void TearDown()
    {
        _conv?.Dispose();
    }

    private static string FindTestDirectory(
        [CallerFilePath] string filePath = "")
    {
        // Walk up from the source folder looking for a "test" directory
        var dir = new FileInfo(filePath).Directory;
        while (dir != null)
        {
            Console.WriteLine($"Trying {dir}...");
            var candidate = Path.Combine(dir.FullName, "test");
            if (Directory.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }

            dir = dir.Parent;
        }

        return null;
    }

    [Test]
    public void PaletteFormats()
    {
        // This image uses the first 8 colours from the palette
        _conv.Filename = Path.Combine(_testDir, "0-7.png");
        // Start at SMS palette with 8 entries
        Assert.That(_conv.GetPaletteAsText(), Is.EqualTo(".db $00 $04 $05 $30 $08 $0C $38 $3C"));
        _conv.PaletteFormat = Palette.Formats.GameGear;
        Assert.That(_conv.GetPaletteAsText(), Is.EqualTo(".dw $000 $050 $055 $F00 $0A0 $0F0 $FA0 $FF0"));
        _conv.FullPalette = true;
        Assert.That(_conv.GetPaletteAsText(),
            Is.EqualTo(".dw $000 $050 $055 $F00 $0A0 $0F0 $FA0 $FF0 $000 $000 $000 $000 $000 $000 $000 $000"));
        _conv.FullPalette = false;
        _conv.PaletteFormat = Palette.Formats.MasterSystemConstants;
        Assert.That(_conv.GetPaletteAsText(),
            Is.EqualTo(".db cl000 cl010 cl110 cl003 cl020 cl030 cl023 cl033"));
        var palettes = _conv.GetPalettes();
        Assert.That(palettes.Count, Is.EqualTo(2));
        Assert.That(palettes[0].Count, Is.EqualTo(8));
        Assert.That(palettes[0], Is.EqualTo(palettes[1]));
        _conv.PaletteFormat = Palette.Formats.GameGear;
        palettes = _conv.GetPalettes();
        Assert.That(palettes.Count, Is.EqualTo(2));
        Assert.That(palettes[0].Count, Is.EqualTo(8));
        Assert.That(palettes[0], Is.EqualTo(palettes[1]));
    }

    [Test]
    public void DualPalette()
    {
        _conv.Filename = Path.Combine(_testDir, "dual-palette.png");
        Assert.That(_conv.GetPaletteAsText(),
            Is.EqualTo(".db $00 $0A $0F $2F $3F $00 $00 $00 $00 $00 $00 $00 $00 $00 $00 $00 $00 $20 $34 $3C $3F"));
        Assert.That(_conv.GetTilesAsText().Trim(), Is.EqualTo(
            """
            ; Tile index $000
            .db $00 $00 $FF $00 $80 $80 $7F $00 $FF $80 $00 $00 $FE $81 $00 $00 $FD $82 $00 $00 $80 $FF $00 $00 $E8 $80 $00 $00 $50 $80 $00 $00
            ; Tile index $001
            .db $5F $5F $A0 $00 $BE $BF $40 $00 $51 $AE $00 $00 $A1 $5E $00 $00 $41 $BE $00 $00 $01 $FE $00 $00 $01 $00 $00 $00 $00 $00 $00 $00
            """));
        Assert.That(_conv.GetTilemapAsText().Trim(), Is.EqualTo(
            """
            .dw $0000 $0001
            .dw $0800 $0801
            """)); // Use sprite palette bit is set for bottom row
        _conv.UseSpritePalette = true;
        Assert.That(_conv.GetTilemapAsText().Trim(), Is.EqualTo(
            """
            .dw $0800 $0801
            .dw $0800 $0801
            """)); // Use sprite palette bit is set for all
        _conv.UseSpritePalette = false;
        _conv.HighPriority = true;
        Assert.That(_conv.GetTilemapAsText().Trim(), Is.EqualTo(
            """
            .dw $1000 $1001
            .dw $1800 $1801
            """)); // Use sprite palette bit is set for bottom row, priority bit is set for all
    }

    [Test]
    public void PaletteOverrides()
    {
        // This image uses the first 8 colours from the palette
        _conv.Filename = Path.Combine(_testDir, "0-7.png");
        // Start at SMS palette with 8 entries
        Assert.That(_conv.GetPaletteAsText(), Is.EqualTo(".db $00 $04 $05 $30 $08 $0C $38 $3C"));
        // Override some colours
        _conv.AddPaletteOverride(0, Color.White);
        _conv.AddPaletteOverride(2, Color.Red);
        _conv.AddPaletteOverride(4, Color.Cyan);
        _conv.AddPaletteOverride(6, Color.Magenta);
        Assert.That(_conv.GetPaletteAsText(), Is.EqualTo(".db $3F $04 $03 $30 $3C $0C $33 $3C"));
        // Add one at an unused index
        _conv.AddPaletteOverride(14, Color.Lime);
        Assert.That(_conv.GetPaletteAsText(),
            Is.EqualTo(".db $3F $04 $03 $30 $3C $0C $33 $3C $00 $00 $00 $00 $00 $00 $0C"));
        _conv.FullPalette = true;
        Assert.That(_conv.GetPaletteAsText(),
            Is.EqualTo(".db $3F $04 $03 $30 $3C $0C $33 $3C $00 $00 $00 $00 $00 $00 $0C $00"));
        // Put them back
        _conv.ClearPaletteOverrides();
        Assert.That(_conv.GetPaletteAsText(),
            Is.EqualTo(".db $00 $04 $05 $30 $08 $0C $38 $3C $00 $00 $00 $00 $00 $00 $00 $00"));
    }

    [Test]
    public void PaletteTruncation()
    {
        // This image uses only the first entry in the palette
        _conv.Filename = Path.Combine(_testDir, "blanktile.png");
        Assert.That(_conv.GetPaletteAsText(), Is.EqualTo(".db $00"));
        _conv.FullPalette = true;
        Assert.That(_conv.GetPaletteAsText(),
            Is.EqualTo(".db $00 $04 $05 $30 $08 $0C $38 $3C $02 $06 $03 $0B $0F $3A $2F $3F"));
    }

    [Test]
    public void ImageNotPaletted()
    {
        // This image is 24-bit
        _conv.Filename = Path.Combine(_testDir, "24bpp.png");
        Assert.That(
            // ReSharper disable once AccessToDisposedClosure
            () => { _conv.GetTilesAsText(); },
            Throws.InstanceOf<AppException>().With.Message.Contains("Unsupported bitmap format"));
    }


    [Test]
    public void BadImageSize()
    {
        // This image is 24-bit
        _conv.Filename = Path.Combine(_testDir, "Bad width.png");
        Assert.That(
            // ReSharper disable once AccessToDisposedClosure
            () => { _conv.GetTilesAsText(); },
            Throws.InstanceOf<AppException>().With.Message.Contains("width"));
    }

    [Test]
    public void TileCount()
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        // 263 distinct tiles at first
        Assert.That(
            Regex.Matches(_conv.GetTilesAsText(), "; Tile index ").Count,
            Is.EqualTo(263));
        // 284 without mirroring
        _conv.UseMirroring = false;
        Assert.That(
            Regex.Matches(_conv.GetTilesAsText(), "; Tile index ").Count,
            Is.EqualTo(284));
        // 768 without duplicate removal
        _conv.RemoveDuplicates = false;
        Assert.That(
            Regex.Matches(_conv.GetTilesAsText(), "; Tile index ").Count,
            Is.EqualTo(768));
        // Back to normal
        _conv.UseMirroring = true;
        _conv.RemoveDuplicates = true;
        // This should make our tile count go down by 1
        _conv.ReplaceFirstTileWith(0);
        Assert.That(
            Regex.Matches(_conv.GetTilesAsText(), "; Tile index ").Count,
            Is.EqualTo(262));
        // Now we ask for a subset
        _conv.ReplaceFirstTileWith(-1); // This turns it off
        _conv.SetTileRange(1, 2);
        Assert.That(
            _conv.GetTilesAsText().Trim(),
            Is.EqualTo(
                // This is the original tiles 1..2. The comments are maybe now a bit confusing...
                """
                ; Tile index $000
                .db $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $00 $00 $00
                ; Tile index $001
                .db $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF
                """));
    }

    [Test]
    public void TileFormats()
    {
        // This image uses the first 8 colours from the palette
        _conv.Filename = Path.Combine(_testDir, "0-7.png");
        // First planar...
        Assert.That(_conv.GetTilesAsText().Trim(), Is.EqualTo(
            """
            ; Tile index $000
            .db $00 $00 $00 $00 $FF $00 $00 $00 $00 $FF $00 $00 $FF $FF $00 $00 $00 $00 $FF $00 $FF $00 $FF $00 $00 $FF $FF $00 $FF $FF $FF $00
            """));
        // Then chunky
        _conv.Chunky = true;
        Assert.That(_conv.GetTilesAsText().Trim(), Is.EqualTo(
            """
            ; Tile index $000
            .db $00 $00 $00 $00 $11 $11 $11 $11 $22 $22 $22 $22 $33 $33 $33 $33 $44 $44 $44 $44 $55 $55 $55 $55 $66 $66 $66 $66 $77 $77 $77 $77
            """));
    }

    [TestCase("akmw.bmp", ".db $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF")]
    [TestCase("akmw-8bpp.png", ".db $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF")]
    [TestCase("akmw-1bpp.png", ".db $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00")]
    public void ImageBitDepth(string filename, string expectedTile11)
    {
        _conv.Filename = Path.Combine(_testDir, filename);
        _conv.RemoveDuplicates = false;
        _conv.SetTileRange(10, 10);
        Assert.That(
            StripToLines(_conv.GetTilesAsText())[0], 
            Is.EqualTo(expectedTile11));
    }

    private static List<string> StripToLines(string text)
    {
        return text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.StartsWith(".d"))
            .ToList();
    }

    [Test]
    public void TileOrder()
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        _conv.RemoveDuplicates = false;
        var orderedTiles = StripToLines(_conv.GetTilesAsText());
        _conv.AdjacentBelow = true;
        var newTiles = StripToLines(_conv.GetTilesAsText());
        Assert.That(orderedTiles, Has.Count.EqualTo(768));
        Assert.That(newTiles, Has.Count.EqualTo(768));
        for (var index = 0; index < 768; ++index)
        {
            // First grab the data for the tile at this index
            var oldTile = orderedTiles[index];
            // Then figure out where that tile ought to be in the "adjacent below" ordering
            // Given source data
            //  0  1  2  3 ...
            // 32 33 34 35 ...
            // 64 65 66 67 ...
            // 96 97 98 99 ...
            // We expect the new data to be in order:
            //  0 32  1 33  2 34  3 35 ...
            // 64 96 65 97 66 98 67 99 ...
            var locationInReordered = index / 64 * 64; // Pick the start of the pair of rows
            locationInReordered += (index % 32) * 2;
            if (index % 64 > 31)
            {
                locationInReordered += 1;
            }

            var newTile = newTiles[locationInReordered];
            Assert.That(newTile, Is.EqualTo(oldTile), $"Tile {index} expected to be at {locationInReordered}");
        }
    }

    [Test]
    public void TileOffsetAndExclusions()
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        Assert.That(
            Regex.Matches(
                    _conv.GetTilemapAsText(),
                    "\\$([0-9A-F]+)")
                .Min(x => int.Parse(x.Groups[1].Value, NumberStyles.HexNumber)),
            Is.Zero);

        _conv.TileOffset = 10;
        Assert.That(
            Regex.Matches(
                    _conv.GetTilemapAsText(),
                    "\\$([0-9A-F]+)")
                .Min(x => int.Parse(x.Groups[1].Value, NumberStyles.HexNumber)),
            Is.EqualTo(10));

        _conv.ExcludeTileIndex(10);
        Assert.That(
            Regex.Matches(
                    _conv.GetTilemapAsText(),
                    "\\$([0-9A-F]+)")
                .Min(x => int.Parse(x.Groups[1].Value, NumberStyles.HexNumber)),
            Is.EqualTo(11));

        _conv.ResetExcludedTileIndices();
        Assert.That(
            Regex.Matches(
                    _conv.GetTilemapAsText(),
                    "\\$([0-9A-F]+)")
                .Min(x => int.Parse(x.Groups[1].Value, NumberStyles.HexNumber)),
            Is.EqualTo(10));
    }

    [TestCase("akmw.bmp")]
    [TestCase("akmw-8bpp.png")]
    [TestCase("akmw-1bpp.png")]
    public void SpriteSheet(string filename)
    {
        _conv.Filename = Path.Combine(_testDir, filename);
        _conv.RemoveDuplicates = false;
        _conv.SpriteSheet(32, 32);
        Assert.That(
            string.Join("\r\n", StripToLines(_conv.GetTilemapAsText()).Take(4)),
            Is.EqualTo(
                """
                .dw $0000 $0001 $0002 $0003
                .dw $0004 $0005 $0006 $0007
                .dw $0008 $0009 $000A $000B
                .dw $000C $000D $000E $000F
                """),
            "Expected a 4 column wide tilemap");
        var tileData = filename.Contains("1bpp")
            ? """
              .db $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $00 $00 $00 $00
              .db $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $00 $00 $00 $00
              .db $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $7F $00 $00 $00
              .db $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00 $FF $00 $00 $00
              """
            : """
              .db $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $00 $00 $00
              .db $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $00 $00 $00
              .db $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $7F $7F $7F
              .db $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF $00 $FF $FF $FF
              """;
        Assert.That(
            string.Join("\r\n", StripToLines(_conv.GetTilesAsText()).Skip(16).Take(4)),
            Is.EqualTo(tileData),
            "Second sprite is not expected tiles");
    }


    [Test]
    public void CropTilemap()
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        _conv.CropTo(32, 64, 8, 16);
        _conv.RemoveDuplicates = false;
        Assert.That(_conv.GetTilemapAsText().Trim(), Is.EqualTo(
            """
            .dw $0044
            .dw $0064
            """));
        Assert.That(
            () => {_conv.CropTo(1, 2, 3, 4);},
            Throws.Exception.InstanceOf<ArgumentException>());
    }

    [Test]
    public void SetTileRange()
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        var allTiles = StripToLines(_conv.GetTilesAsText());
        Assert.That(allTiles.Count, Is.EqualTo(263));
        _conv.SetTileRange(10, 20);
        Assert.That(StripToLines(_conv.GetTilesAsText()), Is.EqualTo(allTiles.Skip(10).Take(11)));
        _conv.SetTileRange(0, 0);
        Assert.That(StripToLines(_conv.GetTilesAsText()), Is.EqualTo(allTiles.Skip(0).Take(1)));
        _conv.SetTileRange(262, 300);
        Assert.That(StripToLines(_conv.GetTilesAsText()), Is.EqualTo(allTiles.Skip(262).Take(1)));

    }

    [Test]
    public void GetCompressorInfo()
    {
        var info = _conv.GetCompressorInfo().ToList();
        // We only have the built-in .inc "compressor"
        Assert.That(info.Count, Is.EqualTo(1));
    }

    [Test]
    public void SaveFiles()
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        var tilesFilename = Path.Combine(TestContext.CurrentContext.WorkDirectory, "tiles.inc");
        var tilemapFilename = Path.Combine(TestContext.CurrentContext.WorkDirectory, "tilemap.inc");
        var paletteFilename = Path.Combine(TestContext.CurrentContext.WorkDirectory, "palette.inc");
        _conv.SaveTiles(tilesFilename);
        _conv.SaveTilemap(tilemapFilename);
        _conv.SavePalette(paletteFilename);
        Assert.That(File.ReadAllText(tilesFilename), Is.EqualTo(_conv.GetTilesAsText()));
        Assert.That(File.ReadAllText(tilemapFilename), Is.EqualTo(_conv.GetTilemapAsText()));
        Assert.That(File.ReadAllText(paletteFilename), Is.EqualTo(_conv.GetPaletteAsText()));
    }
}
