using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

public class SaveArray : SaveElement
{
    public ImmutableArray<SaveElement> Items { get; }
    
    public override SaveType Type => SaveType.Array;
    
    public SaveArray(ImmutableArray<SaveElement> items)
    {
        Items = items;
    }
}