using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a completed stage in first contact.
/// </summary>
public record CompletedStage
{
    /// <summary>
    /// Gets or sets the date when the stage was completed.
    /// </summary>
    public required string Date { get; init; }

    /// <summary>
    /// Gets or sets the stage name.
    /// </summary>
    public required string Stage { get; init; }

    /// <summary>
    /// Default instance of CompletedStage.
    /// </summary>
    public static CompletedStage Default => new()
    {
        Date = "2200.01.01",
        Stage = string.Empty
    };

    /// <summary>
    /// Loads a completed stage from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the completed stage data.</param>
    /// <returns>A new CompletedStage instance with default values if properties are missing. The date defaults to "2200.01.01" and stage defaults to empty string.</returns>
    public static CompletedStage Load(SaveObject saveObject)
    {
        return new CompletedStage
        {
            Date = saveObject.TryGetString("date", out var date) ? date : "2200.01.01",
            Stage = saveObject.TryGetString("stage", out var stage) ? stage : string.Empty
        };
    }
}






