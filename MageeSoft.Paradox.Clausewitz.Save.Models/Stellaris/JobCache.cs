namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a job cache in the game state.
/// </summary>
[SaveModel]
public partial class JobCache
{
    /// <summary>
    /// Gets the default instance of JobCache.
    /// </summary>
    public static JobCache Default { get; } = new()
    {
        Job = string.Empty,
        Count = 0
    };

    /// <summary>
    /// Gets or sets the job.
    /// </summary>
    public required string Job { get;set; }

    /// <summary>
    /// Gets or sets the count.
    /// </summary>
    public required int Count { get;set; }
}