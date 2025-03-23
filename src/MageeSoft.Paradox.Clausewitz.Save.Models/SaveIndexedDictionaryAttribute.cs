namespace MageeSoft.Paradox.Clausewitz.Save.Models;

[AttributeUsage(AttributeTargets.Property)]
public class SaveIndexedDictionaryAttribute : Attribute
{
    public string Name { get; }

    public SaveIndexedDictionaryAttribute(string name)
    {
        Name = name;
    }
} 