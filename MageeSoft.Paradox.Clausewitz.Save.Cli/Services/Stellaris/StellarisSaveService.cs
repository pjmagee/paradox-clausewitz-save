using MageeSoft.Paradox.Clausewitz.Save.Cli.Games;
using MageeSoft.Paradox.Clausewitz.Save.Reader.Games.Stellaris;
using Microsoft.Extensions.Logging;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Services.Games.Stellaris;

/// <summary>
/// Service for handling Stellaris save files.
/// </summary>
public class StellarisSaveService(ILogger<StellarisSaveService> logger) : BaseGameSaveService(logger)
{
    public override GameType GameType => GameType.Stellaris;

    public override string DefaultSaveDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Documents",
        "Paradox Interactive",
        "Stellaris",
        "save games"
    );

    public override string SaveFileExtension => ".sav";

    protected override string SteamAppId => "281990";

    protected override string? MicrosoftStorePackageName => "ParadoxInteractive.ProjectTitus_zfnrdv2de78ny";

    public override string GetSummary(FileInfo saveFile)
    {
        return System.Text.Json.JsonSerializer.Serialize(GetSaveSummary(saveFile), StellarisSaveSummaryContext.Default.StellarisSaveSummary);
    }

    public StellarisSaveSummary GetSaveSummary(FileInfo saveFile)
    {
        try
        {
            var stellarisSave = StellarisSave.FromSave(saveFile);
            
            return new StellarisSaveSummary
            {
                FileName = saveFile.Name,
                FilePath = saveFile.FullName,
                FileSize = saveFile.Length,
                LastModified = saveFile.LastWriteTime,
                Meta = stellarisSave.Meta,
                Error = string.Empty
            };
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Error parsing save file: {FilePath}", saveFile.FullName);
            return new StellarisSaveSummary
            {
                FileName = saveFile.Name,
                FilePath = saveFile.FullName,
                FileSize = saveFile.Length,
                LastModified = saveFile.LastWriteTime,
                Error = ex.Message
            };
        }
    }
}