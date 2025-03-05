using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using StellarisSaveParser;

namespace StellarisSaveParser.Cli.Services;

public class StellarisSaveService
{
    private readonly ILogger<StellarisSaveService> _logger;
    
    // Stellaris Steam App ID
    private const string SteamAppId = "281990";

    public StellarisSaveService(ILogger<StellarisSaveService> logger)
    {
        _logger = logger;
    }

    public IEnumerable<FileInfo> FindSaveFiles()
    {
        var paths = GetSavePaths();
        var saveFiles = new List<FileInfo>();

        foreach (var path in paths)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogDebug("Save directory not found: {Path}", path);
                continue;
            }

            _logger.LogDebug("Searching for save files in: {Path}", path);
            var files = Directory.GetFiles(path, "*.sav", SearchOption.AllDirectories)
                .Select(f => new FileInfo(f));
            
            saveFiles.AddRange(files);
        }

        return saveFiles;
    }

    public SaveSummary GetSaveSummary(FileInfo saveFile)
    {
        try
        {
            var documents = GameSaveZip.Unzip(saveFile);
            
            // Extract meta information
            var meta = documents.Meta.RootElement;
            var version = GetPropertyValue(meta, "version");
            var date = GetPropertyValue(meta, "date");
            var name = GetPropertyValue(meta, "name");
            var ironman = GetPropertyValue(meta, "ironman");
            var metaFleets = GetPropertyValue(meta, "meta_fleets");
            var metaPlanets = GetPropertyValue(meta, "meta_planets");
            
            // Extract gamestate information
            var gamestate = documents.GameState.RootElement;
            var topLevelKeys = gamestate.EnumerateObject().Select(p => p.Key).ToList();
            
            return new SaveSummary
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
            _logger.LogDebug(ex, "Error parsing save file: {FilePath}", saveFile.FullName);
            return new SaveSummary
            {
                FileName = saveFile.Name,
                FilePath = saveFile.FullName,
                FileSize = saveFile.Length,
                LastModified = saveFile.LastWriteTime,
                Error = ex.Message
            };
        }
    }

    private string GetPropertyValue(Element element, string propertyName)
    {
        try
        {
            var property = element.EnumerateObject().FirstOrDefault(p => p.Key == propertyName);
            if (property.Key != null)
            {
                return property.Value.ToString().Replace("Scalar: ", "");
            }
        }
        catch
        {
            // Ignore errors and return empty string
        }
        
        return string.Empty;
    }

    private IEnumerable<string> GetSavePaths()
    {
        var paths = new List<string>();
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Standard Documents location
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            paths.Add(Path.Combine(documentsPath, "Paradox Interactive", "Stellaris", "save games"));
            
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
                        var stellarisSavePath = Path.Combine(userFolder, SteamAppId, "remote", "save games");
                        paths.Add(stellarisSavePath);
                    }
                }
            }
            
            // Microsoft Store / Game Pass
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            paths.Add(Path.Combine(localAppData, "Packages", "ParadoxInteractive.ProjectTitus_zfnrdv2de78ny", "SystemAppData", "wgs"));
            
            // GOG
            paths.Add(Path.Combine(documentsPath, "Paradox Interactive", "Stellaris", "save games"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Steam on Linux
            var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            paths.Add(Path.Combine(homePath, ".local", "share", "Paradox Interactive", "Stellaris", "save games"));
            
            // Steam Cloud on Linux
            paths.Add(Path.Combine(homePath, ".steam", "steam", "userdata"));
            
            // Steam Proton / Wine
            paths.Add(Path.Combine(homePath, ".steam", "steam", "steamapps", "compatdata", SteamAppId, "pfx", "drive_c", "users", "steamuser", "Documents", "Paradox Interactive", "Stellaris", "save games"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS
            var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            paths.Add(Path.Combine(homePath, "Documents", "Paradox Interactive", "Stellaris", "save games"));
            
            // Steam Cloud on macOS
            var libraryPath = Path.Combine(homePath, "Library");
            paths.Add(Path.Combine(libraryPath, "Application Support", "Steam", "userdata"));
        }
        
        return paths;
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
            _logger.LogDebug(ex, "Error getting Steam path from registry");
        }
        
        return string.Empty;
    }
}

public class SaveSummary
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
    public string GameVersion { get; set; } = string.Empty;
    public string GameDate { get; set; } = string.Empty;
    public string EmpireName { get; set; } = string.Empty;
    public bool IsIronman { get; set; }
    public int FleetCount { get; set; }
    public int PlanetCount { get; set; }
    public List<string> TopLevelSections { get; set; } = new();
    public string Error { get; set; } = string.Empty;
    
    public bool HasError => !string.IsNullOrEmpty(Error);
    
    public string GetFormattedSize()
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = FileSize;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }
} 