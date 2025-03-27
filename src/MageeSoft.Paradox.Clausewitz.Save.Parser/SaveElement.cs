namespace MageeSoft.Paradox.Clausewitz.Save.Parser;

/// <summary>
/// Base class for elements in a save file.
/// </summary>
public abstract class SaveElement
{
    public abstract SaveType Type { get; }

    public override string ToString()
    {
        var serializer = new Serializer();
        serializer.Serialize(this);
        return serializer.ToString()!;
    }
}