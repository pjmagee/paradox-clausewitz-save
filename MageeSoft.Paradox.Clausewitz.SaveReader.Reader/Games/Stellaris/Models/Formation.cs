using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

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






