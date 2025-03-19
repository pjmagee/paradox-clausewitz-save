namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a subject conversion process in the game state.
/// </summary>
[SaveModel]
public partial class SubjectConversionProcess
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get;set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get;set; }

    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    public required float Progress { get;set; }
}






