using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BMP2Tile
{
    internal class ArgParser
    {
        private readonly Action<string> _extraParameterHandler;
        private readonly Func<IList<IList<string>>> _helpTextExtraHandler;

        public ArgParser(Action<string> extraParameterHandler, Func<IList<IList<string>>> helpTextExtraHandler)
        {
            _extraParameterHandler = extraParameterHandler;
            _helpTextExtraHandler = helpTextExtraHandler;
        }

        private class ArgHandler
        {
            public IList<string> Names { get; set; }
            public string Description { get; set; }
            public Action<string> Action { get; set; }
            public string ValueName { get; set; }
        }
        private readonly Dictionary<string, ArgHandler> _args = new Dictionary<string, ArgHandler>();

        public void Add(IList<string> names, string description, Action<string> action, string argName = null)
        {
            var arg = new ArgHandler
            {
                Names = names,
                Description = description,
                Action = action,
                ValueName = argName
            };
            foreach (var name in names)
            {
                _args.Add(name, arg);
            }
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
                if (arg.StartsWith("-"))
                {
                    // We remove any number of leading - or /
                    var argName = Regex.Replace(arg, "^[-/]+", "");
                    if (argName == "?" || argName == "h" || argName == "help" || !_args.TryGetValue(argName, out var handler))
                    {
                        Console.Error.WriteLine($"Unknown action {arg}");
                        ShowHelp();
                        return 1;
                    }

                    // We have a match, invoke the handler with the right number of args
                    if (handler.ValueName != null)
                    {
                        if (i == args.Length - 1)
                        {
                            throw new Exception($"Missing value for action {arg} <{handler.ValueName}>");
                        }

                        handler.Action(args[++i]);
                    }
                    else
                    {
                        handler.Action(null);
                    }
                }
                else
                {
                    _extraParameterHandler(arg);
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
            Console.Error.WriteLine("Usage: bmp2tile <input file> <-action> <-action ...>");
            Console.Error.WriteLine("Actions take effect in order from left to right. Multiple input files can be processed this way.");
            Console.Error.WriteLine("\nActions:");

            Console.Error.WriteLine(PrintInGrid(GetArgs().ToList()));

            Console.Error.WriteLine(PrintInGrid(_helpTextExtraHandler()));
        }

        private IEnumerable<IList<string>> GetArgs()
        {
            foreach (var arg in _args.Values)
            {
                if (arg.ValueName == null)
                {
                    yield return new[] { $"-{arg.Names[0]}", arg.Description };
                }
                else
                {
                    yield return new[] { $"-{arg.Names[0]} <{arg.ValueName}>", arg.Description };
                }

                foreach (var s in arg.Names.Skip(1))
                {
                    yield return new[] { $"-{s}", $"Synonym of -{arg.Names[0]}" };
                }
            }
        }
    }
}