using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a formation in the game state.
/// </summary>
public class Formation
{
    [SaveScalar("scale")]
    public float? Scale { get; set; }

    [SaveScalar("angle")]
    public float? Angle { get; set; }

    [SaveScalar("type")]
    public string? Type { get; set; }
    
    [SaveScalar("root")]
    public int? Root { get; set; }
    
    [SaveArray("ships")]
    public int[]? Ships { get; set; }
    
    [SaveArray("parent")]
    public int[]? Parent { get; set; }
}






