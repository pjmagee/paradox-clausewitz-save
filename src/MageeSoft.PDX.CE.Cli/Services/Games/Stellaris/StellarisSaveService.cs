using System.CommandLine;
using System.CommandLine.IO;
using System.Text.Json;
using MageeSoft.PDX.CE.Cli.Games;
using MageeSoft.PDX.CE.Save;

namespace MageeSoft.PDX.CE.Cli.Services.Games.Stellaris;

public class StellarisSaveService(IConsole console, GamePathResolver pathResolver) : IGameFilesProvider
{
    public GameType GameType => GameType.Stellaris;
    
    public string GameName => "Stellaris";
    
    public string SaveFileExtension => ".sav";

    public IEnumerable<FileInfo> FindSaveFiles()
    {
        var savePaths = pathResolver.GetPotentialSavePaths(GameName);
        var saveFiles = new List<FileInfo>();

        foreach (var savePath in savePaths)
        {
            console.WriteLine($"Searching for save files in {savePath}");
            
            try
            {
                if (!Directory.Exists(savePath))
                {
                    console.WriteLine($"Directory does not exist: {savePath}");
                    continue;
                }
                
                var files = Directory.GetFiles(savePath, $"*{SaveFileExtension}", SearchOption.AllDirectories)
                    .Select(f => new FileInfo(f))
                    .Where(IsValidSaveFile)
                    .ToArray();
                
                saveFiles.AddRange(files);
                
                console.WriteLine($"Found {files.Length} save files in {savePath}");
            }
            catch (Exception ex)
            {
                console.Error.WriteLine($"Error accessing save directory: {savePath}. Exception: {ex.Message}");
            }
        }

        return saveFiles;
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
                Ironman = save.Meta.Ironman!.Value,
                Version = save.Meta.Version!,
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