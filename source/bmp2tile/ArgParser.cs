using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BMP2Tile;

internal class ArgParser(
    Action<string> extraParameterHandler,
    Func<IEnumerable<string>> helpTextHandler,
    Func<IList<IList<string>>> helpTextExtraHandler)
{
    private class ArgHandler
    {
        public IList<string> Names { get; init; }
        public string Description { get; init; }
        public Action<Dictionary<string, string>> Action { get; init; }
        public string[] ValueNames { get; init; }
    }
    private readonly Dictionary<string, ArgHandler> _args = new();

    public ArgParser Add(IList<string> names, string description, Action<Dictionary<string, string>> action, params string[] valueNames)
    {
        var arg = new ArgHandler
        {
            Names = names,
            Description = description,
            Action = action,
            ValueNames = valueNames
        };
        foreach (var name in names)
        {
            _args.Add(name, arg);
        }

        return this;
    }

    public int Parse(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return 1;
        }
        for (var i = 0; i < args.Length; ++i)
        {
            var arg = args[i];
            if (arg.StartsWith('-') || arg.StartsWith('/'))
            {
                // We remove any number of leading - or /
                var argName = Regex.Replace(arg, "^[-/]+", "");
                if (argName is "?" or "h" or "help")
                {
                    ShowHelp();
                    return 0;
                }
                if (!_args.TryGetValue(argName, out var handler))
                {
                    Console.Error.WriteLine($"Unknown action {arg}");
                    ShowHelp();
                    return 1;
                }

                // We have a match, invoke the handler with the args
                // We build a dictionary of the args
                var actionArgs = new Dictionary<string, string>();
                foreach (var valueName in handler.ValueNames)
                {
                    ++i;
                    if (i == args.Length)
                    {
                        throw new AppException($"Not enough parameters while processing {arg}");
                    }
                    actionArgs[valueName] = args[i];
                }

                try
                {
                    handler.Action(actionArgs);
                }
                catch (Exception e)
                {
                    throw new Exception($"Error parsing argument: {arg} {string.Join(" ", actionArgs.Values)}", e);
                }
            }
            else
            {
                extraParameterHandler(arg);
            }
        }

        return 0;
    }

    private static string PrintInGrid(IList<IList<string>> rows)
    {
        // Get the max width of each column
        var maxCount = rows.Max(x => x.Count);
        var widths = Enumerable
            .Range(0, maxCount)
            .Select(n => rows
                .Where(r => r.Count > n && r.Count > 1)
                .Max(r => r[n].Length) + 2)
            .ToList();

        // Then print each padded to the max width, except the last one
        var sb = new StringBuilder();
        foreach (var row in rows)
        {
            for (var i = 0; i < row.Count; i++)
            {
                sb.Append(i == row.Count - 1 ? row[i] : row[i].PadRight(widths[i]));
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private void ShowHelp()
    {
        foreach (var line in helpTextHandler())
        {
            Console.Error.WriteLine(line);
        }

        Console.Error.WriteLine("\nParameters:");
        Console.Error.WriteLine(PrintInGrid(GetArgs().ToList()));

        Console.Error.WriteLine(PrintInGrid(helpTextExtraHandler()));
    }

    private IEnumerable<IList<string>> GetArgs()
    {
        // We iterate over the "primary" args, so we can list synonyms separately
        foreach (var arg in _args
                     .Where(x => x.Key == x.Value.Names[0])
                     .Select(x => x.Value))
        {
            yield return [$"-{arg.Names[0]}{PrintArgs(arg.ValueNames)}", arg.Description];

            foreach (var s in arg.Names.Skip(1))
            {
                yield return [$"-{s}", $"Synonym of -{arg.Names[0]}"];
            }
        }
    }

    private string PrintArgs(string[] names)
    {
        return names.Length == 0 
            ? "" 
            : names.Aggregate("", (s, arg) => s + $" <{arg}>");
    }
}