using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a ship design in the game state.
/// </summary>
public class ShipDesign
{
    /// <summary>
    /// Gets or sets the ship design ID.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets or sets the ship design name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the ship design owner.
    /// </summary>
    public long Owner { get; init; }

    /// <summary>
    /// Gets or sets the ship design type.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the ship design size.
    /// </summary>
    public string Size { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the ship size class.
    /// </summary>
    public string ShipSize { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the ship design sections.
    /// </summary>
    public ImmutableDictionary<string, string> Sections { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets or sets the ship design components.
    /// </summary>
    public ImmutableDictionary<string, string> Components { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets or sets the ship design utilities.
    /// </summary>
    public ImmutableDictionary<string, string> Utilities { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets or sets the ship design weapons.
    /// </summary>
    public ImmutableDictionary<string, string> Weapons { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets or sets the ship design cost.
    /// </summary>
    public ImmutableDictionary<string, float> Cost { get; init; } = ImmutableDictionary<string, float>.Empty;

    /// <summary>
    /// Gets or sets the ship design stats.
    /// </summary>
    public ImmutableDictionary<string, float> Stats { get; init; } = ImmutableDictionary<string, float>.Empty;

    /// <summary>
    /// Loads all ship designs from the game state document.
    /// </summary>
    /// <param name="gameState">The game state document to load from.</param>
    /// <returns>An immutable array of ship designs.</returns>
    public static ImmutableArray<ShipDesign> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<ShipDesign>();

        var designsObj = GetObject(root, "ship_designs");
            if (designsObj != null)
            {
                foreach (var designElement in designsObj.Properties)
                {
                    if (long.TryParse(designElement.Key, out var designId) && designElement.Value is SaveObject designObj)
                    {
                        var sectionsBuilder = ImmutableDictionary.CreateBuilder<string, string>();
                        var componentsBuilder = ImmutableDictionary.CreateBuilder<string, string>();
                        var utilitiesBuilder = ImmutableDictionary.CreateBuilder<string, string>();
                        var weaponsBuilder = ImmutableDictionary.CreateBuilder<string, string>();
                        var costBuilder = ImmutableDictionary.CreateBuilder<string, float>();
                        var statsBuilder = ImmutableDictionary.CreateBuilder<string, float>();

                        var size = GetScalarString(designObj, "size");

                        if (GetObject(designObj, "sections") is SaveObject sectionsObj)
                        {
                            foreach (var section in sectionsObj.Properties)
                            {
                                if (section.Value is Scalar<string> scalar)
                                {
                                    sectionsBuilder.Add(section.Key, scalar.Value);
                                }
                            }
                        }

                        if (GetObject(designObj, "components") is SaveObject componentsObj)
                        {
                            foreach (var component in componentsObj.Properties)
                            {
                                if (component.Value is Scalar<string> scalar)
                                {
                                    componentsBuilder.Add(component.Key, scalar.Value);
                                }
                            }
                        }

                        if (GetObject(designObj, "utilities") is SaveObject utilitiesObj)
                        {
                            foreach (var utility in utilitiesObj.Properties)
                            {
                                if (utility.Value is Scalar<string> scalar)
                                {
                                    utilitiesBuilder.Add(utility.Key, scalar.Value);
                                }
                            }
                        }

                        if (GetObject(designObj, "weapons") is SaveObject weaponsObj)
                        {
                            foreach (var weapon in weaponsObj.Properties)
                            {
                                if (weapon.Value is Scalar<string> scalar)
                                {
                                    weaponsBuilder.Add(weapon.Key, scalar.Value);
                                }
                            }
                        }

                        if (GetObject(designObj, "cost") is SaveObject costObj)
                        {
                            foreach (var cost in costObj.Properties)
                            {
                                if (cost.Value is Scalar<float> scalar)
                                {
                                    costBuilder.Add(cost.Key, scalar.Value);
                                }
                            }
                        }

                        if (GetObject(designObj, "stats") is SaveObject statsObj)
                        {
                            foreach (var stat in statsObj.Properties)
                            {
                                if (stat.Value is Scalar<float> scalar)
                                {
                                    statsBuilder.Add(stat.Key, scalar.Value);
                                }
                            }
                        }

                        var design = new ShipDesign
                        {
                            Id = designId,
                            Name = GetScalarString(designObj, "name"),
                            Owner = GetScalarLong(designObj, "owner"),
                            Type = GetScalarString(designObj, "type"),
                            Size = size,
                            ShipSize = size,
                            Sections = sectionsBuilder.ToImmutable(),
                            Components = componentsBuilder.ToImmutable(),
                            Utilities = utilitiesBuilder.ToImmutable(),
                            Weapons = weaponsBuilder.ToImmutable(),
                            Cost = costBuilder.ToImmutable(),
                            Stats = statsBuilder.ToImmutable()
                        };

                        builder.Add(design);
                    }
                }
            }

        return builder.ToImmutable();
    }
}