namespace MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SavePropertyAttribute : Attribute
{
    public string Name { get; }

    public SavePropertyAttribute(string name)
    {
        Name = name;
    }
} 