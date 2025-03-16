using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a movement path node in the game state.
/// </summary>
public record MovementPathNode
{
    /// <summary>
    /// Gets or sets the coordinate.
    /// </summary>
    public required Coordinate Coordinate { get; init; }

    /// <summary>
    /// Gets or sets the FTL type.
    /// </summary>
    public required string Ftl { get; init; }

    /// <summary>
    /// Creates a new instance of MovementPathNode with default values.
    /// </summary>
    public MovementPathNode()
    {
        Coordinate = Coordinate.Default;
        Ftl = "jump_count";
    }

    /// <summary>
    /// Default instance of MovementPathNode.
    /// </summary>
    public static MovementPathNode Default { get; } = new()
    {
        Coordinate = Coordinate.Default,
        Ftl = "jump_count"
    };

    /// <summary>
    /// Loads a movement path node from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the movement path node data.</param>
    /// <returns>A new MovementPathNode instance.</returns>
    public static MovementPathNode? Load(SaveObject saveObject)
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

        string ftl;
        if (!saveObject.TryGetString("ftl", out ftl))
        {
            return null;
        }

        return new MovementPathNode
        {
            Coordinate = coordinate,
            Ftl = ftl
        };
    }
}