namespace MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SaveIndexedDictionaryAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
