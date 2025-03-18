using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents localized text in the game state.
/// </summary>
public record LocalizedText
{
    [SaveScalar("key")]
    public string Key { get; init; }
    
    public LocalizedTextVariable[] Variables { get; init; }
}






