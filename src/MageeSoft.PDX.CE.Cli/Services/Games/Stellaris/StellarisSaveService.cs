using System.CommandLine;
using System.CommandLine.IO;
using System.Text.Json;
using MageeSoft.PDX.CE.Cli.Games;
using MageeSoft.PDX.CE.Save;
using MageeSoft.PDX.CE.Cli.JsonContext;
using MageeSoft.PDX.CE.Cli.Services.Model;
using MageeSoft.PDX.CE.Reader.Cli;

namespace MageeSoft.PDX.CE.Cli.Services.Games.Stellaris;

public class StellarisSaveService(IConsole console, GamePathResolver pathResolver) : IGameFilesProvider
{
    public GameType GameType => GameType.Stellaris;
    
    public string GameName => "Stellaris";
    
    public string SaveFileExtension => ".sav";

    public IEnumerable<FileInfo> FindSaveFiles()
    {
        var saveDirectory = pathResolver.GetSaveDirectory(GameType);
        if (saveDirectory is null || !Directory.Exists(saveDirectory))
        {
            console.Error.WriteLine($"Save directory for {GameName} not found.");
            return Array.Empty<FileInfo>();
        }

        try
        {
            var directory = new DirectoryInfo(saveDirectory);
            return directory.GetFiles("*" + SaveFileExtension, SearchOption.TopDirectoryOnly)
                .Where(file => file.Length > 0)
                .OrderByDescending(file => file.LastWriteTime);
        }
        catch (Exception ex)
        {
            console.Error.WriteLine($"Error finding save files: {ex.Message}");
            return Array.Empty<FileInfo>();
        }
    }

    public bool IsValidSaveFile(FileInfo file)
    {
        return file.Extension.Equals(SaveFileExtension, StringComparison.OrdinalIgnoreCase) &&
               file is { Exists: true, Length: > 0 };
    }

    public SaveSummary GetSaveSummary(FileInfo saveFile)
    {
        try
        {
            var save = StellarisSave.FromSave(saveFile);
            
            var summary = new SaveSummary
            {
                GameType = GameType,
                FileName = saveFile.Name,
                FileSize = saveFile.Length,
                LastModified = saveFile.LastWriteTime,
                Ironman = save.Meta.Ironman != null && save.Meta.Ironman == true,
                Version = save.Meta.Version ?? "Unknown",
                Error = null,
            };

            return summary;
        }
        catch (Exception ex)
        {
            console.Error.WriteLine($"Error parsing save file: {saveFile.FullName}");
            return new SaveSummary 
            { 
                GameType = GameType,
                HasError = true, 
                Error = ex.Message 
            };
        }
    }

    public string GetSummaryAsJson(FileInfo saveFile)
    {
        var summary = GetSaveSummary(saveFile);
        return JsonSerializer.Serialize(summary, CliJsonSerializerContext.Default.SaveSummary);
    }
} 