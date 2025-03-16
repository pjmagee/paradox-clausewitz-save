namespace MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SaveScalarAttribute : Attribute
{
    public string Name { get; }

    public SaveScalarAttribute(string name)
    {
        Name = name;
    }
} 
