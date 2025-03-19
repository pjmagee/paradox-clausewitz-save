namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a conversion process in the game state.
/// </summary>
[SaveModel]
public partial class ConversionProcess
{
    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    [SaveScalar("progress")]
    public float Progress { get;set; }

    /// <summary>
    /// Gets or sets whether the process is in progress.
    /// </summary>
    [SaveScalar("in_progress")]
    public bool InProgress { get;set; }

    /// <summary>
    /// Gets or sets whether the process is done.
    /// </summary>
    [SaveScalar("done")]
    public bool Done { get;set; }

    /// <summary>
    /// Gets or sets whether to ignore the process.
    /// </summary>
    [SaveScalar("ignore")]
    public bool Ignore { get;set; }
}