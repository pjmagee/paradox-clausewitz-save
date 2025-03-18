namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a deposit in the game state.
/// </summary>
public record Deposit
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public required int Amount { get; init; }

    /// <summary>
    /// Gets or sets whether the deposit is infinite.
    /// </summary>
    public required bool Infinite { get; init; }

    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    public Position? Position { get; init; }
    
} 






