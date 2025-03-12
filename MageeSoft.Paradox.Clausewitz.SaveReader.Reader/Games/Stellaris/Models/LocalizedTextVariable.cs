namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a variable in localized text.
/// </summary>
public class LocalizedTextVariable
{
    /// <summary>
    /// Gets or sets the variable key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the variable value.
    /// </summary>
    public LocalizedText Value { get; set; } = new();
}