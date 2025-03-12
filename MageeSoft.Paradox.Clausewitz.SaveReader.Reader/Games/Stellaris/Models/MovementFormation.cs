using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a movement formation in the game state.
/// </summary>
public class MovementFormation
{
    /// <summary>
    /// Gets or sets the scale.
    /// </summary>
    public float Scale { get; set; }

    /// <summary>
    /// Gets or sets the angle.
    /// </summary>
    public float Angle { get; set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Loads a movement formation from a SaveElement.
    /// </summary>
    /// <param name="clausewitzElement">The SaveElement containing the movement formation data.</param>
    /// <returns>A new MovementFormation instance.</returns>
    public static MovementFormation Load(SaveElement clausewitzElement)
    {
        var formation = new MovementFormation();
        var formationObj = clausewitzElement as SaveObject;
        if (formationObj != null)
        {
            foreach (var property in formationObj.Properties)
            {
                switch (property.Key)
                {
                    case "scale" when property.Value is Scalar<float> scaleScalar:
                        formation.Scale = scaleScalar.Value;
                        break;
                    case "angle" when property.Value is Scalar<float> angleScalar:
                        formation.Angle = angleScalar.Value;
                        break;
                    case "type" when property.Value is Scalar<string> typeScalar:
                        formation.Type = typeScalar.Value;
                        break;
                }
            }
        }

        return formation;
    }
}