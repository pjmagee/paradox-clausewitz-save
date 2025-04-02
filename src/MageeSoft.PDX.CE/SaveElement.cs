namespace MageeSoft.PDX.CE;

/// <summary>
/// Base class for elements in a save file.
/// </summary>
public abstract class SaveElement
{
    public abstract SaveType Type { get; }
    
    /// <summary>
    /// The original path of the element in the save file hierarchy.
    /// Used by the source generator for better code generation.
    /// </summary>
    public string? OriginalPath { get; set; }

    public override string ToString()
    {
        var serializer = new Serializer();
        serializer.Serialize(this);
        return serializer.ToString()!;
    }
}