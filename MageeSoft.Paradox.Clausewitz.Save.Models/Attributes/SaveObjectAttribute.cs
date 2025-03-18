namespace MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SaveObjectAttribute(string name) : Attribute
{
    public string Name { get; } = name;
} 