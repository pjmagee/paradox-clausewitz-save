using Microsoft.Extensions.Logging;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Services;

/// <summary>
/// Manages access to game-specific file providers
/// </summary>
public class GameServiceManager(ILogger<GameServiceManager> logger, IEnumerable<IGameFilesProvider> gameProviders)
{
    /// <summary>
    /// Gets all the registered game providers
    /// </summary>
    public IEnumerable<IGameFilesProvider> GetAllProviders() => gameProviders;

    /// <summary>
    /// Gets a specific game provider by name (case-insensitive)
    /// </summary>
    public IGameFilesProvider? GetProviderByName(string gameName)
    {
        if (string.IsNullOrEmpty(gameName)) return null;
        var provider = gameProviders.FirstOrDefault(p => p.GameName.Equals(gameName, StringComparison.OrdinalIgnoreCase));
            
        if (provider == null)
        {
            logger.LogWarning("No game provider found for game: {GameName}", gameName);
        }
        
        return provider;
    }
    
    /// <summary>
    /// Find all save files across all supported games
    /// </summary>
    public IEnumerable<SaveFileInfo> FindAllGameSaveFiles()
    {
        var allSaves = new List<SaveFileInfo>();
        int index = 1;
        
        foreach (var provider in gameProviders)
        {
            logger.LogDebug("Finding save files for {Game}", provider.GameName);
            
            try
            {
                var saveFiles = provider.FindSaveFiles();
                
                foreach (var file in saveFiles)
                {
                    allSaves.Add(new SaveFileInfo
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Error finding save files for {Game}", provider.GameName);
            }
        }
        
        return allSaves;
    }
} 