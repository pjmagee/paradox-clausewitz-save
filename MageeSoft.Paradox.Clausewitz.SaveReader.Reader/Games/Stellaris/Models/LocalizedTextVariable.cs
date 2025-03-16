using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;


public class LocalizedTextVariable
{
    [SaveProperty("key")]
    public required string Key { get; set; }
    
    [SaveProperty("value")]
    public required LocalizedTextValue Value { get; set; }
}