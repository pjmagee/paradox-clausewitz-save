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
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the army type.
    /// </summary>
    public string Type { get;set; }

    /// <summary>
    /// Gets or sets the army name.
    /// </summary>
    public LocalizedText Name { get;set; }

    /// <summary>
    /// Gets or sets the current health.
    /// </summary>
    public int Health { get;set; }

    /// <summary>
    /// Gets or sets the maximum health.
    /// </summary>
    public int MaxHealth { get;set; }

    /// <summary>
    /// Gets or sets the jump drive cooldown.
    /// </summary>
    public string JumpDriveCooldown { get;set; }

    /// <summary>
    /// Gets or sets the planet ID.
    /// </summary>
    public long Planet { get;set; }

    /// <summary>
    /// Gets or sets the country ID.
    /// </summary>
    public long Country { get;set; }

    /// <summary>
    /// Gets or sets the ship ID.
    /// </summary>
    public long Ship { get;set; }

    /// <summary>
    /// Gets or sets the leader ID.
    /// </summary>
    public long Leader { get;set; }

    /// <summary>
    /// Gets or sets the morale value.
    /// </summary>
    public int Morale { get;set; }
} 






