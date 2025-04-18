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
        Handler = CommandHandler.Create<IHost, IConsole, string, int?, FileInfo?, string, string, FileInfo?>(HandleCommand);
    }

    private void HandleCommand(IHost host, IConsole console, string game, int? number, FileInfo? saveFile, string query, string format, FileInfo? output)
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
            provider = host.Services.GetRequiredService<GameServiceManager>().GetProviderByName(game) ??
                       host.Services.GetRequiredService<GameServiceManager>().GetProviderByName("stellaris");
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
        var results = pdxQuery.GetList(query);

        string outputString;
        if (format == "json")
        {
            outputString = JsonSerializer.Serialize(results.Select(PdxQuery.ElementToString), CliJsonSerializerContext.Default.String);
        }
        else // text
        {
            outputString = string.Join("\n", results.Select(PdxQuery.ElementToString));
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