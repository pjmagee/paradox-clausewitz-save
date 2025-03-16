using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a movement target in the game state.
/// </summary>
public record MovementTarget
{
    /// <summary>
    /// Gets or sets the coordinate.
    /// </summary>
    public required Coordinate Coordinate { get; init; }

    /// <summary>
    /// Creates a new instance of MovementTarget with default values.
    /// </summary>
    public MovementTarget()
    {
        Coordinate = Coordinate.Default;
    }

    /// <summary>
    /// Default instance of MovementTarget.
    /// </summary>
    public static MovementTarget Default { get; } = new()
    {
        Coordinate = Coordinate.Default
    };

    /// <summary>
    /// Loads a movement target from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the movement target data.</param>
    /// <returns>A new MovementTarget instance.</returns>
    public static MovementTarget? Load(SaveObject saveObject)
    {
        SaveObject? coordinateObj;
        if (!saveObject.TryGetSaveObject("coordinate", out coordinateObj) || coordinateObj == null)
        {
            return null;
        }

        var coordinate = Coordinate.Load(coordinateObj);
        if (coordinate == null)
        {
            return null;
        }

        return new MovementTarget
        {
            Coordinate = coordinate
        };
    }
}






