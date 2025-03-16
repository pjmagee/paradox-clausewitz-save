namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;

[Obsolete("Use Binder Attributes instead")]
public class SaveName(string name) : Attribute
{
    public string Name { get; } = name;
}