namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a bypass in the game state.
/// </summary>
[SaveModel]
public partial class Bypass
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string? Type { get;set; }

    /// <summary>
    /// Gets or sets whether the bypass is active.
    /// </summary>
    public bool? IsActive { get;set; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public long? Owner { get;set; }

    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    public int? Index { get;set; }

    /// <summary>
    /// Gets or sets whether the bypass is locked.
    /// </summary>
    public bool? IsLocked { get;set; }

    /// <summary>
    /// Gets or sets the days left.
    /// </summary>
    public int? DaysLeft { get;set; }

    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    public Position? Position { get;set; }
} 