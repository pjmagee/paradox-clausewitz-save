using MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Services.Games.Stellaris;

public class StellarisSaveSummary
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
    public string Error { get; set; } = string.Empty;
    public bool HasError => !string.IsNullOrEmpty(Error);
    public Meta Meta { get; set; } = new Meta();
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