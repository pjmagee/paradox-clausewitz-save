namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a job cache in the game state.
/// </summary>
[SaveModel]
public partial class JobCache
{

    /// <summary>
    /// Gets or sets the job.
    /// </summary>
    public string? Job { get;set; }

    /// <summary>
    /// Gets or sets the count.
    /// </summary>
    public int? Count { get;set; }
}