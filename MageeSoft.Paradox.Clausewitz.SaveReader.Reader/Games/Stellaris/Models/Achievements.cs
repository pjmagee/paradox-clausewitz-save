using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents achievements in the game state.
/// </summary>
public class Achievements
{
    /// <summary>
    /// Gets or sets the list of achievement IDs.
    /// </summary>
    public ImmutableArray<int> AchievementIds { get; init; } = ImmutableArray<int>.Empty;

    /// <summary>
    /// Loads achievements from the game save documents.
    /// </summary>
    /// <param name="documents">The game save documents to load from.</param>
    /// <returns>The loaded achievements.</returns>
    public static Achievements Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<int>();
        
        var achievementElement = root.Properties.FirstOrDefault(p => p.Key == "achievement");
            
        if (achievementElement.Value is SaveArray array)
        {
            foreach (var element in array.Items)
            {
                if (element.TryGetScalar<int>(out var value))
                {
                    builder.Add(value);
                }
            }
        }           

        return new Achievements { AchievementIds = builder.ToImmutable() };
    }
} 