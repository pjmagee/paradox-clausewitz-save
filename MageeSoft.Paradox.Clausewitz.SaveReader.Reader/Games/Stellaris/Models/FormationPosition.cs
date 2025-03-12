using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a formation position in the game state.
/// </summary>
public class FormationPosition
{
    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// Gets or sets the Z coordinate.
    /// </summary>
    public float Z { get; set; }

    /// <summary>
    /// Gets or sets the speed.
    /// </summary>
    public float Speed { get; set; }

    /// <summary>
    /// Gets or sets the rotation.
    /// </summary>
    public float Rotation { get; set; }

    /// <summary>
    /// Gets or sets the forward X component.
    /// </summary>
    public float ForwardX { get; set; }

    /// <summary>
    /// Gets or sets the forward Y component.
    /// </summary>
    public float ForwardY { get; set; }

    /// <summary>
    /// Loads a formation position from a SaveElement.
    /// </summary>
    /// <param name="element">The SaveElement containing the formation position data.</param>
    /// <returns>A new FormationPosition instance.</returns>
    public static FormationPosition Load(SaveElement element)
    {
        var position = new FormationPosition();
        var posObj = element as SaveObject;
        if (posObj != null)
        {
            foreach (var property in posObj.Properties)
            {
                switch (property.Key)
                {
                    case "x" when property.Value is Scalar<float> xScalar:
                        position.X = xScalar.Value;
                        break;
                    case "y" when property.Value is Scalar<float> yScalar:
                        position.Y = yScalar.Value;
                        break;
                    case "z" when property.Value is Scalar<float> zScalar:
                        position.Z = zScalar.Value;
                        break;
                    case "speed" when property.Value is Scalar<float> speedScalar:
                        position.Speed = speedScalar.Value;
                        break;
                    case "rotation" when property.Value is Scalar<float> rotationScalar:
                        position.Rotation = rotationScalar.Value;
                        break;
                    case "forward_x" when property.Value is Scalar<float> forwardXScalar:
                        position.ForwardX = forwardXScalar.Value;
                        break;
                    case "forward_y" when property.Value is Scalar<float> forwardYScalar:
                        position.ForwardY = forwardYScalar.Value;
                        break;
                }
            }
        }

        return position;
    }
}