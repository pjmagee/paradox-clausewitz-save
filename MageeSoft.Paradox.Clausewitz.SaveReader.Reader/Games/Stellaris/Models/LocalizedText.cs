using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents localized text in the game state.
/// </summary>
public record LocalizedText
{
    [SaveProperty("key")]
    public string Key { get; init; }
    public LocalizedTextVariable[] Variables { get; init; }
}






