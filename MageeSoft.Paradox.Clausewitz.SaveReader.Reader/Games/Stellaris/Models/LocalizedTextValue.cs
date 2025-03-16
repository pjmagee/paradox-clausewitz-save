using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public class LocalizedTextValue
{
    [SaveProperty("key")]
    public string Key { get; init; }
}