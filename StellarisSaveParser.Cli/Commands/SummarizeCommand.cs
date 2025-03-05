using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StellarisSaveParser.Cli.Services;

namespace StellarisSaveParser.Cli.Commands;

public class SummarizeCommand : Command
{
    private readonly Argument<FileInfo?> _fileArgument;
    private readonly Option<int?> _numberOption;

    public SummarizeCommand() : base("summarize", "Display a summary of a Stellaris save file")
    {
        _fileArgument = new Argument<FileInfo?>(
            name: "file",
            description: "The save file to summarize",
            getDefaultValue: () => null);
        
        _numberOption = new Option<int?>(
            aliases: ["--number", "-n"],
            description: "The number of the save file from the list command",
            getDefaultValue: () => null);
        
        AddArgument(_fileArgument);
        AddOption(_numberOption);
        
        this.Handler = CommandHandler.Create<FileInfo?, int?, IHost>(HandleCommand);
    }
    
    private int HandleCommand(FileInfo? file, int? number, IHost host)
    {
        var logger = host.Services.GetRequiredService<ILogger<SummarizeCommand>>();
        var saveService = host.Services.GetRequiredService<StellarisSaveService>();
        
        // If number is provided, try to get the file from the cache
        if (number.HasValue)
        {
            var cachePath = Path.Combine(Path.GetTempPath(), "stellaris-sav-parser-cache.txt");
            if (!File.Exists(cachePath))
            {
                Console.WriteLine("No save file list cache found. Please run the 'list' command first.");
                return 1;
            }
            
            try
            {
                var lines = File.ReadAllLines(cachePath);
                if (number.Value <= 0 || number.Value > lines.Length)
                {
                    Console.WriteLine($"Invalid save file number: {number.Value}. Valid range is 1-{lines.Length}.");
                    return 1;
                }
                
                var filePath = lines[number.Value - 1].Split('|')[1];
                file = new FileInfo(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading save file cache: {ex.Message}");
                return 1;
            }
        }
        
        // Ensure we have a file to process
        if (file == null)
        {
            Console.WriteLine("No file specified. Please provide a file path or use --number option.");
            return 1;
        }
        
        if (!file.Exists)
        {
            Console.WriteLine($"File not found: {file.FullName}");
            return 1;
        }
        
        if (!file.Extension.Equals(".sav", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"File is not a Stellaris save file: {file.FullName}");
            return 1;
        }
        
        var summary = saveService.GetSaveSummary(file);
        
        if (summary.HasError)
        {
            Console.WriteLine($"Error analyzing save file: {summary.Error}");
            return 1;
        }
        
        Console.WriteLine($"Save File Summary: {summary.FileName}");
        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"File Path:      {summary.FilePath}");
        Console.WriteLine($"File Size:      {summary.GetFormattedSize()}");
        Console.WriteLine($"Last Modified:  {summary.LastModified:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Game Version:   {summary.GameVersion}");
        Console.WriteLine($"Game Date:      {summary.GameDate}");
        Console.WriteLine($"Empire Name:    {summary.EmpireName}");
        Console.WriteLine($"Ironman:        {(summary.IsIronman ? "Yes" : "No")}");
        Console.WriteLine($"Fleet Count:    {summary.FleetCount}");
        Console.WriteLine($"Planet Count:   {summary.PlanetCount}");
        
        Console.WriteLine();
        Console.WriteLine("Top-level Sections:");
        foreach (var section in summary.TopLevelSections.Take(20))
        {
            Console.WriteLine($"  - {section}");
        }
        
        if (summary.TopLevelSections.Count > 20)
        {
            Console.WriteLine($"  ... and {summary.TopLevelSections.Count - 20} more sections");
        }
        
        return 0;
    }
} 