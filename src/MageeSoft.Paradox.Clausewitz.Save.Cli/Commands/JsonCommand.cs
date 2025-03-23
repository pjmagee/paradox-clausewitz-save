using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Commands.Options;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Commands;

/// <summary>
/// Command to export a save file as JSON
/// </summary>
public class JsonCommand : BaseCommand
{
    public JsonCommand() : base("json", "Export a save file as JSON")
    {
        AddOption(new GameOption());
        AddOption(new NumberOption());
        AddOption(new OutputOption());
        Handler = CommandHandler.Create<IHost, IConsole, string, int, FileInfo?>(HandleCommand);
    }

    private void HandleCommand(IHost host, IConsole console, string game, int number, FileInfo? output)
    {
        var cachePath = Path.Combine(Path.GetTempPath(), "paradox-save-parser-cache.txt");
        
        if (!File.Exists(cachePath))
        {
            console.WriteLine("Error: No save file list cache found. Please run the 'list' command first.");
            return;
        }
        
        try
        {
            var lines = File.ReadAllLines(cachePath);
            
            if (number <= 0 || number > lines.Length)
            {
                console.WriteLine($"Error: Invalid save file number: {number}. Valid range is 1-{lines.Length}.");
                return;
            }
            
            var parts = lines[number - 1].Split('|');
            if (parts.Length < 3)
            {
                console.WriteLine("Error: Invalid format in cache file. Please run the 'list' command again.");
                return;
            }
            
            var filePath = parts[1];
            var gameType = parts[2];
            
            // If game is specified, make sure it matches
            if (!string.IsNullOrEmpty(game) && !gameType.Equals(game, StringComparison.OrdinalIgnoreCase))
            {
                console.WriteLine($"Error: Save file #{number} is from game '{gameType}', but '{game}' was requested.");
                return;
            }
            
            var file = new FileInfo(filePath);
            
            if (!file.Exists)
            {
                console.WriteLine($"Error: Save file not found at {filePath}");
                return;
            }
            
            // Get the appropriate provider for this game
            var provider = host.Services.GetRequiredService<GameServiceManager>().GetProviderByName(gameType);
            if (provider == null)
            {
                console.WriteLine($"Error: No provider found for game type '{gameType}'.");
                return;
            }

            // Get the full JSON representation of the save file
            var json = provider.GetSummaryAsJson(file);

            // Output to file or console
            if (output != null)
            {
                File.WriteAllText(output.FullName, json);
                console.WriteLine($"JSON data written to {output.FullName}");
            }
            else
            {
                console.WriteLine(json);
            }
        }
        catch (Exception ex)
        {
            console.WriteLine($"Error: Failed to read save file cache: {ex.Message}");
        }
    }
}