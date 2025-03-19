namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents an army in the game state.
/// </summary>
[SaveModel]
public partial class Army
{
    /// <summary>
    /// Gets or sets the army ID.
    /// </summary>
    public required long Id { get; set; }

    /// <summary>
    /// Gets or sets the army type.
    /// </summary>
    public required string Type { get;set; }

    /// <summary>
    /// Gets or sets the army name.
    /// </summary>
    public required LocalizedText Name { get;set; }

    /// <summary>
    /// Gets or sets the current health.
    /// </summary>
    public required int Health { get;set; }

    /// <summary>
    /// Gets or sets the maximum health.
    /// </summary>
    public required int MaxHealth { get;set; }

    /// <summary>
    /// Gets or sets the jump drive cooldown.
    /// </summary>
    public required string JumpDriveCooldown { get;set; }

    /// <summary>
    /// Gets or sets the planet ID.
    /// </summary>
    public required long Planet { get;set; }

    /// <summary>
    /// Gets or sets the country ID.
    /// </summary>
    public required long Country { get;set; }

    /// <summary>
    /// Gets or sets the ship ID.
    /// </summary>
    public required long Ship { get;set; }

    /// <summary>
    /// Gets or sets the leader ID.
    /// </summary>
    public required long Leader { get;set; }

    /// <summary>
    /// Gets or sets the morale value.
    /// </summary>
    public required int Morale { get;set; }
} 






