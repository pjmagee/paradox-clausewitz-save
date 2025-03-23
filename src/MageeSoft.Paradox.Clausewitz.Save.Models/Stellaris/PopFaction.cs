namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a pop faction in the game state.
/// </summary>
[SaveModel]
public partial class PopFaction
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    [SaveScalar("id")]
    public int Id { get;set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [SaveScalar("name")]
    public string Name { get;set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string Type { get;set; }

    /// <summary>
    /// Gets or sets the support.
    /// </summary>
    public float Support { get;set; }

    /// <summary>
    /// Gets or sets the approval.
    /// </summary>
    public float Approval { get;set; }
}







