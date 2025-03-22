using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Commands.Options;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Commands;

/// <summary>
/// Command to list available save files
/// </summary>
public class ListCommand : BaseCommand
{
    public ListCommand() : base("list", "List available save files")
    {
        AddOption(new GameOption());
        Handler = CommandHandler.Create<IHost, IConsole, string>(HandleCommand);
    }
    
    private void HandleCommand(IHost host, IConsole console, string game)
    {
        var manager = host.Services.GetRequiredService<GameServiceManager>();
        var resolver = host.Services.GetRequiredService<GamePathResolver>();
        
        var cacheDir = Path.Combine(Path.GetTempPath(), "paradox-cli-cache");
        if (!Directory.Exists(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
        }
        
        var saveFiles = new List<SaveFileInfo>();
        
        if (string.IsNullOrEmpty(game))
        {
            console.WriteLine("Searching for save files across all supported games");
            
            foreach (var provider in manager.GetAllProviders())
            {
                console.WriteLine($"  - {provider.GameName}");
            }
            
            saveFiles = manager.FindAllGameSaveFiles().ToList();
        }
        else
        {
            // Find save files for specific game
            var provider = manager.GetProviderByName(game);
            
            if (provider == null)
            {
                console.WriteLine($"Error: Game '{game}' is not supported.");
                console.WriteLine("Supported games:");
                
                foreach (var supportedProvider in manager.GetAllProviders())
                {
                    console.WriteLine($"  - {supportedProvider.GameName}");
                }
                
                return;
            }
            
            var searchPaths = resolver.GetPotentialSavePaths(provider.GameName);
            console.WriteLine($"Searching for {provider.GameName} save files in:");
                    
            foreach (var path in searchPaths)
            {
                console.WriteLine($"  - {path}");
            }
            
            var index = 1;
            var files = provider.FindSaveFiles();
            
            foreach (var file in files)
            {
                saveFiles.Add(new SaveFileInfo
                {
                    Number = index++,
                    GameType = provider.GameType,
                    GameName = provider.GameName,
                    Path = file.FullName,
                    Name = file.Name,
                    Directory = file.Directory?.Name ?? "",
                    Size = file.Length,
                    LastModified = file.LastWriteTime
                });
            }
        }
        
        if (!saveFiles.Any())
        {
            if (string.IsNullOrEmpty(game))
            {
                console.WriteLine("No save files found for any supported games.");
            }
            else
            {
                console.WriteLine($"No save files found for {game}.");
            }
            
            return;
        }

        // Cache save files for later use with number parameter
        CacheSaveFiles(saveFiles);
        
        // Group by game if showing all games
        if (string.IsNullOrEmpty(game))
        {
            var gameGroups = saveFiles.GroupBy(s => s.GameName).OrderBy(g => g.Key);
            
            console.WriteLine($"Found {saveFiles.Count} save files across {gameGroups.Count()} games:");
            
            foreach (var gameGroup in gameGroups)
            {
                console.WriteLine($"{gameGroup.Key} ({gameGroup.Count()} files):");
                
                foreach (var file in gameGroup)
                {
                    console.WriteLine($"  [{file.Number}] {file.Name} - {FormatFileSize(file.Size)} - {file.LastModified}");
                }
            }
        }
        else
        {
            console.WriteLine($"Found {saveFiles.Count} {game} save files:");
            
            foreach (var file in saveFiles)
            {
                console.WriteLine($"[{file.Number}] {file.Name} - {FormatFileSize(file.Size)} - {file.LastModified}");
            }
        }
        
        console.WriteLine("To view details of a save file, use:");
        console.WriteLine("  dotnet run -- summary --number <number>");
    }
    
    private void CacheSaveFiles(List<SaveFileInfo> saveFiles)
    {
        var cachePath = Path.Combine(Path.GetTempPath(), "paradox-save-parser-cache.txt");
        using var writer = new StreamWriter(cachePath);
        
        foreach (var file in saveFiles)
        {
            writer.WriteLine($"{file.Number}|{file.Path}|{file.GameName}");
        }
    }
} 