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
    public required MovementTarget MovementTarget { get;set; }

    /// <summary>
    /// Gets or sets the movement path.
    /// </summary>
    public required MovementPath MovementPath { get;set; }

    /// <summary>
    /// Gets or sets the movement formation.
    /// </summary>
    public required MovementFormation MovementFormation { get;set; }

    /// <summary>
    /// Gets or sets the formation position.
    /// </summary>
    public required FormationPosition FormationPosition { get;set; }

    /// <summary>
    /// Gets or sets the coordinate.
    /// </summary>
    public required Coordinate Coordinate { get;set; }

    /// <summary>
    /// Gets or sets the target.
    /// </summary>
    public required MovementTarget Target { get;set; }

    /// <summary>
    /// Gets or sets the target coordinate.
    /// </summary>
    public required Coordinate TargetCoordinate { get;set; }

    /// <summary>
    /// Gets or sets the movement state.
    /// </summary>
    public required string State { get;set; }

    /// <summary>
    /// Gets or sets the orbit information.
    /// </summary>
    public required Orbit Orbit { get;set; }

    /// <summary>
    /// Gets or sets the last FTL jump information.
    /// </summary>
    public required FtlJump LastFtlJump { get;set; }

    /// <summary>
    /// Gets or sets the movement state.
    /// </summary>
    public required string MovementState { get;set; }

    /// <summary>
    /// Gets or sets the movement type.
    /// </summary>
    public required string MovementType { get;set; }

    /// <summary>
    /// Gets or sets the movement mode.
    /// </summary>
    public required string MovementMode { get;set; }

    /// <summary>
    /// Gets or sets the movement action.
    /// </summary>
    public required string MovementAction { get;set; }

    /// <summary>
    /// Gets or sets the movement action state.
    /// </summary>
    public required string MovementActionState { get;set; }

    /// <summary>
    /// Gets or sets the movement action type.
    /// </summary>
    public required string MovementActionType { get;set; }

    /// <summary>
    /// Gets or sets the movement action target.
    /// </summary>
    public required string MovementActionTarget { get;set; }

    /// <summary>
    /// Gets or sets the movement action target type.
    /// </summary>
    public required string MovementActionTargetType { get;set; }

    /// <summary>
    /// Gets or sets the movement action target mode.
    /// </summary>
    public required string MovementActionTargetMode { get;set; }

    /// <summary>
    /// Gets or sets the movement action target state.
    /// </summary>
    public required string MovementActionTargetState { get;set; }

    /// <summary>
    /// Gets or sets the movement action target action.
    /// </summary>
    public required string MovementActionTargetAction { get;set; }

    /// <summary>
    /// Gets or sets the movement action target action type.
    /// </summary>
    public required string MovementActionTargetActionType { get;set; }

    /// <summary>
    /// Gets or sets the movement action target action mode.
    /// </summary>
    public required string MovementActionTargetActionMode { get;set; }

    /// <summary>
    /// Gets or sets the movement action target action state.
    /// </summary>
    public required string MovementActionTargetActionState { get;set; }
}






