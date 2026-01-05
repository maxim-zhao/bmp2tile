using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using BMP2Tile;

namespace bmp2tile.Tests;

[TestFixture]
[Parallelizable]
public class ConverterTests
{
    private string _testDir;

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
            Assert.Fail("Test directory not found");
        }
    }

    private static string FindTestDirectory()
    {
        // Walk up from the test output folder searching for a 'test' directory
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir != null)
        {
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
        using var conv = new Converter((_, _) => { });
        // This image uses the first 8 colours from the palette
        conv.Filename = Path.Combine(_testDir, "0-7.png");
        // Start at SMS palette with 8 entries
        Assert.That(conv.GetPaletteAsText(), Is.EqualTo(".db $00 $04 $05 $30 $08 $0C $38 $3C"));
        conv.PaletteFormat = Palette.Formats.GameGear;
        Assert.That(conv.GetPaletteAsText(), Is.EqualTo(".dw $000 $050 $055 $F00 $0A0 $0F0 $FA0 $FF0"));
        conv.FullPalette = true;
        Assert.That(conv.GetPaletteAsText(), Is.EqualTo(".dw $000 $050 $055 $F00 $0A0 $0F0 $FA0 $FF0 $000 $000 $000 $000 $000 $000 $000 $000"));
    }

    [Test]
    public void DualPalette()
    {
        using var conv = new Converter((_, _) => { });
        conv.Filename = Path.Combine(_testDir, "dual-palette.png");
        Assert.That(conv.GetPaletteAsText(), Is.EqualTo(".db $00 $0A $0F $2F $3F $00 $00 $00 $00 $00 $00 $00 $00 $00 $00 $00 $00 $20 $34 $3C $3F"));
        Assert.That(conv.GetTilesAsText().Trim(), Is.EqualTo(
            """
            ; Tile index $000
            .db $00 $00 $FF $00 $80 $80 $7F $00 $FF $80 $00 $00 $FE $81 $00 $00 $FD $82 $00 $00 $80 $FF $00 $00 $E8 $80 $00 $00 $50 $80 $00 $00
            ; Tile index $001
            .db $5F $5F $A0 $00 $BE $BF $40 $00 $51 $AE $00 $00 $A1 $5E $00 $00 $41 $BE $00 $00 $01 $FE $00 $00 $01 $00 $00 $00 $00 $00 $00 $00
            """));
        Assert.That(conv.GetTilemapAsText().Trim(), Is.EqualTo(
            """
            .dw $0000 $0001
            .dw $0800 $0801
            """)); // Use sprite palette bit is set for bottom row
        conv.UseSpritePalette = true;
        Assert.That(conv.GetTilemapAsText().Trim(), Is.EqualTo(
            """
            .dw $0800 $0801
            .dw $0800 $0801
            """)); // Use sprite palette bit is set for all
        conv.UseSpritePalette = false;
        conv.HighPriority = true;
        Assert.That(conv.GetTilemapAsText().Trim(), Is.EqualTo(
            """
            .dw $1000 $1001
            .dw $1800 $1801
            """)); // Use sprite palette bit is set for bottom row, priority bit is set for all

    }

    [Test]
    public void PaletteTruncation()
    {
        using var conv = new Converter((_, _) => { });
        // This image uses only the first entry in the palette
        conv.Filename = Path.Combine(_testDir, "blanktile.png");
        Assert.That(conv.GetPaletteAsText(), Is.EqualTo(".db $00"));
        conv.FullPalette = true;
        Assert.That(conv.GetPaletteAsText(), Is.EqualTo(".db $00 $04 $05 $30 $08 $0C $38 $3C $02 $06 $03 $0B $0F $3A $2F $3F"));
    }

    [Test]
    public void ImageNotPaletted()
    {
        using var conv = new Converter((_, _) => { });
        // This image is 24-bit
        conv.Filename = Path.Combine(_testDir, "24bpp.png");
        Assert.That(
            // ReSharper disable once AccessToDisposedClosure
            () => { conv.GetTilesAsText(); },
            Throws.InstanceOf<AppException>().With.Message.Contains("Unsupported bitmap format"));
    }


    [Test]
    public void BadImageSize()
    {
        using var conv = new Converter((_, _) => { });
        // This image is 24-bit
        conv.Filename = Path.Combine(_testDir, "Bad width.png");
        Assert.That(
            // ReSharper disable once AccessToDisposedClosure
            () => { conv.GetTilesAsText(); },
            Throws.InstanceOf<AppException>().With.Message.Contains("width"));
    }

    [Test]
    public void TileCount()
    {
        using var conv = new Converter((_, _) => { });
        conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        // 263 distinct tiles at first
        Assert.That(
            Regex.Matches(conv.GetTilesAsText(), "; Tile index ").Count,
            Is.EqualTo(263));
        // 284 without mirroring
        conv.UseMirroring = false;
        Assert.That(
            Regex.Matches(conv.GetTilesAsText(), "; Tile index ").Count,
            Is.EqualTo(284));
        // 768 without duplicate removal
        conv.RemoveDuplicates = false;
        Assert.That(
            Regex.Matches(conv.GetTilesAsText(), "; Tile index ").Count,
            Is.EqualTo(768));
        // Back to normal
        conv.UseMirroring = true;
        conv.RemoveDuplicates = true;
        // This should make our tile count go down by 1
        conv.ReplaceFirstTileWith(0);
        Assert.That(
            Regex.Matches(conv.GetTilesAsText(), "; Tile index ").Count,
            Is.EqualTo(262));
        // Now we ask for a subset
        conv.ReplaceFirstTileWith(-1); // This turns it off
        conv.SetTileRange(1, 2);
        Assert.That(
            conv.GetTilesAsText().Trim(),
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
        using var conv = new Converter((_, _) => { });
        // This image uses the first 8 colours from the palette
        conv.Filename = Path.Combine(_testDir, "0-7.png");
        // First planar...
        Assert.That(conv.GetTilesAsText().Trim(), Is.EqualTo(
            """
            ; Tile index $000
            .db $00 $00 $00 $00 $FF $00 $00 $00 $00 $FF $00 $00 $FF $FF $00 $00 $00 $00 $FF $00 $FF $00 $FF $00 $00 $FF $FF $00 $FF $FF $FF $00
            """));
        // Then chunky
        conv.Chunky = true;
        Assert.That(conv.GetTilesAsText().Trim(), Is.EqualTo(
            """
            ; Tile index $000
            .db $00 $00 $00 $00 $11 $11 $11 $11 $22 $22 $22 $22 $33 $33 $33 $33 $44 $44 $44 $44 $55 $55 $55 $55 $66 $66 $66 $66 $77 $77 $77 $77
            """));
    }

    [Test]
    public void TileOrder()
    {
        using var conv = new Converter((_, _) => { });
        conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        conv.RemoveDuplicates = false;
        var orderedTiles = conv.GetTilesAsText()
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.StartsWith(".db"))
            .ToList();
        conv.AdjacentBelow = true;
        var newTiles = conv.GetTilesAsText()
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.StartsWith(".db"))
            .ToList();
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
    public void TileOffset()
    {
        using var conv = new Converter((_, _) => { });
        conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        Assert.That(
            Regex.Matches(
                    conv.GetTilemapAsText(),
                    "\\$([0-9A-F]+)")
                .Min(x => int.Parse(x.Groups[1].Value, NumberStyles.HexNumber)), 
            Is.Zero);

        conv.TileOffset = 10;
        Assert.That(
            Regex.Matches(
                    conv.GetTilemapAsText(),
                    "\\$([0-9A-F]+)")
                .Min(x => int.Parse(x.Groups[1].Value, NumberStyles.HexNumber)),
            Is.EqualTo(10));

        conv.ExcludeTileIndex(10);
        Assert.That(
            Regex.Matches(
                    conv.GetTilemapAsText(),
                    "\\$([0-9A-F]+)")
                .Min(x => int.Parse(x.Groups[1].Value, NumberStyles.HexNumber)),
            Is.EqualTo(11));
    }
}
