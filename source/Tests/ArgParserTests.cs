using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BMP2Tile;
using NUnit.Framework;

namespace bmp2tile.Tests;

[TestFixture]
public class ArgParserTests
{
    [Test]
    public void NoArgs()
    {
        var parser = new ArgParser(
            _ => { },
            () => ["This is the help text at the start"],
            () => [["Extra grid stuff", "After the args"]]);

        parser.Add(["dummy", "synonym"], "A dummy arg", _ => { }, "v");

        var sw = new StringWriter();
        var original = Console.Error;
        try
        {
            Console.SetError(sw);
            var ret = parser.Parse([]);
            Assert.That(ret, Is.EqualTo(1));
            var output = sw.ToString();
            Assert.That(output, Is.EqualTo(
                """
                This is the help text at the start
                
                Parameters:
                -dummy <v>  A dummy arg
                -synonym    Synonym of -dummy
                
                Extra grid stuff  After the args
                
                
                """));
        }
        finally
        {
            Console.SetError(original);
        }
    }

    [Test]
    public void UnknownArg()
    {
        var parser = new ArgParser(_ => { }, () => [], () => []);
        parser.Add([ "known" ], "desc", _ => { });

        var sw = new StringWriter();
        var original = Console.Error;
        try
        {
            Console.SetError(sw);
            var ret = parser.Parse(["-unknown"]);
            Assert.That(ret, Is.EqualTo(1));
            Assert.That(sw.ToString(), Does.Contain("Unknown action -unknown"));
        }
        finally
        {
            Console.SetError(original);
        }
    }

    [TestCase("-h")]
    [TestCase("/help")]
    [TestCase("-?")]
    public void HelpFlags(string flag)
    {
        var parser = new ArgParser(_ => { }, () => ["Usage"], () => []);
        parser.Add((List<string>)["a"], "desc", _ => { });

        var sw = new StringWriter();
        var original = Console.Error;
        try
        {
            Console.SetError(sw);
            var ret = parser.Parse([flag]);
            Assert.That(ret, Is.EqualTo(0));
            Assert.That(sw.ToString(), Does.Contain("Usage"));
        }
        finally
        {
            Console.SetError(original);
        }
    }

    [Test]
    public void ArgWithParameters()
    {
        var extras = new List<string>();
        var received = new Dictionary<string, string>();
        var parser = new ArgParser(
            extra => extras.Add(extra),
            () => [],
            () => []);

        parser.Add(
            ["o", "option"], 
            "desc", 
            args =>
            {
                foreach (var kv in args)
                {
                    received[kv.Key] = kv.Value;
                }
            }, 
            "file", 
            "mode");

        var ret = parser.Parse(["-o", "input.txt", "binary", "positional"]);
        Assert.That(ret, Is.EqualTo(0));
        Assert.That(received["file"], Is.EqualTo("input.txt"));
        Assert.That(received["mode"], Is.EqualTo("binary"));
        Assert.That(extras.Single(), Is.EqualTo("positional"));
    }

    [Test]
    public void NotEnoughParameters()
    {
        var parser = new ArgParser(_ => { }, () => [], () => []);
        parser.Add(["o"], "desc", _ => { }, "one", "two");

        Assert.Throws<AppException>(() => parser.Parse(["-o", "onlyone"]));
    }

    [Test]
    public void ArgHandlerThrows()
    {
        var parser = new ArgParser(_ => { }, () => [], () => []);
        parser.Add(["o"], "desc", _ => throw new AppException("Hello"));

        Assert.That(
            () => parser.Parse(["-o"]),
            Throws.Exception
                .With.Message.Contains("Error parsing argument")
                .And.InnerException.Message.Contains("Hello"));
    }

}