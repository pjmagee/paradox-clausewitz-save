using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a subject conversion process in the game state.
/// </summary>
public record SubjectConversionProcess
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    public required float Progress { get; init; }

    /// <summary>
    /// Default instance of SubjectConversionProcess.
    /// </summary>
    public static SubjectConversionProcess Default => new()
    {
        Id = 0,
        Type = string.Empty,
        Progress = 0f
    };

    /// <summary>
    /// Loads a subject conversion process from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the subject conversion process data.</param>
    /// <returns>A new SubjectConversionProcess instance.</returns>
    public static SubjectConversionProcess? Load(SaveObject saveObject)
    {
        long id;
        string type;
        float progress;

        if (!saveObject.TryGetLong("id", out id) ||
            !saveObject.TryGetString("type", out type) ||
            !saveObject.TryGetFloat("progress", out progress))
        {
            return null;
        }

        return new SubjectConversionProcess
        {
            Id = id,
            Type = type,
            Progress = progress
        };
    }
}






