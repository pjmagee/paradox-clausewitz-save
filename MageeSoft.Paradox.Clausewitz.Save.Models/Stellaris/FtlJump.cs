namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents an FTL jump in the game state.
/// </summary>
[SaveModel]
public partial class FtlJump
{
    /// <summary>
    /// Gets or sets the origin coordinate.
    /// </summary>
    public required Coordinate From { get;set; }

    /// <summary>
    /// Gets or sets the destination system ID.
    /// </summary>
    public long? To { get;set; }

    /// <summary>
    /// Gets or sets the fleet ID.
    /// </summary>
    public required long Fleet { get;set; }

    /// <summary>
    /// Gets or sets the jump method.
    /// </summary>
    public required string JumpMethod { get;set; }

    /// <summary>
    /// Gets or sets the bypass from ID.
    /// </summary>
    public required long BypassFrom { get;set; }

    /// <summary>
    /// Gets or sets the bypass to ID.
    /// </summary>
    public required long BypassTo { get;set; }
}






