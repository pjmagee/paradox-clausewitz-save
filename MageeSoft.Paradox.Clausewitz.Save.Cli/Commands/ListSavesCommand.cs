using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Text.Json;
using System.Text.Json.Serialization;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Commands;

// Add a source generator context for SaveFileInfo
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<SaveFileInfo>))]
internal partial class SaveFileJsonContext : JsonSerializerContext
{
}

public class SaveFileInfo
{
    public int Number { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
    public long Size { get; set; }
    public string FormattedSize { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
}

public class ListSavesCommand : Command
{
    private readonly Option<string> _sortOption;
    private readonly Option<bool> _fullPathOption;
    private readonly Option<bool> _numberedOption;
    private readonly Option<bool> _jsonOption;

    public ListSavesCommand() : base("list", "List Stellaris save files")
    {
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

        _jsonOption = new Option<bool>(
            aliases: ["--json", "-j"],
            description: "Output as JSON",
            getDefaultValue: () => false);
        
        AddOption(_sortOption);
        AddOption(_fullPathOption);
        AddOption(_numberedOption);
        AddOption(_jsonOption);
        
        this.Handler = CommandHandler.Create<string, bool, bool, bool, IHost>(HandleCommand);
    }
    
    private int HandleCommand(string sort, bool fullPath, bool numbered, bool json, IHost host)
    {
        var logger = host.Services.GetRequiredService<ILogger<ListSavesCommand>>();
        var saveService = host.Services.GetRequiredService<IGameSaveService>();
        
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
            if (json)
            {
                Console.WriteLine("[]");
                return 0;
            }
            
            Console.WriteLine("No Stellaris save files found.");
            Console.WriteLine($"Default save directory: {saveService.DefaultSaveDirectory}");
            return 0;
        }
        
        // Create a cache file with the paths for the summarize command
        var cachePath = Path.Combine(Path.GetTempPath(), "stellaris-sav-parser-cache.txt");
        using (var writer = new StreamWriter(cachePath))
        {
            for (int i = 0; i < saveFiles.Count; i++)
            {
                writer.WriteLine($"{i + 1}|{saveFiles[i].FullName}");
            }
        }

        if (json)
        {
            var fileList = saveFiles.Select((file, index) => new SaveFileInfo
            {
                Number = index + 1,
                Path = file.FullName,
                Name = file.Name,
                Directory = file.Directory?.Name ?? string.Empty,
                Size = file.Length,
                FormattedSize = FormatFileSize(file.Length),
                LastModified = file.LastWriteTime
            }).ToList();

            Console.WriteLine(JsonSerializer.Serialize(fileList, SaveFileJsonContext.Default.ListSaveFileInfo));
            return 0;
        }
        
        Console.WriteLine($"Found {saveFiles.Count} Stellaris save files:");
        Console.WriteLine();
        
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
        Console.WriteLine("  paradox-clausewitz-sav summarize --number <number>");
        Console.WriteLine("  or");
        Console.WriteLine("  paradox-clausewitz-sav summarize <path-to-file>");
        
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