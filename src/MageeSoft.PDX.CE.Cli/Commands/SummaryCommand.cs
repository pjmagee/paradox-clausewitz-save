using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Text;
using System.Text.Json;
using MageeSoft.PDX.CE.Cli.Commands.Options;
using MageeSoft.PDX.CE.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MageeSoft.PDX.CE.Cli.Commands;

/// <summary>
/// Command to display a summary of a save file
/// </summary>
public class SummaryCommand : BaseCommand
{
    public SummaryCommand() : base("summary", "Display a summary of a save file")
    {
        AddOption(new GameOption());
        AddOption(new NumberOption());
        AddOption(new FormatOption());
        AddOption(new OutputOption());
        Handler = CommandHandler.Create<IHost, IConsole, string, int, string, FileInfo?>(HandleCommand);
    }
    
    private void HandleCommand(IHost host, IConsole console, string game, int number, string format, FileInfo? output)
    {
        var gameServiceManager = host.Services.GetRequiredService<GameServiceManager>();
        
        var saveFile = GetSaveFileWithProvider(gameServiceManager, number, game);
        if (saveFile == null) return;
        
        var (file, provider) = saveFile.Value;
        var summary = provider.GetSaveSummary(file);
        var result = "json".Equals(format) ? JsonSerializer.Serialize(summary, CliJsonSerializerContext.Default.SaveSummary) : FormatSummaryAsText(summary);

        if (output != null)
        {
            File.WriteAllText(output.FullName, result);
            console.WriteLine($"Summary written to {output.FullName}");
        }
        else
        {
            console.WriteLine(result);
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
    
    private string FormatSummaryAsText(SaveSummary summary)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Game: {summary.GameName}");
        builder.AppendLine($"File Name: {summary.FileName}");
        builder.AppendLine($"File Size: {FormatFileSize(summary.FileSize)}");
        builder.AppendLine($"Last Modified: {summary.LastModified}");
        
        if (!string.IsNullOrEmpty(summary.Version))
        {
            builder.AppendLine($"Game Version: {summary.Version}");
        }
        
        builder.AppendLine($"Ironman: {(summary.Ironman ? "Yes" : "No")}");
        return builder.ToString();
    }
} 