namespace MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SaveIndexedDictionaryAttribute : Attribute
{
    public string Name { get; }

    public SaveIndexedDictionaryAttribute(string name)
    {
        Name = name;
    }
}
