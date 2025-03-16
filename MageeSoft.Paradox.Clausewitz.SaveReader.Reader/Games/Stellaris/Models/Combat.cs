using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents combat in the game state.
/// </summary>
public record Combat
{
    [SaveProperty("coordinate")]
    public required Position Coordinate { get; set; }

    [SaveProperty("formation_pos")]
    public required FormationPosition FormationPos { get; set; }

    [SaveProperty("formation")]
    public required Formation Formation { get; set; }

    [SaveProperty("start_coordinate")]
    public required Position StartCoordinate { get; set; }

    [SaveProperty("start_date")]
    public required string StartDate { get; init; }
} 