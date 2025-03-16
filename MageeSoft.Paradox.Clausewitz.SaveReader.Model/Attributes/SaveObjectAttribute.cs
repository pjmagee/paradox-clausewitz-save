namespace MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SaveObjectAttribute : Attribute
{
    public string Name { get; }

    public SaveObjectAttribute(string name)
    {
        Name = name;
    }
} 