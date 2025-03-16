namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an astral rift event.
/// </summary>
public record AstralRiftEvent
{
    /// <summary>
    /// Gets or sets the scope information.
    /// </summary>
    public required AstralRiftEventScope Scope { get; init; }

    /// <summary>
    /// Gets or sets the effect.
    /// </summary>
    public required string Effect { get; init; }

    /// <summary>
    /// Gets or sets the picture.
    /// </summary>
    public required string Picture { get; init; }

    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// Default instance of AstralRiftEvent.
    /// </summary>
    public static AstralRiftEvent Default => new()
    {
        Scope = AstralRiftEventScope.Default,
        Effect = string.Empty,
        Picture = string.Empty,
        Index = 0
    };
}