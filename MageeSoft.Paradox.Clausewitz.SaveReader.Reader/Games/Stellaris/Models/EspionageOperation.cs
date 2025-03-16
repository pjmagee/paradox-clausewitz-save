using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an espionage operation in the game state.
/// </summary>
public record EspionageOperation
{
    /// <summary>
    /// Gets or sets the operation ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the type of operation.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the country performing the operation.
    /// </summary>
    public required int Country { get; init; }

    /// <summary>
    /// Gets or sets the target country of the operation.
    /// </summary>
    public required int TargetCountry { get; init; }

    /// <summary>
    /// Default instance of EspionageOperation.
    /// </summary>
    public static EspionageOperation Default => new()
    {
        Id = 0,
        Type = string.Empty,
        Country = 0,
        TargetCountry = 0
    };

    /// <summary>
    /// Loads all espionage operations from the game state.
    /// </summary>
    /// <param name="root">The game state root object.</param>
    /// <returns>An immutable array of espionage operations.</returns>
    public static ImmutableArray<EspionageOperation> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<EspionageOperation>();

        if (!Extensions.TryGetSaveObject(root, "espionage_operations", out var operationsObj))
        {
            return builder.ToImmutable();
        }

        foreach (var operationElement in operationsObj!.Properties)
        {
            if (long.TryParse(operationElement.Key, out var operationId) && operationElement.Value is SaveObject obj)
            {
                var operation = LoadSingle(obj);
                if (operation != null)
                {
                    builder.Add(operation with { Id = operationId });
                }
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Loads a single espionage operation from a SaveObject.
    /// </summary>
    /// <param name="obj">The SaveObject containing the operation data.</param>
    /// <returns>A new EspionageOperation instance if successful, null if any required property is missing. Required properties are: type, country, and target_country.</returns>
    private static EspionageOperation? LoadSingle(SaveObject obj)
    {
        string typeValue;
        int countryValue;
        int targetCountryValue;

        if (!Extensions.TryGetString(obj, "type", out typeValue) ||
            !Extensions.TryGetInt(obj, "country", out countryValue) ||
            !Extensions.TryGetInt(obj, "target_country", out targetCountryValue))
        {
            return null;
        }

        return new EspionageOperation
        {
            Id = 0, // Will be set by the caller
            Type = typeValue,
            Country = countryValue,
            TargetCountry = targetCountryValue
        };
    }
} 






