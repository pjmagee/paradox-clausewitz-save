namespace MageeSoft.Paradox.Clausewitz.Save.Reader.Games.Stellaris;

[Obsolete("Use Binder Attributes instead")]
public class SaveName(string name) : Attribute
{
    public string Name { get; } = name;
}