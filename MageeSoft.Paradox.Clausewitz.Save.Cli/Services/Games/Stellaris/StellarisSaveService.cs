using MageeSoft.Paradox.Clausewitz.Save.Cli.Games;
using MageeSoft.Paradox.Clausewitz.Save.Parser;
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

    public StellarisSaveSummary GetSaveSummary(FileInfo saveFile)
    {
        try
        {
            using var stream = File.OpenRead(saveFile.FullName);
            using var zip = new GameSaveZip(stream);
            var documents = zip.GetDocuments();
            
            // Extract meta information
            var meta = documents.MetaDocument.Root as SaveObject;
            var version = GetPropertyValue(meta, "version");
            var date = GetPropertyValue(meta, "date");
            var name = GetPropertyValue(meta, "name");
            var ironman = GetPropertyValue(meta, "ironman");
            var metaFleets = GetPropertyValue(meta, "meta_fleets");
            var metaPlanets = GetPropertyValue(meta, "meta_planets");
            
            // Extract gamestate information
            var gamestate = documents.GameStateDocument.Root as SaveObject;
            var topLevelKeys = gamestate?.Properties.Select(p => p.Key).ToList() ?? new List<string>();
            
            return new StellarisSaveSummary
            {
                FileName = saveFile.Name,
                FilePath = saveFile.FullName,
                FileSize = saveFile.Length,
                LastModified = saveFile.LastWriteTime,
                GameVersion = version,
                GameDate = date,
                EmpireName = name,
                IsIronman = ironman == "True",
                FleetCount = int.TryParse(metaFleets, out var fleets) ? fleets : 0,
                PlanetCount = int.TryParse(metaPlanets, out var planets) ? planets : 0,
                TopLevelSections = topLevelKeys
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

    private string GetPropertyValue(SaveObject? saveObject, string propertyName)
    {
        try
        {
            if (saveObject == null)
                return string.Empty;

            var property = saveObject.Properties.FirstOrDefault(p => p.Key == propertyName);
            if (property.Key != null)
            {
                // BAD: This is a hack to remove the "Scalar: " prefix from the string
                // Should use correct library method to get the value
                return property.Value.ToString().Replace("Scalar: ", "");
            }
        }
        catch
        {
            // Ignore errors and return empty string
        }
        
        return string.Empty;
    }
}