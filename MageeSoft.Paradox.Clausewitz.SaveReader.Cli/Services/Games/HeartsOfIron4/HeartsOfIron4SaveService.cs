using MageeSoft.Paradox.Clausewitz.SaveReader.Cli.Games;
using Microsoft.Extensions.Logging;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Cli.Services.Games.HeartsOfIron4;

/// <summary>
/// Service for handling Hearts of Iron IV save files.
/// </summary>
public class HeartsOfIron4SaveService : BaseGameSaveService
{
    public HeartsOfIron4SaveService(ILogger<HeartsOfIron4SaveService> logger) : base(logger)
    {
    }

    public override GameType GameType => GameType.HeartsOfIron4;

    public override string DefaultSaveDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Documents",
        "Paradox Interactive",
        "Hearts of Iron IV",
        "save games"
    );

    public override string SaveFileExtension => ".hoi4";

    protected override string SteamAppId => "394360";

    protected override string? MicrosoftStorePackageName => "ParadoxInteractive.HeartsofIronIV_zfnrdv2de78ny";

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