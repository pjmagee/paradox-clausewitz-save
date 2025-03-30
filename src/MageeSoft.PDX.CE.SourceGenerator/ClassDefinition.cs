namespace MageeSoft.PDX.CE.SourceGenerator;

public class ClassDefinition
{
    public string Name { get; } // Name is now determined by analyzer uniquely
    public string FullName { get; internal set; } = string.Empty; // Added: Full concatenated name
    public List<PropertyDefinition> Properties { get; } = new();
    
    // List to hold definitions of classes nested directly within this one
    public List<ClassDefinition> NestedClasses { get; } = new();
    
    // Set to track names of nested classes/members within this class's scope
    internal readonly HashSet<string> NestedClassNames = new(); // Internal for analyzer access
    public bool IsSimpleType { get; internal set; } = false; // Added flag for simple types
    
    // Track the original path in the save file structure
    public string OriginalPath { get; set; } = string.Empty;

    public ClassDefinition(string name)
    {
        Name = name;
    }
}