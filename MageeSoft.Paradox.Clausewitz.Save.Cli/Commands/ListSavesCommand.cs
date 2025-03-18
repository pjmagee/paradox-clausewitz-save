using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Games;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Commands;

public class ListSavesCommand : Command
{
    private readonly Option<GameType> _gameOption;
    private readonly Option<string> _sortOption;
    private readonly Option<bool> _fullPathOption;
    private readonly Option<bool> _numberedOption;

    public ListSavesCommand() : base("list", "List save files for Paradox games")
    {
        _gameOption = new Option<GameType>(
            aliases: ["--game", "-g"],
            description: "The game to list save files for (Stellaris, CrusaderKings3, HeartsOfIron4, Victoria3)",
            getDefaultValue: () => GameType.Stellaris);

        _sortOption = new Option<string>(
            aliases: ["--sort", "-s"],
            description: "Sort by: name, date, size",
            getDefaultValue: () => "date");
            
        _fullPathOption = new Option<bool>(
            aliases: ["--full-path", "-f"],
            description: "Display full path instead of parent directory and filename",
            getDefaultValue: () => false);
            
        _numberedOption = new Option<bool>(
            aliases: ["--numbered", "-n"],
            description: "Display a numbered list for easy reference with the summarize command",
            getDefaultValue: () => true);
        
        AddOption(_gameOption);
        AddOption(_sortOption);
        AddOption(_fullPathOption);
        AddOption(_numberedOption);
        
        this.Handler = CommandHandler.Create<GameType, string, bool, bool, IHost>(HandleCommand);
    }
    
    private int HandleCommand(GameType game, string sort, bool fullPath, bool numbered, IHost host)
    {
        var logger = host.Services.GetRequiredService<ILogger<ListSavesCommand>>();
        var saveServices = host.Services.GetServices<IGameSaveService>();
        var saveService = saveServices.FirstOrDefault(s => s.GameType == game);

        if (saveService == null)
        {
            Console.WriteLine($"No save service found for game: {game}");
            return 1;
        }
        
        var saveFiles = saveService.FindSaveFiles().ToList();
        
        // Sort the files based on the option
        saveFiles = sort?.ToLower() switch
        {
            "name" => saveFiles.OrderBy(f => f.Name).ToList(),
            "size" => saveFiles.OrderByDescending(f => f.Length).ToList(),
            _ => saveFiles.OrderByDescending(f => f.LastWriteTime).ToList()
        };
        
        if (!saveFiles.Any())
        {
            Console.WriteLine($"No save files found for {game}.");
            Console.WriteLine($"Default save directory: {saveService.DefaultSaveDirectory}");
            return 0;
        }
        
        Console.WriteLine($"Found {saveFiles.Count} save files for {game}:");
        Console.WriteLine();
        
        // Create a cache file with the paths for the summarize command
        var cachePath = Path.Combine(Path.GetTempPath(), $"paradox-sav-parser-{game.ToString().ToLower()}-cache.txt");
        using (var writer = new StreamWriter(cachePath))
        {
            for (int i = 0; i < saveFiles.Count; i++)
            {
                writer.WriteLine($"{i + 1}|{saveFiles[i].FullName}");
            }
        }
        
        for (int i = 0; i < saveFiles.Count; i++)
        {
            var file = saveFiles[i];
            var size = FormatFileSize(file.Length);
            var lastModified = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
            
            if (numbered)
            {
                Console.Write($"[{i + 1}] ");
            }
            
            if (fullPath)
            {
                Console.WriteLine($"{file.FullName}");
            }
            else
            {
                // Get parent directory and file name
                var parentDir = file.Directory?.Name ?? string.Empty;
                var displayPath = $".../{parentDir}/{file.Name}";
                
                Console.WriteLine($"{displayPath}");
            }
            
            Console.WriteLine($"  Size: {size}, Last Modified: {lastModified}");
            Console.WriteLine();
        }
        
        Console.WriteLine("To view details of a save file, use:");
        Console.WriteLine($"  paradox-clausewitz-sav summarize --game {game.ToString().ToLower()} --number <number>");
        Console.WriteLine("  or");
        Console.WriteLine($"  paradox-clausewitz-sav summarize --game {game.ToString().ToLower()} <path-to-file>");
        
        return 0;
    }
    
    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }
} 