using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a fleet in the game state.
/// </summary>
public record Fleet
{
    /// <summary>
    /// Gets or sets the fleet ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the type of the fleet.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the owner ID of the fleet.
    /// </summary>
    public required int OwnerId { get; init; }

    /// <summary>
    /// Gets or sets the position of the fleet.
    /// </summary>
    public required Coordinate Position { get; init; }

    /// <summary>
    /// Gets or sets the ships in the fleet.
    /// </summary>
    public required ImmutableArray<Ship> Ships { get; init; }

    /// <summary>
    /// Loads all fleets from the game state.
    /// </summary>
    /// <param name="gameState">The game state root object to load from.</param>
    /// <returns>An immutable array of fleets.</returns>
    public static ImmutableArray<Fleet> Load(SaveObject gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState, nameof(gameState));
        
        var builder = ImmutableArray.CreateBuilder<Fleet>();
        var fleetsElement = gameState.Properties.FirstOrDefault(p => p.Key == "fleets");

        var fleetsObj = fleetsElement.Value as SaveObject;
        if (fleetsObj != null)
        {
            foreach (var fleetElement in fleetsObj.Properties)
            {
                if (long.TryParse(fleetElement.Key, out var fleetId))
                {
                    var obj = fleetElement.Value as SaveObject;
                    if (obj == null)
                    {
                        continue;
                    }

                    var type = GetScalarString(obj, "type");
                    var ownerId = GetScalarInt(obj, "owner");
                    var position = Coordinate.Load(GetObject(obj, "coordinate"));
                        
                    var ships = GetArray(obj, "ships")?.Items
                        .OfType<Scalar<int>>()
                        .Select(s => Ship.Load(gameState).First(ship => ship.Id == s.Value))
                        .ToImmutableArray() ?? ImmutableArray<Ship>.Empty;

                    if (type == null || position == null)
                    {
                        continue;
                    }

                    builder.Add(new Fleet
                    {
                        Id = fleetId,
                        Type = type,
                        OwnerId = ownerId,
                        Position = position,
                        Ships = ships
                    });
                }
            }
        }

        return builder.ToImmutable();
    }
}