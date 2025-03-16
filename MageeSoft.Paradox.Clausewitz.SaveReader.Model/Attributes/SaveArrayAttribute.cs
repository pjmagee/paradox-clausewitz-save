namespace MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SaveArrayAttribute : Attribute
{
    public string Name { get; }

    public SaveArrayAttribute(string name)
    {
        Name = name;
    }
}

