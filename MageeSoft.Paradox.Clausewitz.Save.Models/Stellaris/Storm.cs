namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a storm in the game state.
/// </summary>
[SaveModel]
public partial class Storm
{
    /// <summary>
    /// Gets or sets the storm ID.
    /// </summary>
    public long Id { get;set; }

    /// <summary>
    /// Gets or sets the type of the storm.
    /// </summary>
    public required string Type { get;set; }

    /// <summary>
    /// Gets or sets the position of the storm.
    /// </summary>
    public required Position Position { get;set; }
} 






