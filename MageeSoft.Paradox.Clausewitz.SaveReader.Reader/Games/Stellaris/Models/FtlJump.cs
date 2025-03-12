using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an FTL jump in the game state.
/// </summary>
public class FtlJump
{
    /// <summary>
    /// Gets or sets the FTL jump type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the FTL jump state.
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the FTL jump progress.
    /// </summary>
    public float Progress { get; set; }

    /// <summary>
    /// Gets or sets the FTL jump target.
    /// </summary>
    public MovementTarget Target { get; set; } = new();

    /// <summary>
    /// Gets or sets the FTL jump path.
    /// </summary>
    public MovementPath Path { get; set; } = new();

    /// <summary>
    /// Gets or sets the FTL jump formation.
    /// </summary>
    public MovementFormation Formation { get; set; } = new();

    /// <summary>
    /// Gets or sets the FTL jump position.
    /// </summary>
    public FormationPosition Position { get; set; } = new();

    /// <summary>
    /// Loads an FTL jump from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the FTL jump data.</param>
    /// <returns>A new FtlJump instance.</returns>
    public static FtlJump Load(SaveObject saveObject)
    {
        var ftlJump = new FtlJump();

        foreach (var property in saveObject.Properties)
        {
            switch (property.Key)
            {
                case "type" when property.Value is Scalar<string> typeScalar:
                    ftlJump.Type = typeScalar.Value;
                    break;
                case "state" when property.Value is Scalar<string> stateScalar:
                    ftlJump.State = stateScalar.Value;
                    break;
                case "progress" when property.Value is Scalar<float> progressScalar:
                    ftlJump.Progress = progressScalar.Value;
                    break;
                case "target" when property.Value is SaveObject targetObj:
                    ftlJump.Target = MovementTarget.Load(targetObj);
                    break;
                case "path" when property.Value is SaveObject pathObj:
                    ftlJump.Path = MovementPath.Load(pathObj);
                    break;
                case "formation" when property.Value is SaveObject formationObj:
                    ftlJump.Formation = MovementFormation.Load(formationObj);
                    break;
                case "position" when property.Value is SaveObject positionObj:
                    ftlJump.Position = FormationPosition.Load(positionObj);
                    break;
            }
        }

        return ftlJump;
    }
}