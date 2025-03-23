namespace MageeSoft.Paradox.Clausewitz.Save.Models;

[AttributeUsage(AttributeTargets.Property)]
public class SaveArrayAttribute : Attribute
{
    public string Name { get; }

    public SaveArrayAttribute(string name)
    {
        Name = name;
    }
} 