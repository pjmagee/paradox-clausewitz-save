using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents combat statistics in the game state.
/// </summary>
public record CombatStats
{
    /// <summary>
    /// Gets or sets the damage dealt.
    /// </summary>
    public required float DamageDealt { get; init; }

    /// <summary>
    /// Gets or sets the damage taken.
    /// </summary>
    public required float DamageTaken { get; init; }

    /// <summary>
    /// Gets or sets the ships lost.
    /// </summary>
    public required int ShipsLost { get; init; }

    /// <summary>
    /// Gets or sets the armies lost.
    /// </summary>
    public required int ArmiesLost { get; init; }
}






