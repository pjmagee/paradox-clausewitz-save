namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents combat statistics in the game state.
/// </summary>
[SaveModel]
public partial class CombatStats
{
    /// <summary>
    /// Gets or sets the damage dealt.
    /// </summary>
    public float DamageDealt { get;set; }

    /// <summary>
    /// Gets or sets the damage taken.
    /// </summary>
    public float DamageTaken { get;set; }

    /// <summary>
    /// Gets or sets the ships lost.
    /// </summary>
    public int ShipsLost { get;set; }

    /// <summary>
    /// Gets or sets the armies lost.
    /// </summary>
    public int ArmiesLost { get;set; }
}






