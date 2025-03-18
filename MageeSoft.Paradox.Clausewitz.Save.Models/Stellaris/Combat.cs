using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents combat in the game state.
/// </summary>
public record Combat
{
    [SaveObject("coordinate")]
    public required Position Coordinate { get; set; }

    [SaveObject("formation_pos")]
    public required FormationPosition FormationPos { get; set; }

    [SaveObject("formation")]
    public required Formation Formation { get; set; }

    [SaveObject("start_coordinate")]
    public required Position StartCoordinate { get; set; }

    [SaveScalar("start_date")]
    public required string StartDate { get; init; }
} 