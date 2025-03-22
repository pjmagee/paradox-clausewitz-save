namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a fleet movement manager in the game state.
/// </summary>
[SaveModel]
public partial class FleetMovementManager
{
    /// <summary>
    /// Gets or sets the movement target.
    /// </summary>
    [SaveObject("movement_target")]
    public MovementTarget MovementTarget { get;set; }

    /// <summary>
    /// Gets or sets the movement path.
    /// </summary>
    public MovementPath MovementPath { get;set; }

    /// <summary>
    /// Gets or sets the movement formation.
    /// </summary>
    public MovementFormation MovementFormation { get;set; }

    /// <summary>
    /// Gets or sets the formation position.
    /// </summary>
    public FormationPosition FormationPosition { get;set; }

    /// <summary>
    /// Gets or sets the coordinate.
    /// </summary>
    public Coordinate Coordinate { get;set; }

    /// <summary>
    /// Gets or sets the target.
    /// </summary>
    public MovementTarget Target { get;set; }

    /// <summary>
    /// Gets or sets the target coordinate.
    /// </summary>
    public Coordinate TargetCoordinate { get;set; }

    /// <summary>
    /// Gets or sets the movement state.
    /// </summary>
    public string State { get;set; }

    /// <summary>
    /// Gets or sets the orbit information.
    /// </summary>
    public Orbit Orbit { get;set; }

    /// <summary>
    /// Gets or sets the last FTL jump information.
    /// </summary>
    public FtlJump LastFtlJump { get;set; }

    /// <summary>
    /// Gets or sets the movement state.
    /// </summary>
    public string MovementState { get;set; }

    /// <summary>
    /// Gets or sets the movement type.
    /// </summary>
    public string MovementType { get;set; }

    /// <summary>
    /// Gets or sets the movement mode.
    /// </summary>
    public string MovementMode { get;set; }

    /// <summary>
    /// Gets or sets the movement action.
    /// </summary>
    public string MovementAction { get;set; }

    /// <summary>
    /// Gets or sets the movement action state.
    /// </summary>
    public string MovementActionState { get;set; }

    /// <summary>
    /// Gets or sets the movement action type.
    /// </summary>
    public string MovementActionType { get;set; }

    /// <summary>
    /// Gets or sets the movement action target.
    /// </summary>
    public string MovementActionTarget { get;set; }

    /// <summary>
    /// Gets or sets the movement action target type.
    /// </summary>
    public string MovementActionTargetType { get;set; }

    /// <summary>
    /// Gets or sets the movement action target mode.
    /// </summary>
    public string MovementActionTargetMode { get;set; }

    /// <summary>
    /// Gets or sets the movement action target state.
    /// </summary>
    public string MovementActionTargetState { get;set; }

    /// <summary>
    /// Gets or sets the movement action target action.
    /// </summary>
    public string MovementActionTargetAction { get;set; }

    /// <summary>
    /// Gets or sets the movement action target action type.
    /// </summary>
    public string MovementActionTargetActionType { get;set; }

    /// <summary>
    /// Gets or sets the movement action target action mode.
    /// </summary>
    public string MovementActionTargetActionMode { get;set; }

    /// <summary>
    /// Gets or sets the movement action target action state.
    /// </summary>
    public string MovementActionTargetActionState { get;set; }
}






