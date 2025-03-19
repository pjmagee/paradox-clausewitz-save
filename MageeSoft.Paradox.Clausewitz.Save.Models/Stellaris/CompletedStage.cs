

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a completed stage in first contact.
/// </summary>
[SaveModel]
public partial class CompletedStage
{
    /// <summary>
    /// Gets or sets the date when the stage was completed.
    /// </summary>
    public required string Date { get;set; }

    /// <summary>
    /// Gets or sets the stage name.
    /// </summary>
    public required string Stage { get;set; }


}






