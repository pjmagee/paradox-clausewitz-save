using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents an astral rift in the game state.
/// </summary>
public class AstralRift
{
    /// <summary>
    /// Gets or sets the astral rift ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the type of the astral rift.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the astral rift is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the name of the astral rift.
    /// </summary>
    public required LocalizedText Name { get; init; }

    /// <summary>
    /// Gets or sets the coordinate of the astral rift.
    /// </summary>
    public required Coordinate Coordinate { get; init; }

    /// <summary>
    /// Gets or sets the owner ID.
    /// </summary>
    public required int Owner { get; init; }

    /// <summary>
    /// Gets or sets the explorer fleet ID.
    /// </summary>
    public required long ExplorerFleet { get; init; }

    /// <summary>
    /// Gets or sets the leader ID.
    /// </summary>
    public required long Leader { get; init; }

    /// <summary>
    /// Gets or sets the explorer ID.
    /// </summary>
    public required long Explorer { get; init; }

    /// <summary>
    /// Gets or sets the number of clues.
    /// </summary>
    public required int Clues { get; init; }

    /// <summary>
    /// Gets or sets the last roll value.
    /// </summary>
    public required int LastRoll { get; init; }

    /// <summary>
    /// Gets or sets the days left.
    /// </summary>
    public required int DaysLeft { get; init; }

    /// <summary>
    /// Gets or sets the difficulty.
    /// </summary>
    public required int Difficulty { get; init; }

    /// <summary>
    /// Gets or sets the event information.
    /// </summary>
    public required AstralRiftEvent Event { get; init; }

    /// <summary>
    /// Gets or sets the event choice.
    /// </summary>
    public required string EventChoice { get; init; }

    /// <summary>
    /// Gets or sets the on roll failed value.
    /// </summary>
    public required string OnRollFailed { get; init; }

    /// <summary>
    /// Gets or sets the fail probability.
    /// </summary>
    public required int FailProbability { get; init; }

    /// <summary>
    /// Gets or sets the cumulated fail probability.
    /// </summary>
    public required int CumulatedFailProbability { get; init; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Gets or sets the flags.
    /// </summary>
    public required ImmutableDictionary<string, long> Flags { get; init; }

    /// <summary>
    /// Gets or sets the interactable by IDs.
    /// </summary>
    public required ImmutableArray<int> InteractableBy { get; init; }

    /// <summary>
    /// Gets or sets the astral rift orbitals.
    /// </summary>
    public required ImmutableArray<object> AstralRiftOrbitals { get; init; }

    /// <summary>
    /// Gets or sets the ship class orbital station ID.
    /// </summary>
    public required long ShipClassOrbitalStation { get; init; }
}