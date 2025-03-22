

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents localized text in the game state.
/// </summary>
[SaveModel]
public partial class LocalizedText
{
    [SaveScalar("key")]
    public string Key { get;set; }
    
    public LocalizedTextVariable[] Variables { get;set; }
}






