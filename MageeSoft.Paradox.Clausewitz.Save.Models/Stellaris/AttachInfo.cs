namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents attach information for an ambient object.
/// </summary>
[SaveModel]
public partial class AttachInfo
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required int Type { get;set; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get;set; }
}