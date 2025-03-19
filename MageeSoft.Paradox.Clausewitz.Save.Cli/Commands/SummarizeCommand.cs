using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Text.Json;
using System.Text.Json.Serialization;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Services.Games.Stellaris;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Commands;

// Error response model
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
}

// JSON context for error responses
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ErrorResponse))]
internal partial class ErrorResponseJsonContext : JsonSerializerContext
{
}

public class SummarizeCommand : Command
{
    private readonly Argument<FileInfo?> _fileArgument;
    private readonly Option<int?> _numberOption;
    private readonly Option<bool> _jsonOption;

    public SummarizeCommand() : base("summarize", "Display a summary of the Stellaris save file")
    {
        _fileArgument = new Argument<FileInfo?>(
            name: "file",
            description: "The save file to summarize", 
            getDefaultValue: () => null);
        
        _numberOption = new Option<int?>(
            aliases: ["--number", "-n"],
            description: "The number of the save file from the list command",
            getDefaultValue: () => null);
        
        _jsonOption = new Option<bool>(
            aliases: ["--json", "-j"],
            description: "Output in JSON format",
            getDefaultValue: () => false);
        
        AddArgument(_fileArgument);
        AddOption(_numberOption);
        AddOption(_jsonOption);
        
        this.Handler = CommandHandler.Create<FileInfo?, int?, bool, IHost>(HandleCommand);
    }
    
    private int HandleCommand(FileInfo? file, int? number, bool json, IHost host)
    {
        var logger = host.Services.GetRequiredService<ILogger<SummarizeCommand>>();
        var saveService = host.Services.GetRequiredService<StellarisSaveService>();
        
        // If number is provided, try to get the file from the cache
        if (number.HasValue)
        {
            var cachePath = Path.Combine(Path.GetTempPath(), "stellaris-sav-parser-cache.txt");
            if (!File.Exists(cachePath))
            {
                if (json)
                {
                    WriteErrorJson("No save file list cache found. Please run the 'list' command first.");
                    return 1;
                }
                Console.WriteLine("No save file list cache found. Please run the 'list' command first.");
                return 1;
            }
            
            try
            {
                var lines = File.ReadAllLines(cachePath);
                if (number.Value <= 0 || number.Value > lines.Length)
                {
                    if (json)
                    {
                        WriteErrorJson($"Invalid save file number: {number.Value}. Valid range is 1-{lines.Length}.");
                        return 1;
                    }
                    Console.WriteLine($"Invalid save file number: {number.Value}. Valid range is 1-{lines.Length}.");
                    return 1;
                }
                
                var filePath = lines[number.Value - 1].Split('|')[1];
                file = new FileInfo(filePath);
            }
            catch (Exception ex)
            {
                if (json)
                {
                    WriteErrorJson($"Error reading save file cache: {ex.Message}");
                    return 1;
                }
                Console.WriteLine($"Error reading save file cache: {ex.Message}");
                return 1;
            }
        }
        
        // Ensure we have a file to process
        if (file == null)
        {
            if (json)
            {
                WriteErrorJson("No file specified. Please provide a file path or use --number option.");
                return 1;
            }
            Console.WriteLine("No file specified. Please provide a file path or use --number option.");
            return 1;
        }
        
        if (!file.Exists)
        {
            if (json)
            {
                WriteErrorJson($"File not found: {file.FullName}");
                return 1;
            }
            Console.WriteLine($"File not found: {file.FullName}");
            return 1;
        }
        
        if (!file.Extension.Equals(".sav", StringComparison.OrdinalIgnoreCase))
        {
            if (json)
            {
                WriteErrorJson($"File is not a Stellaris save file: {file.FullName}");
                return 1;
            }
            Console.WriteLine($"File is not a Stellaris save file: {file.FullName}");
            return 1;
        }
        
        var summary = saveService.GetSaveSummary(file);
        
        if (summary.HasError)
        {
            if (json)
            {
                WriteErrorJson($"Error analyzing save file: {summary.Error}");
                return 1;
            }
            Console.WriteLine($"Error analyzing save file: {summary.Error}");
            return 1;
        }

        // Always output as JSON
        Console.WriteLine(saveService.GetSummary(file));
        
        return 0;
    }
    
    private void WriteErrorJson(string errorMessage)
    {
        var error = new ErrorResponse { Error = errorMessage };
        Console.WriteLine(JsonSerializer.Serialize(error, ErrorResponseJsonContext.Default.ErrorResponse));
    }
} 