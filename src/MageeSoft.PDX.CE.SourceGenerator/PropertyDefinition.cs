namespace MageeSoft.PDX.CE.SourceGenerator;

public class PropertyDefinition
{
    public string Name { get; } // PascalCase C# name
    public string PropertyType { get; } // C# type syntax
    public string OriginalName { get; } // Original PDX key name
    public bool IsCollection { get; } // For Lists/Arrays
    public bool IsDictionary { get; } // For Dictionaries (all kinds)
    public bool RepresentsDuplicateKeys { get; } // For Lists derived from duplicate keys
    public bool IsPdxDictionaryPattern { get; } // New flag for { { key {val} } ... } pattern

    public PropertyDefinition(
        string name,
        string propertyType,
        string originalName,
        bool isCollection,
        bool isDictionary,
        bool representsDuplicateKeys,
        bool isPdxDictionaryPattern)
    {
        Name = name; 
        PropertyType = propertyType;
        OriginalName = originalName;
        IsCollection = isCollection;
        IsDictionary = isDictionary;
        RepresentsDuplicateKeys = representsDuplicateKeys;
        IsPdxDictionaryPattern = isPdxDictionaryPattern;
    }
}