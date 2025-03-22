using System.CommandLine;
namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Commands;

/// <summary>
/// Base class for all CLI commands with common functionality
/// </summary>
public abstract class BaseCommand(string name, string description) : Command(name, description)
{
    /// <summary>
    /// Gets a save file from the cache by its number
    /// </summary>
    protected FileInfo? GetSaveFileByNumber(int number)
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
            
            var filePath = lines[number - 1].Split('|')[1];
            var file = new FileInfo(filePath);
            
            if (!file.Exists)
            {
                Console.WriteLine($"Error: Save file not found at {filePath}");
                return null;
            }
            
            return file;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Failed to read save file cache: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Formats file size for display
    /// </summary>
    protected string FormatFileSize(long byteCount)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = byteCount;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:n1} {suffixes[counter]}";
    }
} 