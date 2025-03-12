using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an army in the game state.
/// </summary>
public record Army
{
    /// <summary>
    /// Gets or sets the army ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the name of the army.
    /// </summary>
    public LocalizedText Name { get; set; } = new();

    /// <summary>
    /// Gets or sets the type of the army.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the health of the army.
    /// </summary>
    public int Health { get; set; }

    /// <summary>
    /// Gets or sets the maximum health of the army.
    /// </summary>
    public int MaxHealth { get; set; }

    /// <summary>
    /// Gets or sets the jump drive cooldown.
    /// </summary>
    public string JumpDriveCooldown { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the home planet ID.
    /// </summary>
    public long HomePlanet { get; set; }

    /// <summary>
    /// Gets or sets the owner ID.
    /// </summary>
    public long Owner { get; set; }

    /// <summary>
    /// Gets or sets the ship ID.
    /// </summary>
    public long Ship { get; set; }

    /// <summary>
    /// Gets or sets the leader ID.
    /// </summary>
    public long? Leader { get; set; }

    /// <summary>
    /// Gets or sets the morale value.
    /// </summary>
    public int Morale { get; set; }

    /// <summary>
    /// Gets or sets the country ID.
    /// </summary>
    public required int Country { get; init; }

    /// <summary>
    /// Gets or sets the planet ID.
    /// </summary>
    public required int Planet { get; init; }

    /// <summary>
    /// Gets or sets whether the army is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Loads all armies from the game save documents.
    /// </summary>
    /// <param name="documents">The game save documents to load from.</param>
    /// <returns>An immutable array of armies.</returns>
    public static ImmutableArray<Army> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Army>();
        var armiesElement = root.Properties.FirstOrDefault(p => p.Key == "armies");

        var armiesObj = armiesElement.Value as SaveObject;
        if (armiesObj != null)
        {
            foreach (var armyElement in armiesObj.Properties)
            {
                if (long.TryParse(armyElement.Key, out var armyId))
                {
                    var obj = armyElement.Value as SaveObject;
                    if (obj == null)
                    {
                        continue;
                    }

                    var type = GetScalarString(obj, "type");
                    var country = GetScalarInt(obj, "country");
                    var planet = GetScalarInt(obj, "planet");
                    var isActive = GetScalarBoolean(obj, "is_active");

                    if (type == null)
                    {
                        continue;
                    }

                    builder.Add(new Army
                    {
                        Id = armyId,
                        Type = type,
                        Country = country,
                        Planet = planet,
                        IsActive = isActive
                    });
                }
            }
        }

        return builder.ToImmutable();
    }
} 