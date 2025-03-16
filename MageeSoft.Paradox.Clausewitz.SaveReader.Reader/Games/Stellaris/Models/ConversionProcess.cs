using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a conversion process in the game state.
/// </summary>
public class ConversionProcess
{
    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    [SaveName("progress")]
    public float Progress { get; init; }

    /// <summary>
    /// Gets or sets whether the process is in progress.
    /// </summary>
    [SaveName("in_progress")]
    public bool InProgress { get; init; }

    /// <summary>
    /// Gets or sets whether the process is done.
    /// </summary>
    [SaveName("done")]
    public bool Done { get; init; }

    /// <summary>
    /// Gets or sets whether to ignore the process.
    /// </summary>
    [SaveName("ignore")]
    public bool Ignore { get; init; }

    /// <summary>
    /// Default instance of ConversionProcess.
    /// </summary>
    public static ConversionProcess Default { get; } = new();

    /// <summary>
    /// Loads a conversion process from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the conversion process data.</param>
    /// <returns>A new ConversionProcess instance.</returns>
    public static ConversionProcess? Load(SaveObject saveObject)
    {
        if (!saveObject.TryGetFloat("progress", out var progress) ||
            !saveObject.TryGetBool("in_progress", out var inProgress) ||
            !saveObject.TryGetBool("done", out var done) ||
            !saveObject.TryGetBool("ignore", out var ignore))
        {
            return null;
        }

        return new ConversionProcess
        {
            Progress = progress,
            InProgress = inProgress,
            Done = done,
            Ignore = ignore
        };
    }
}