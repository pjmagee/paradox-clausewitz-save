using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents an astral rift in the game state.
/// </summary>
[SaveModel]
public partial class AstralRift
{
    /// <summary>
    /// Gets or sets the astral rift ID.
    /// </summary>
    public long Id { get;set; }

    /// <summary>
    /// Gets or sets the type of the astral rift.
    /// </summary>
    public string Type { get;set; }

    /// <summary>
    /// Gets or sets a value indicating whether the astral rift is active.
    /// </summary>
    public bool IsActive { get;set; }

    /// <summary>
    /// Gets or sets the name of the astral rift.
    /// </summary>
    public LocalizedText Name { get;set; }

    /// <summary>
    /// Gets or sets the coordinate of the astral rift.
    /// </summary>
    public Coordinate Coordinate { get;set; }

    /// <summary>
    /// Gets or sets the owner ID.
    /// </summary>
    public int Owner { get;set; }

    /// <summary>
    /// Gets or sets the explorer fleet ID.
    /// </summary>
    public long ExplorerFleet { get;set; }

    /// <summary>
    /// Gets or sets the leader ID.
    /// </summary>
    public long Leader { get;set; }

    /// <summary>
    /// Gets or sets the explorer ID.
    /// </summary>
    public long Explorer { get;set; }

    /// <summary>
    /// Gets or sets the number of clues.
    /// </summary>
    public int Clues { get;set; }

    /// <summary>
    /// Gets or sets the last roll value.
    /// </summary>
    public int LastRoll { get;set; }

    /// <summary>
    /// Gets or sets the days left.
    /// </summary>
    public int DaysLeft { get;set; }

    /// <summary>
    /// Gets or sets the difficulty.
    /// </summary>
    public int Difficulty { get;set; }

    /// <summary>
    /// Gets or sets the event information.
    /// </summary>
    public AstralRiftEvent Event { get;set; }

    /// <summary>
    /// Gets or sets the event choice.
    /// </summary>
    public string EventChoice { get;set; }

    /// <summary>
    /// Gets or sets the on roll failed value.
    /// </summary>
    public string OnRollFailed { get;set; }

    /// <summary>
    /// Gets or sets the fail probability.
    /// </summary>
    public int FailProbability { get;set; }

    /// <summary>
    /// Gets or sets the cumulated fail probability.
    /// </summary>
    public int CumulatedFailProbability { get;set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get;set; }

    /// <summary>
    /// Gets or sets the flags.
    /// </summary>
    public ImmutableDictionary<string, long> Flags { get;set; }

    /// <summary>
    /// Gets or sets the interactable by IDs.
    /// </summary>
    public ImmutableArray<int> InteractableBy { get;set; }

    /// <summary>
    /// Gets or sets the astral rift orbitals.
    /// </summary>
    public ImmutableArray<object> AstralRiftOrbitals { get;set; }

    /// <summary>
    /// Gets or sets the ship class orbital station ID.
    /// </summary>
    public long ShipClassOrbitalStation { get;set; }
}