namespace MageeSoft.PDX.CE.SourceGenerator;

public class PropertyDefinition
{
    public string Name { get; } // PascalCase C# name
    public string PropertyType { get; } // C# type syntax
    
    // public string AttributeType { get; } // REMOVED
    
    public string OriginalName { get; } // Original PDX key name
    public bool IsCollection { get; } // For Lists/Arrays
    public bool IsDictionary { get; } // For Dictionaries (all kinds)
    public bool RepresentsDuplicateKeys { get; } // For Lists derived from duplicate keys
    public bool IsPdxDictionaryPattern { get; } // New flag for { { key {val} } ... } pattern

    // Constructor updated to remove attributeType and add isPdxDictionaryPattern
    public PropertyDefinition(string name, string propertyType, /*string attributeType,*/ string originalName, bool isCollection, bool isDictionary, bool representsDuplicateKeys, bool isPdxDictionaryPattern)
    {
        Name = name; // This should be the simple PascalCase name (e.g., "Army"), not the hierarchical one
        PropertyType = propertyType;
        // AttributeType = attributeType; // Removed
        OriginalName = originalName;
        IsCollection = isCollection;
        IsDictionary = isDictionary;
        RepresentsDuplicateKeys = representsDuplicateKeys;
        IsPdxDictionaryPattern = isPdxDictionaryPattern; // Assign new flag
    }
}