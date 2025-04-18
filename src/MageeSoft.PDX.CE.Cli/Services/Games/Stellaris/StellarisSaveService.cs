using System.CommandLine;
using System.CommandLine.IO;
using System.Text.Json;
using MageeSoft.PDX.CE.Save;

namespace MageeSoft.PDX.CE.Cli.Services.Games.Stellaris;

public class StellarisSaveService(IConsole console, GamePathResolver pathResolver) : IGameFilesProvider
{
    public GameType GameType => GameType.Stellaris;
    
    public string GameName => "Stellaris";
    
    public string SaveFileExtension => ".sav";

    public IEnumerable<FileInfo> FindSaveFiles()
    {
        var saveDirectories = pathResolver.GetPotentialSavePaths(GameName);

        var files = new List<FileInfo>();
        foreach (var dir in saveDirectories)
        {
            if (!Directory.Exists(dir))
                continue;
            
            try
            {
                var directory = new DirectoryInfo(dir);
                
                // save files are usually in subfolders of a game session / empire / country name
                var entries = directory
                    .GetFiles("*" + SaveFileExtension, SearchOption.AllDirectories)
                    .Where(file => file.Length > 0);

                files.AddRange(entries);
            }
            catch (Exception ex)
            {
                console.Error.WriteLine($"Error finding save files in {dir}: {ex.Message}");
            }
        }
        return files.OrderByDescending(file => file.LastWriteTime);
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
                Ironman = save.Meta.FindProperty("ironman").Value.Value<bool>(),
                Version = save.Meta.FindProperty("version").Value.Value<string>(),
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