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
    public long Id { get;set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string Type { get;set; }

    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    public float Progress { get;set; }
}






