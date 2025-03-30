// Added for LINQ usage
// Added for Dictionary



// For ToPascalCase if moved here
// Using ConcurrentDictionary for thread safety if needed later

// Add Regex for parsing types later if needed

namespace MageeSoft.PDX.CE.SourceGenerator;

// --- Supporting classes remain the same ---
// Minor update to PropertyDefinition constructor for clarity

public class ClassDefinition
{
    public string Name { get; } // Name is now determined by analyzer uniquely
    public string FullName { get; internal set; } = string.Empty; // Added: Full concatenated name
    public List<PropertyDefinition> Properties { get; } = new();
    // List to hold definitions of classes nested directly within this one
    public List<ClassDefinition> NestedClasses { get; } = new();
    // Set to track names of nested classes/members within this class's scope
    internal HashSet<string> _nestedClassNames = new(); // Internal for analyzer access
    public bool IsSimpleType { get; internal set; } = false; // Added flag for simple types

    public ClassDefinition(string name)
    {
        Name = name;
    }

    // Optional: Override Equals/GetHashCode based on signature if comparing definitions
}

// Simple struct to hold diagnostic info before reporting