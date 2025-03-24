namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents combat in the game state.
/// </summary>
[SaveModel]
public partial class Combat
{
    [SaveObject("coordinate")]
    public Position? Coordinate { get; set; }

    [SaveObject("formation_pos")]
    public FormationPosition? FormationPos { get; set; }

    [SaveObject("formation")]
    public Formation? Formation { get; set; }

    [SaveObject("start_coordinate")]
    public Position? StartCoordinate { get; set; }

    [SaveScalar("start_date")]
    public string? StartDate { get;set; }
} 