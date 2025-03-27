using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Parser;

public class SaveArray : SaveElement
{
    public List<SaveElement> Items { get; }
    
    public override SaveType Type => SaveType.Array;
    
    public SaveArray(List<SaveElement> items)
    {
        Items = items;
    }
}