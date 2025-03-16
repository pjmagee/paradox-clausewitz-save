using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a formation in the game state.
/// </summary>
public class Formation
{
    [SaveProperty("scale")]
    public float? Scale { get; set; }

    [SaveProperty("angle")]
    public float? Angle { get; set; }

    [SaveProperty("type")]
    public string? Type { get; set; }
    
    [SaveProperty("root")]
    public int? Root { get; set; }
    
    [SaveProperty("ships")]
    public int[]? Ships { get; set; }
    
    [SaveProperty("parent")]
    public int[]? Parent { get; set; }
}






