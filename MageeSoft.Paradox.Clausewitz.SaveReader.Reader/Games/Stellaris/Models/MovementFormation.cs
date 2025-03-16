using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a movement formation in the game state.
/// </summary>
public record MovementFormation
{
    /// <summary>
    /// Gets or sets the scale.
    /// </summary>
    public required float Scale { get; init; }

    /// <summary>
    /// Gets or sets the angle.
    /// </summary>
    public required float Angle { get; init; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Creates a new instance of MovementFormation with default values.
    /// </summary>
    public MovementFormation()
    {
        Scale = 1f;
        Angle = -0.78561f;
        Type = "wedge";
    }

    /// <summary>
    /// Default instance of MovementFormation.
    /// </summary>
    public static MovementFormation Default { get; } = new()
    {
        Scale = 1f,
        Angle = -0.78561f,
        Type = "wedge"
    };

    /// <summary>
    /// Loads a movement formation from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the movement formation data.</param>
    /// <returns>A new MovementFormation instance.</returns>
    public static MovementFormation? Load(SaveObject saveObject)
    {
        if (!saveObject.TryGetString("type", out var type))
        {
            return null;
        }

        return new MovementFormation
        {
            Scale = saveObject.TryGetFloat("scale", out var scale) ? scale : 1f,
            Angle = saveObject.TryGetFloat("angle", out var angle) ? angle : -0.78561f,
            Type = type
        };
    }
}






