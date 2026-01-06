using System;
using System.Drawing;
using NUnit.Framework;
using BMP2Tile;

namespace bmp2tile.Tests;

[TestFixture]
public class UtilitiesTests
{
    [TestCase("#FF0000", 255, 0, 0)]
    [TestCase("#00FF00", 0, 255, 0)]
    [TestCase("#0000FF", 0, 0, 255)]
    [TestCase("#FFFFFF", 255, 255, 255)]
    [TestCase("#000000", 0, 0, 0)]
    [TestCase("#ff8040", 255, 128, 64)]
    public void ParseHexColour_ValidInputs_ReturnsExpectedRgb(string hex, int r, int g, int b)
    {
        Assert.That(
            Utilities.ParseHexColour(hex), 
            Is.EqualTo(Color.FromArgb(r, g, b)));
    }

    [Test]
    public void ParseHexColour_LowerAndUpperCase_Accepted()
    {
        var c1 = Utilities.ParseHexColour("#ff00ff");
        var c2 = Utilities.ParseHexColour("#FF00FF");
        Assert.That(c1.ToArgb(), Is.EqualTo(c2.ToArgb()));
    }

    [TestCase("FF0000")]    // missing #
    [TestCase("#FF00")]     // too short
    [TestCase("#FF0000FF")] // too long (alpha included)
    [TestCase("#GGGGGG")]   // invalid hex chars
    [TestCase("")]          // empty
    [TestCase("#")]         // only hash
    public void ParseHexColour_InvalidInputs_ThrowException(string input)
    {
        Assert.That(() => Utilities.ParseHexColour(input), Throws.Exception.TypeOf<Exception>().With.Message.Contains("#RRGGBB"));
    }

    [Test]
    public void ParseHexColour_Null_ThrowsArgumentNullException()
    {
        Assert.That(() => Utilities.ParseHexColour(null), Throws.TypeOf<ArgumentNullException>());
    }
}
