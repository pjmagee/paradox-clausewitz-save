

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a completed stage in first contact.
/// </summary>
public record CompletedStage
{
    /// <summary>
    /// Gets or sets the date when the stage was completed.
    /// </summary>
    public required string Date { get; init; }

    /// <summary>
    /// Gets or sets the stage name.
    /// </summary>
    public required string Stage { get; init; }


}






