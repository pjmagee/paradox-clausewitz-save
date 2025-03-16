using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a ship design in the game state.
/// </summary>
public record ShipDesign
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the ship size.
    /// </summary>
    public required string ShipSize { get; init; }
   
}






