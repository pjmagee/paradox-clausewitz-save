using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a formation in the game state.
/// </summary>
public class Formation
{
    /// <summary>
    /// Gets or sets the formation ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the formation name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the formation owner.
    /// </summary>
    public long Owner { get; set; }

    /// <summary>
    /// Gets or sets the formation ships.
    /// </summary>
    public List<long> Ships { get; set; } = new();

    /// <summary>
    /// Gets or sets the formation movement target.
    /// </summary>
    public MovementTarget MovementTarget { get; set; } = new();

    /// <summary>
    /// Gets or sets the formation movement path.
    /// </summary>
    public MovementPath MovementPath { get; set; } = new();

    /// <summary>
    /// Gets or sets the formation movement formation.
    /// </summary>
    public MovementFormation MovementFormation { get; set; } = new();

    /// <summary>
    /// Gets or sets the formation position.
    /// </summary>
    public FormationPosition FormationPosition { get; set; } = new();

    /// <summary>
    /// Loads all formations from the game state.
    /// </summary>
    /// <param name="root">The game state root object to load from.</param>
    /// <returns>An immutable array of formations.</returns>
    public static ImmutableArray<Formation> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Formation>();
        var formationsElement = root.Properties
            .FirstOrDefault(p => p.Key == "formations");

        var formationsObj = formationsElement.Value as SaveObject;
        if (formationsObj != null)
        {
            foreach (var formationElement in formationsObj.Properties)
            {
                if (long.TryParse(formationElement.Key, out var formationId))
                {
                    var obj = formationElement.Value as SaveObject;
                    if (obj == null)
                    {
                        continue;
                    }

                    var formation = new Formation
                    {
                        Id = formationId,
                        Name = GetScalarString(obj, "name") ?? string.Empty,
                        Owner = GetScalarLong(obj, "owner"),
                        Ships = GetArray(obj, "ships")?.Items
                            .OfType<Scalar<long>>()
                            .Select(s => s.Value)
                            .ToList() ?? new(),
                        MovementTarget = GetObject(obj, "movement_target") is SaveObject targetObj ? MovementTarget.Load(targetObj) : new(),
                        MovementPath = GetObject(obj, "movement_path") is SaveObject pathObj ? MovementPath.Load(pathObj) : new(),
                        MovementFormation = GetObject(obj, "movement_formation") is SaveObject movementFormationObj ? MovementFormation.Load(movementFormationObj) : new(),
                        FormationPosition = GetObject(obj, "formation_position") is SaveObject positionObj ? FormationPosition.Load(positionObj) : new()
                    };

                    builder.Add(formation);
                }
            }
        }

        return builder.ToImmutable();
    }
}