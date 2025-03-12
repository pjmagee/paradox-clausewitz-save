using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a ship in the game state.
/// </summary>
public record Ship
{
    public long Id { get; init; }
    public string Type { get; init; }
    public int Design { get; init; }
    public Coordinate Coordinate { get; init; }
    public float Health { get; init; }
    public float MaxHealth { get; init; }
    public string Name { get; init; }
    public long Fleet { get; init; }

    public Ship(long id, string type, int design, Coordinate coordinate, float health, float maxHealth)
    {
        Id = id;
        Type = type;
        Design = design;
        Coordinate = coordinate;
        Health = health;
        MaxHealth = maxHealth;
        Name = string.Empty;
        Fleet = 0;
    }

    /// <summary>
    /// Loads all ships from the game state.
    /// </summary>
    /// <param name="gameState">The game state root object to load from.</param>
    /// <returns>An immutable array of ships.</returns>
    public static ImmutableArray<Ship> Load(SaveObject gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState, nameof(gameState));
        
        var builder = ImmutableArray.CreateBuilder<Ship>();
        var shipsElement = gameState.Properties.FirstOrDefault(p => p.Key == "ships");

        var shipsObj = shipsElement.Value as SaveObject;
        if (shipsObj != null)
        {
            foreach (var shipElement in shipsObj.Properties)
            {
                if (long.TryParse(shipElement.Key, out var shipId))
                {
                    var obj = shipElement.Value as SaveObject;
                    if (obj == null)
                    {
                        continue;
                    }

                    var type = GetScalarString(obj, "type");
                    var design = GetScalarInt(obj, "design");
                    var coordinate = Coordinate.Load(GetObject(obj, "coordinate"));
                    var health = GetScalarFloat(obj, "health");
                    var maxHealth = GetScalarFloat(obj, "max_health");
                    var name = GetScalarString(obj, "name") ?? string.Empty;
                    var fleet = GetScalarLong(obj, "fleet");

                    if (type == null || coordinate == null)
                    {
                        continue;
                    }

                    var ship = new Ship(shipId, type, design, coordinate, health, maxHealth)
                    {
                        Name = name,
                        Fleet = fleet
                    };
                    builder.Add(ship);
                }
            }
        }

        return builder.ToImmutable();
    }
}