using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a movement target in the game state.
/// </summary>
public class MovementTarget
{
    /// <summary>
    /// Gets or sets the target ID.
    /// </summary>
    public long Target { get; set; }

    /// <summary>
    /// Gets or sets the target type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Loads a movement target from a SaveElement.
    /// </summary>
    /// <param name="clausewitzElement">The SaveElement containing the movement target data.</param>
    /// <returns>A new MovementTarget instance.</returns>
    public static MovementTarget Load(SaveElement clausewitzElement)
    {
        var target = new MovementTarget();
        var targetObj = clausewitzElement as SaveObject;
        if (targetObj != null)
        {
            foreach (var property in targetObj.Properties)
            {
                switch (property.Key)
                {
                    case "target" when property.Value is Scalar<long> targetScalar:
                        target.Target = targetScalar.Value;
                        break;
                    case "type" when property.Value is Scalar<string> typeScalar:
                        target.Type = typeScalar.Value;
                        break;
                }
            }
        }

        return target;
    }
}