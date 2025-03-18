using System.Runtime.InteropServices;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Games;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Services;

/// <summary>
/// Base class for game-specific save file services.
/// </summary>
public abstract class BaseGameSaveService(ILogger logger) : IGameSaveService
{
    protected readonly ILogger Logger = logger;

    public abstract GameType GameType { get; }
    public abstract string SaveFileExtension { get; }
    public abstract string DefaultSaveDirectory { get; }

    /// <summary>
    /// Gets the Steam App ID for the game.
    /// </summary>
    protected abstract string SteamAppId { get; }

    /// <summary>
    /// Gets the Microsoft Store package name for the game, if applicable.
    /// </summary>
    protected abstract string? MicrosoftStorePackageName { get; }

    public virtual IEnumerable<FileInfo> FindSaveFiles()
    {
        var paths = GetSavePaths().Distinct();
        var saveFiles = new List<FileInfo>();

        foreach (var path in paths)
        {
            if (!Directory.Exists(path))
            {
                Logger.LogDebug("Save directory not found: {Path}", path);
                continue;
            }

            Logger.LogDebug("Searching for save files in: {Path}", path);
            var files = Directory.GetFiles(path, "*" + SaveFileExtension, SearchOption.AllDirectories)
                .Select(f => new FileInfo(f))
                .Where(IsValidSaveFile);

            saveFiles.AddRange(files);
        }

        return saveFiles;
    }

    public virtual bool IsValidSaveFile(FileInfo file)
    {
        return file.Extension.Equals(SaveFileExtension, StringComparison.OrdinalIgnoreCase) &&
               file.Length > 0;
    }

    protected virtual IEnumerable<string> GetSavePaths()
    {
        var paths = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Standard Documents location
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            paths.Add(DefaultSaveDirectory);

            // Steam Cloud Saves
            var steamPath = GetSteamPath();
            if (!string.IsNullOrEmpty(steamPath))
            {
                var userDataPath = Path.Combine(steamPath, "userdata");
                if (Directory.Exists(userDataPath))
                {
                    // Each user has their own folder (SteamID3)
                    foreach (var userFolder in Directory.GetDirectories(userDataPath))
                    {
                        var gameSavePath = Path.Combine(userFolder, SteamAppId, "remote", "save games");
                        paths.Add(gameSavePath);
                    }
                }
            }

            // Microsoft Store / Game Pass
            if (!string.IsNullOrEmpty(MicrosoftStorePackageName))
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                paths.Add(Path.Combine(localAppData, "Packages", MicrosoftStorePackageName, "SystemAppData", "wgs"));
            }

            // GOG
            paths.Add(DefaultSaveDirectory);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // Native Linux installation
            paths.Add(Path.Combine(homePath, ".local", "share", "Paradox Interactive", GetGameFolderName(), "save games"));

            // Steam Cloud on Linux
            paths.Add(Path.Combine(homePath, ".steam", "steam", "userdata"));

            // Steam Proton / Wine
            paths.Add(Path.Combine(homePath, ".steam", "steam", "steamapps", "compatdata", SteamAppId, "pfx", "drive_c", "users", "steamuser", "Documents", "Paradox Interactive", GetGameFolderName(), "save games"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // macOS
            paths.Add(Path.Combine(homePath, "Documents", "Paradox Interactive", GetGameFolderName(), "save games"));

            // Steam Cloud on macOS
            var libraryPath = Path.Combine(homePath, "Library");
            paths.Add(Path.Combine(libraryPath, "Application Support", "Steam", "userdata"));
        }

        return paths;
    }

    protected virtual string GetGameFolderName()
    {
        return GameType switch
        {
            GameType.Stellaris => "Stellaris",
            GameType.CrusaderKings3 => "Crusader Kings III",
            GameType.HeartsOfIron4 => "Hearts of Iron IV",
            GameType.Victoria3 => "Victoria 3",
            _ => throw new ArgumentException($"Unknown game type: {GameType}")
        };
    }

    private string GetSteamPath()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Check 64-bit registry
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
                if (key != null)
                {
                    var installPath = key.GetValue("InstallPath") as string;
                    if (!string.IsNullOrEmpty(installPath))
                    {
                        return installPath;
                    }
                }

                // Check 32-bit registry
                using var key32 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam");
                if (key32 != null)
                {
                    var installPath = key32.GetValue("InstallPath") as string;
                    if (!string.IsNullOrEmpty(installPath))
                    {
                        return installPath;
                    }
                }

                // Check user registry
                using var userKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam");
                if (userKey != null)
                {
                    var installPath = userKey.GetValue("SteamPath") as string;
                    if (!string.IsNullOrEmpty(installPath))
                    {
                        // Convert forward slashes to backslashes if needed
                        return installPath.Replace("/", "\\");
                    }
                }
            }

            // Default Steam paths for other platforms
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(homePath, ".steam", "steam");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(homePath, "Library", "Application Support", "Steam");
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Error getting Steam path from registry");
        }

        return string.Empty;
    }
} 