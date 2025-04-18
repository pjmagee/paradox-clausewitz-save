using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace MageeSoft.PDX.CE.Cli.Services;

public class GamePathResolver
{
    public IEnumerable<string> GetPotentialSavePaths(string gameName)
    {
        var paths = new List<string>();
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            paths.AddRange(GetWindowsSavePaths(gameName, homeDir));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            paths.AddRange(GetLinuxSavePaths(gameName, homeDir));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            paths.AddRange(GetMacOSSavePaths(gameName, homeDir));
        }
        
        paths.AddRange(GetSteamSavePaths(gameName));
        return paths.Where(Directory.Exists).Distinct().ToList();
    }

    private static IEnumerable<string> GetWindowsSavePaths(string gameName, string homeDir)
    {
        gameName = NormalizeGameName(gameName);
        
        // Standard Paradox save locations
        var paths = new List<string>
        {
            // Regular Documents folder
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Paradox Interactive", gameName, "save games"),
            
            // OneDrive-synced Documents folder
            Path.Combine(homeDir, "OneDrive", "Documents", "Paradox Interactive", gameName, "save games"),
            
            // Alternate OneDrive location
            Path.Combine(homeDir, "OneDrive\\Documents", "Paradox Interactive", gameName, "save games"),
            
            // Legacy Documents path
            Path.Combine(homeDir, "Documents", "Paradox Interactive", gameName, "save games"),
            
            // Some Windows 10/11 users have this path
            Path.Combine(homeDir, "OneDrive - Personal", "Documents", "Paradox Interactive", gameName, "save games")
        };
        
        // Check Windows Store installation path
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var windowsStorePath = Path.Combine(localAppData, "Packages", GetMicrosoftStorePackageName(gameName), "LocalCache", "Local", "Paradox Interactive", gameName, "save games");
        paths.Add(windowsStorePath);

        return paths;
    }

    [SupportedOSPlatform("linux")]
    private static IEnumerable<string> GetLinuxSavePaths(string gameName, string homeDir)
    {
        var normalizedGameName = NormalizeGameName(gameName);
        
        return new[]
        {
            Path.Combine(homeDir, ".local", "share", "Paradox Interactive", normalizedGameName, "save games"),
            Path.Combine(homeDir, ".paradoxlauncher", normalizedGameName, "save games"),
            Path.Combine(homeDir, ".steam", "steam", "steamapps", "compatdata", GetSteamGameId(gameName), "pfx", "drive_c", "users", "steamuser", "Documents", "Paradox Interactive", normalizedGameName, "save games")
        };
    }

    [SupportedOSPlatform("macos")]
    private static IEnumerable<string> GetMacOSSavePaths(string gameName, string homeDir)
    {
        var normalizedGameName = NormalizeGameName(gameName);
        
        return new[]
        {
            Path.Combine(homeDir, "Documents", "Paradox Interactive", normalizedGameName, "save games"),
            Path.Combine(homeDir, "Library", "Application Support", "Paradox Interactive", normalizedGameName, "save games")
        };
    }

    private static IEnumerable<string> GetSteamSavePaths(string gameName)
    {
        var steamPaths = new List<string>();
        var gameId = GetSteamGameId(gameName);
        var normalizedGameName = NormalizeGameName(gameName);

        if (string.IsNullOrEmpty(gameId)) return steamPaths;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            steamPaths.AddRange(GetWindowsSteamInstallPaths());
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            steamPaths.AddRange(new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "Steam"),
                "/usr/share/steam"
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            steamPaths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "Steam"));
        }
        
        var result = new List<string>();
        
        // Steam game installation path
        foreach (var steamPath in steamPaths.Where(Directory.Exists))
        {
            result.Add(Path.Combine(steamPath, "steamapps", "common", gameId, "save games"));
            result.Add(Path.Combine(steamPath, "steamapps", "workshop", "content", gameId, "save games"));
            result.Add(Path.Combine(steamPath, "userdata"));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                result.Add(Path.Combine(steamPath, "steamapps", "compatdata", gameId, "pfx", "drive_c", "users", "steamuser", "Documents", "Paradox Interactive", normalizedGameName, "save games"));
            }
        }

        return result;
    }

    [SupportedOSPlatform("windows")]
    private static IEnumerable<string> GetWindowsSteamInstallPaths()
    {
        var paths = new List<string>();

        try
        {
            // Try to get Steam installation path from registry
            string? steamPath = null;
            
            // Try 64-bit registry path first
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam"))
            {
                steamPath = key?.GetValue("InstallPath") as string;
            }
            
            // If not found, try 32-bit path
            if (string.IsNullOrEmpty(steamPath))
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                {
                    steamPath = key?.GetValue("InstallPath") as string;
                }
            }
            
            // If not found in either location, try current user
            if (string.IsNullOrEmpty(steamPath))
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                {
                    steamPath = key?.GetValue("SteamPath") as string;
                }
            }

            if (!string.IsNullOrEmpty(steamPath))
            {
                // Normalize path (in case it has forward slashes)
                steamPath = Path.GetFullPath(steamPath);
                paths.Add(steamPath);

                // Read libraryfolders.vdf to get additional Steam library locations
                var libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                if (File.Exists(libraryFoldersPath))
                {
                    try
                    {
                        // Simple parsing of libraryfolders.vdf
                        foreach (var line in File.ReadAllLines(libraryFoldersPath))
                        {
                            if (line.Contains("\"path\""))
                            {
                                var pathMatch = line.Split('"')[3].Replace("\\\\", "\\"); // Extract path between quotes
                                if (Directory.Exists(pathMatch))
                                {
                                    paths.Add(pathMatch);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ignore errors in parsing the file
                    }
                }
            }
        }
        catch
        {
            // Ignore registry access errors
        }

        // Add fallback paths if we couldn't find Steam in the registry
        paths.AddRange(new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam")
        });

        return paths;
    }

    private static string GetSteamGameId(string gameName) => gameName.ToLowerInvariant() switch
    {
        "stellaris" => "281990",
        "europa universalis iv" => "236850",
        "hearts of iron iv" => "394360",
        "crusader kings iii" => "1158310",
        "victoria 3" => "529340",
        _ => string.Empty
    };

    private static string GetMicrosoftStorePackageName(string gameName) => gameName.ToLowerInvariant() switch
    {
        "stellaris" => "ParadoxInteractive.ProjectTitus_zfnrdv2de78ny",
        "europa universalis iv" => "ParadoxInteractive.EuropaUniversalisIV_pdcmj3cqbc44m",
        "hearts of iron iv" => "ParadoxInteractive.HeartsofIronIV_pdcmj3cqbc44m",
        "crusader kings iii" => "ParadoxInteractive.CrusaderKingsIII_pdcmj3cqbc44m",
        "victoria 3" => "ParadoxInteractive.Victoria3_pdcmj3cqbc44m",
        _ => string.Empty
    };
    
    private static string NormalizeGameName(string gameName) => gameName.ToLowerInvariant() switch
    {
        "stellaris" => "Stellaris",
        "europa universalis iv" => "Europa Universalis IV",
        "hearts of iron iv" => "Hearts of Iron IV",
        "crusader kings iii" => "Crusader Kings III",
        "victoria 3" => "Victoria 3",
        _ => gameName // Return as-is if not matched
    };
} 