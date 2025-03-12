using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a fleet movement manager in the game state.
/// </summary>
public class FleetMovementManager
{
    /// <summary>
    /// Gets or sets the movement target.
    /// </summary>
    public MovementTarget MovementTarget { get; set; } = new();

    /// <summary>
    /// Gets or sets the movement path.
    /// </summary>
    public MovementPath MovementPath { get; set; } = new();

    /// <summary>
    /// Gets or sets the movement formation.
    /// </summary>
    public MovementFormation MovementFormation { get; set; } = new();

    /// <summary>
    /// Gets or sets the formation position.
    /// </summary>
    public FormationPosition FormationPosition { get; set; } = new();

    /// <summary>
    /// Gets or sets the coordinate.
    /// </summary>
    public Coordinate Coordinate { get; set; } = new();

    /// <summary>
    /// Gets or sets the target.
    /// </summary>
    public MovementTarget Target { get; set; } = new();

    /// <summary>
    /// Gets or sets the target coordinate.
    /// </summary>
    public Coordinate TargetCoordinate { get; set; } = new();

    /// <summary>
    /// Gets or sets the movement state.
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the orbit information.
    /// </summary>
    public Orbit Orbit { get; set; } = new();

    /// <summary>
    /// Gets or sets the last FTL jump information.
    /// </summary>
    public FtlJump LastFtlJump { get; set; } = new();

    public string MovementState { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public string MovementMode { get; set; } = string.Empty;
    public string MovementAction { get; set; } = string.Empty;
    public string MovementActionState { get; set; } = string.Empty;
    public string MovementActionType { get; set; } = string.Empty;
    public string MovementActionTarget { get; set; } = string.Empty;
    public string MovementActionTargetType { get; set; } = string.Empty;
    public string MovementActionTargetMode { get; set; } = string.Empty;
    public string MovementActionTargetState { get; set; } = string.Empty;
    public string MovementActionTargetAction { get; set; } = string.Empty;
    public string MovementActionTargetActionType { get; set; } = string.Empty;
    public string MovementActionTargetActionMode { get; set; } = string.Empty;
    public string MovementActionTargetActionState { get; set; } = string.Empty;

    /// <summary>
    /// Loads a fleet movement manager from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the fleet movement manager data.</param>
    /// <returns>A new FleetMovementManager instance.</returns>
    public static FleetMovementManager Load(SaveObject saveObject)
    {
        var manager = new FleetMovementManager();

        foreach (var property in saveObject.Properties)
        {
            switch (property.Key)
            {
                case "movement_target" when property.Value is SaveObject targetObj:
                    manager.MovementTarget = MovementTarget.Load(targetObj);
                    break;
                case "movement_path" when property.Value is SaveObject pathObj:
                    manager.MovementPath = MovementPath.Load(pathObj);
                    break;
                case "movement_formation" when property.Value is SaveObject formationObj:
                    manager.MovementFormation = MovementFormation.Load(formationObj);
                    break;
                case "formation_position" when property.Value is SaveObject positionObj:
                    manager.FormationPosition = FormationPosition.Load(positionObj);
                    break;
            }
        }

        return manager;
    }
}