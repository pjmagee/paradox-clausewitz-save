using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.IO;
using MageeSoft.PDX.CE.Cli.Commands.Options;
using MageeSoft.PDX.CE.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using MageeSoft.PDX.CE.Save;

namespace MageeSoft.PDX.CE.Cli.Commands;

/// <summary>
/// Command to query values from a save file using a PdxQuery path
/// </summary>
public class QueryCommand : BaseCommand
{
    public QueryCommand() : base("query", "Query values from a save file using a path expression")
    {
        AddOption(new GameOption());
        AddOption(new NumberOption());
        AddOption(new SaveFileOption());
        AddOption(new QueryOption());
        AddOption(new FormatOption());
        AddOption(new OutputOption());
        AddOption(new ShowPathsOption());
        Handler = CommandHandler.Create<IHost, IConsole, string, int?, FileInfo?, string, string, FileInfo?, bool>(HandleCommand);
    }

    private void HandleCommand(IHost host, IConsole console, string game, int? number, FileInfo? saveFile, string query, string format, FileInfo? output, bool showPaths)
    {
        // Validate that exactly one of number or saveFile is provided
        if ((number.HasValue && saveFile != null) || (!number.HasValue && saveFile == null))
        {
            console.Error.WriteLine("You must provide either --number/-n or --save-file/-s, but not both.");
            return;
        }

        FileInfo file;
        IGameFilesProvider? provider = null;
        if (saveFile != null)
        {
            file = saveFile;
            provider = host.Services.GetRequiredService<GameServiceManager>().GetProviderByName(game);
        }
        else
        {
            var gameServiceManager = host.Services.GetRequiredService<GameServiceManager>();
            var saveFileResult = GetSaveFileWithProvider(gameServiceManager, number!.Value, game);
            if (saveFileResult == null) return;
            (file, provider) = saveFileResult.Value;
        }

        // Use the provider to get the parsed root object for querying
        PdxObject? root = null;
        if (provider != null && provider.GameType == GameType.Stellaris)
        {
            StellarisSave stellarisSave = StellarisSave.FromSave(file);
            root = stellarisSave.GameState;
        }
        
        // TODO: Add support for other games here
        if (root == null)
        {
            console.Error.WriteLine($"Querying is not supported for game type: {provider?.GameType}");
            return;
        }

        var pdxQuery = new PdxQuery(root);
        
        List<(string Path, string Value)> resultsWithPaths = new();
        
        bool isRecursive = false;
        
        if (query.StartsWith(".. | .") && query.EndsWith("?"))
        {
            // jq-style recursive key search: .. | .key?
            var key = query.Substring(6, query.Length - 7);
            
            foreach (var (path, value) in pdxQuery.RecursiveKeySearch(key))
                resultsWithPaths.Add((path, PdxQuery.ElementToString(value)!));
            
            isRecursive = true;
        }
        else if (query.StartsWith(".. | select(. == "))
        {
            // jq-style recursive value search: .. | select(. == "value")
            var value = query.Substring(17, query.Length - 18).Trim('"');
            foreach (var (path, val) in pdxQuery.RecursiveValueSearch(value))
                resultsWithPaths.Add((path, PdxQuery.ElementToString(val)));
            isRecursive = true;
        }
        else if (query.StartsWith(".. | select(contains(") && query.EndsWith(')') )
        {
            // jq-style recursive substring value search: .. | select(contains("foo"))
            var i = query.LastIndexOf("select(contains(");
            var end = query.IndexOf(")");
            var start = i + "select(contains(".Length;
            var length = end - start;
            var value = query.Substring(start, length);

            foreach (var (path, val) in pdxQuery.RecursiveValueSubstringSearch(value))
                resultsWithPaths.Add((path, PdxQuery.ElementToString(val)));

            isRecursive = true;
        }
        
        if (isRecursive)
        {
            string outputString;
            
            if (format == "json")
            {
                outputString = JsonSerializer.Serialize(
                    showPaths ? resultsWithPaths : resultsWithPaths.Select(r => r.Value),
                    CliJsonSerializerContext.Default.String);
            }
            else // text
            {
                outputString = string.Join('\n', showPaths ? resultsWithPaths.Select(r => $"{r.Path}: {r.Value}") : resultsWithPaths.Select(r => r.Value));
            }
            
            if (output != null)
            {
                File.WriteAllText(output.FullName, outputString);
                console.WriteLine($"Query results written to {output.FullName}");
            }
            else
            {
                console.WriteLine(outputString);
            }
            return;
        }
        // Default: path-based query
        var results = pdxQuery.GetList(query);
        string defaultOutputString;
        if (format == "json")
        {
            if (showPaths)
            {
                var withPaths = results.Select((v, i) => ($"{query}[{i}]", PdxQuery.ElementToString(v))).ToList();
                defaultOutputString = JsonSerializer.Serialize(withPaths, CliJsonSerializerContext.Default.String);
            }
            else
            {
                defaultOutputString = JsonSerializer.Serialize(results.Select(PdxQuery.ElementToString), CliJsonSerializerContext.Default.String);
            }
        }
        else // text
        {
            if (showPaths)
            {
                var withPaths = results.Select((v, i) => $"{query}[{i}]: {PdxQuery.ElementToString(v)}");
                defaultOutputString = string.Join("\n", withPaths);
            }
            else
            {
                defaultOutputString = string.Join("\n", results.Select(PdxQuery.ElementToString));
            }
        }
        if (output != null)
        {
            File.WriteAllText(output.FullName, defaultOutputString);
            console.WriteLine($"Query results written to {output.FullName}");
        }
        else
        {
            console.WriteLine(defaultOutputString);
        }
    }

    private (FileInfo, IGameFilesProvider)? GetSaveFileWithProvider(GameServiceManager gameServiceManager, int number, string? gameName)
    {
        var cachePath = Path.Combine(Path.GetTempPath(), "paradox-save-parser-cache.txt");

        if (!File.Exists(cachePath))
        {
            Console.WriteLine("Error: No save file list cache found. Please run the 'list' command first.");
            return null;
        }

        try
        {
            var lines = File.ReadAllLines(cachePath);

            if (number <= 0 || number > lines.Length)
            {
                Console.WriteLine($"Error: Invalid save file number: {number}. Valid range is 1-{lines.Length}.");
                return null;
            }

            var parts = lines[number - 1].Split('|');
            if (parts.Length < 3)
            {
                Console.WriteLine("Error: Invalid format in cache file. Please run the 'list' command again.");
                return null;
            }

            var filePath = parts[1];
            var gameType = parts[2];

            // If game is specified, make sure it matches
            if (!string.IsNullOrEmpty(gameName) &&
                !gameType.Equals(gameName, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Error: Save file #{number} is from game '{gameType}', but '{gameName}' was requested.");
                return null;
            }

            var file = new FileInfo(filePath);

            if (!file.Exists)
            {
                Console.WriteLine($"Error: Save file not found at {filePath}");
                return null;
            }

            // Get the appropriate provider for this game
            var provider = gameServiceManager.GetProviderByName(gameType);
            if (provider == null)
            {
                Console.WriteLine($"Error: No provider found for game type '{gameType}'.");
                return null;
            }

            return (file, provider);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Failed to read save file cache: {ex.Message}");
            return null;
        }
    }
} 