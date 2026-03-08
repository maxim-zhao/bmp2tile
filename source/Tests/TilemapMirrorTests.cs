using System;
using System.IO;
using NUnit.Framework;
using BMP2Tile;

namespace bmp2tile.Tests;

[TestFixture]
public class TilemapMirrorTests
{
    private string _testDir;
    private Converter _conv;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _testDir = FindTestDirectory();
        Assert.That(_testDir, Is.Not.Null.And.Not.Empty, "Test directory not found");
    }

    [SetUp]
    public void SetUp()
    {
        _conv = new Converter((s, level) => { });
    }

    [TearDown]
    public void TearDown()
    {
        _conv?.Dispose();
    }

    private static string FindTestDirectory()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "test");
            if (Directory.Exists(candidate))
                return Path.GetFullPath(candidate);
            dir = dir.Parent;
        }
        return null;
    }

    [TestCase(Converter.TilemapMirrorMode.Horizontal)]
    [TestCase(Converter.TilemapMirrorMode.Vertical)]
    public void MirroredTilemap_DoesNotThrow(Converter.TilemapMirrorMode mode)
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        _conv.TilemapMirror = mode;
        Assert.That(() => _conv.GetTilemapAsText(), Throws.Nothing, $"Mirror {mode} should not throw");
    }

    [Test]
    public void MirroredTilemap_Horizontal_ChangesText()
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        var orig = _conv.GetTilemapAsText();
        _conv.TilemapMirror = Converter.TilemapMirrorMode.Horizontal;
        var mirrored = _conv.GetTilemapAsText();
        Assert.That(mirrored, Is.Not.EqualTo(orig), "Horizontal mirror should change tilemap text");
    }

    [Test]
    public void MirroredTilemap_Vertical_ChangesText()
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        var orig = _conv.GetTilemapAsText();
        _conv.TilemapMirror = Converter.TilemapMirrorMode.Vertical;
        var mirrored = _conv.GetTilemapAsText();
        Assert.That(mirrored, Is.Not.EqualTo(orig), "Vertical mirror should change tilemap text");
    }

    [Test]
    public void Mirror_After_Rotation()
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        _conv.TilemapRotation = 90;
        var rotated = _conv.GetTilemapAsText();
        _conv.TilemapMirror = Converter.TilemapMirrorMode.Horizontal;
        var rotatedAndMirrored = _conv.GetTilemapAsText();
        Assert.That(rotatedAndMirrored, Is.Not.EqualTo(rotated), "Mirroring after rotation should change tilemap text");
    }
}
