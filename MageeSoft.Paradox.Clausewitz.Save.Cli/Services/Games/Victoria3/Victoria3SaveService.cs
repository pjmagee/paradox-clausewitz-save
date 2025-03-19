using MageeSoft.Paradox.Clausewitz.Save.Cli.Games;
using Microsoft.Extensions.Logging;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Services.Games.Victoria3;

/// <summary>
/// Service for handling Victoria 3 save files.
/// </summary>
public class Victoria3SaveService : BaseGameSaveService
{
    public Victoria3SaveService(ILogger<Victoria3SaveService> logger) : base(logger)
    {
    }

    public override GameType GameType => GameType.Victoria3;

    public override string DefaultSaveDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Documents",
        "Paradox Interactive",
        "Victoria 3",
        "save games"
    );

    public override string SaveFileExtension => ".v3";

    protected override string SteamAppId => "529340";

    protected override string? MicrosoftStorePackageName => "ParadoxInteractive.Victoria3_zfnrdv2de78ny";

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

    public override string GetSummary(FileInfo saveFile) =>  throw new NotImplementedException();
} 