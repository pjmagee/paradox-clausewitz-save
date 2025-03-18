using MageeSoft.Paradox.Clausewitz.Save.Cli.Games;
using Microsoft.Extensions.Logging;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Services.Games.CrusaderKings3;

/// <summary>
/// Service for handling Crusader Kings III save files.
/// </summary>
public class CrusaderKings3SaveService : BaseGameSaveService
{
    public CrusaderKings3SaveService(ILogger<CrusaderKings3SaveService> logger) : base(logger)
    {
    }

    public override GameType GameType => GameType.CrusaderKings3;

    public override string DefaultSaveDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Documents",
        "Paradox Interactive",
        "Crusader Kings III",
        "save games"
    );

    public override string SaveFileExtension => ".ck3";

    protected override string SteamAppId => "1158310";

    protected override string? MicrosoftStorePackageName => "ParadoxInteractive.CrusaderKingsIII_zfnrdv2de78ny";

    public IEnumerable<FileInfo> FindSaveFiles()
    {
        if (!Directory.Exists(DefaultSaveDirectory))
        {
            yield break;
        }

        foreach (var file in Directory.GetFiles(DefaultSaveDirectory, "*" + SaveFileExtension, SearchOption.AllDirectories))
        {
            var fileInfo = new FileInfo(file);
            if (IsValidSaveFile(fileInfo))
            {
                yield return fileInfo;
            }
        }
    }

    public bool IsValidSaveFile(FileInfo file)
    {
        return file.Extension.Equals(SaveFileExtension, StringComparison.OrdinalIgnoreCase) &&
               file.Length > 0;
    }
} 