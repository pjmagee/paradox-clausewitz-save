using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a situation in the game state.
/// </summary>
public record Situation
{
    /// <summary>
    /// Gets or sets the situation ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the type of the situation.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the country ID.
    /// </summary>
    public required int Country { get; init; }

    /// <summary>
    /// Gets or sets the progress value.
    /// </summary>
    public required double Progress { get; init; }

    /// <summary>
    /// Gets or sets the last month progress value.
    /// </summary>
    public required double LastMonthProgress { get; init; }

    /// <summary>
    /// Gets or sets the approach value.
    /// </summary>
    public required string Approach { get; init; }

    /// <summary>
    /// Default instance of Situation.
    /// </summary>
    public static Situation Default => new()
    {
        Id = 0,
        Type = string.Empty,
        Country = 0,
        Progress = 0,
        LastMonthProgress = 0,
        Approach = string.Empty
    };

    /// <summary>
    /// Loads all situations from the game state document.
    /// </summary>
    /// <param name="root">The game state root object.</param>
    /// <returns>An immutable array of situations.</returns>
    public static ImmutableArray<Situation> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Situation>();

        if (!root.TryGetSaveObject("situations", out var situationsObj) || situationsObj == null)
        {
            return builder.ToImmutable();
        }

        foreach (var property in situationsObj.Properties)
        {
            if (long.TryParse(property.Key, out var id) && property.Value is SaveObject obj)
            {
                var situation = LoadSingle(id, obj);
                if (situation != null)
                {
                    builder.Add(situation);
                }
            }
        }

        return builder.ToImmutable();
    }

    private static Situation? LoadSingle(long id, SaveObject obj)
    {
        string type = string.Empty;
        int country = 0;
        double progress = 0;
        double lastMonthProgress = 0;
        string approach = string.Empty;

        foreach (var property in obj.Properties)
        {
            switch (property.Key)
            {
                case "type" when property.Value?.TryGetScalar<string>(out var typeValue) == true:
                    type = typeValue;
                    break;
                case "country" when property.Value?.TryGetScalar<int>(out var countryValue) == true:
                    country = countryValue;
                    break;
                case "progress" when property.Value?.TryGetScalar<double>(out var progressValue) == true:
                    progress = progressValue;
                    break;
                case "last_month_progress" when property.Value?.TryGetScalar<double>(out var lastMonthProgressValue) == true:
                    lastMonthProgress = lastMonthProgressValue;
                    break;
                case "approach" when property.Value?.TryGetScalar<string>(out var approachValue) == true:
                    approach = approachValue;
                    break;
            }
        }

        return new Situation
        {
            Id = id,
            Type = type,
            Country = country,
            Progress = progress,
            LastMonthProgress = lastMonthProgress,
            Approach = approach
        };
    }
} 






