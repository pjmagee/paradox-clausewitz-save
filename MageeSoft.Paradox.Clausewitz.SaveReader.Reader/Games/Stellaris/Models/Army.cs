using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an army in the game state.
/// </summary>
public class Army
{
    /// <summary>
    /// Gets or sets the army ID.
    /// </summary>
    public required long Id { get; set; }

    /// <summary>
    /// Gets or sets the army type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the army name.
    /// </summary>
    public required LocalizedText Name { get; init; }

    /// <summary>
    /// Gets or sets the current health.
    /// </summary>
    public required int Health { get; init; }

    /// <summary>
    /// Gets or sets the maximum health.
    /// </summary>
    public required int MaxHealth { get; init; }

    /// <summary>
    /// Gets or sets the jump drive cooldown.
    /// </summary>
    public required string JumpDriveCooldown { get; init; }

    /// <summary>
    /// Gets or sets the planet ID.
    /// </summary>
    public required long Planet { get; init; }

    /// <summary>
    /// Gets or sets the country ID.
    /// </summary>
    public required long Country { get; init; }

    /// <summary>
    /// Gets or sets the ship ID.
    /// </summary>
    public required long Ship { get; init; }

    /// <summary>
    /// Gets or sets the leader ID.
    /// </summary>
    public required long Leader { get; init; }

    /// <summary>
    /// Gets or sets the morale value.
    /// </summary>
    public required int Morale { get; init; }
} 






