using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using BMP2Tile;

namespace bmp2tile.Tests;

[TestFixture]
public class TilemapRotationTests
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

    [TestCase(0)]
    [TestCase(90)]
    [TestCase(180)]
    [TestCase(270)]
    public void RotatedTilemap_DoesNotThrow(int angle)
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        _conv.TilemapRotation = angle;
        Assert.That(() => _conv.GetTilemapAsText(), Throws.Nothing, $"Rotation {angle} should not throw");
    }

    [Test]
    public void RotatedTilemap_90_ChangesDimensions()
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        var orig = _conv.GetTilemapAsText();
        _conv.TilemapRotation = 90;
        var rotated = _conv.GetTilemapAsText();
        Assert.That(rotated, Is.Not.EqualTo(orig), "90 degree rotation should change tilemap text");
    }

    [Test]
    public void RotatedTilemap_180_ChangesDimensions()
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        var orig = _conv.GetTilemapAsText();
        _conv.TilemapRotation = 180;
        var rotated = _conv.GetTilemapAsText();
        Assert.That(rotated, Is.Not.EqualTo(orig), "180 degree rotation should change tilemap text");
    }

    [Test]
    public void RotatedTilemap_270_ChangesDimensions()
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        var orig = _conv.GetTilemapAsText();
        _conv.TilemapRotation = 270;
        var rotated = _conv.GetTilemapAsText();
        Assert.That(rotated, Is.Not.EqualTo(orig), "270 degree rotation should change tilemap text");
    }

    [Test]
    public void RotatedTilemap_InvalidAngle_Throws()
    {
        _conv.Filename = Path.Combine(_testDir, "akmw.bmp");
        Assert.That(() => _conv.TilemapRotation = 45, Throws.TypeOf<ArgumentException>());
    }
}
