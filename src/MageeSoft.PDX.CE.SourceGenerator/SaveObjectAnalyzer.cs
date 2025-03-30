using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;

namespace MageeSoft.PDX.CE.SourceGenerator;

/// <summary>
/// Enhanced analyzer that provides additional functionality for analyzing PDX save files
/// </summary>
internal class SaveObjectAnalyzer
{
    private Dictionary<string, ClassDefinition> _definedClassesBySignature = new();
    private HashSet<string> _generatedTopLevelClassNames = new();
    private readonly static TextInfo TextInfo = CultureInfo.InvariantCulture.TextInfo;

    // Shared collection of C# keywords and reserved types to avoid duplication
    private static readonly HashSet<string> ReservedTypeNames = new(StringComparer.OrdinalIgnoreCase)
    {
        // C# keywords
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
        "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
        "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
        "virtual", "void", "volatile", "while",
        
        // Common .NET types
        "DateTime", "TimeSpan", "Guid", "Uri", "Version", "Type", "Exception",
        "List", "Dictionary", "HashSet", "Queue", "Stack", "Tuple", "Task"
    };

    /// <summary>
    /// Schema registry to track and merge schema information across multiple instances of the same property
    /// </summary>
    private class SchemaRegistry
    {
        // Track property definitions by their path (e.g., "parent.child.grandchild")
        public Dictionary<string, List<SaveElement>> _propertyInstances = new();
        
        // Track objects (SaveObject) by their signature to find identical structures
        private Dictionary<string, HashSet<string>> _objectSignatures = new();
        
        /// <summary>
        /// Register a property instance with its path
        /// </summary>
        public void RegisterProperty(string path, SaveElement element)
        {
            if (!_propertyInstances.TryGetValue(path, out var instances))
            {
                instances = new List<SaveElement>();
                _propertyInstances[path] = instances;
            }
            
            // Only add if it's not null
            if (element != null)
            {
                instances.Add(element);
            }
        }
        
        /// <summary>
        /// Register an object signature at a given path
        /// </summary>
        public void RegisterObjectSignature(string path, string signature)
        {
            if (!_objectSignatures.TryGetValue(path, out var signatures))
            {
                signatures = new HashSet<string>();
                _objectSignatures[path] = signatures;
            }
            
            signatures.Add(signature);
        }
        
        /// <summary>
        /// Get all instances of a property at a given path
        /// </summary>
        public IEnumerable<SaveElement> GetPropertyInstances(string path)
        {
            if (_propertyInstances.TryGetValue(path, out var instances))
            {
                return instances;
            }
            
            return Enumerable.Empty<SaveElement>();
        }
        
        /// <summary>
        /// Check if a property has any non-empty instances
        /// </summary>
        public bool HasNonEmptyInstances(string path)
        {
            if (_propertyInstances.TryGetValue(path, out var instances))
            {
                return instances.Any(i => 
                    (i is SaveObject obj && obj.Properties.Count > 0) ||
                    (i is SaveArray arr && arr.Items.Count > 0));
            }
            
            return false;
        }
        
        /// <summary>
        /// Get all object signatures at a given path
        /// </summary>
        public IEnumerable<string> GetObjectSignatures(string path)
        {
            if (_objectSignatures.TryGetValue(path, out var signatures))
            {
                return signatures;
            }
            
            return Enumerable.Empty<string>();
        }
        
        /// <summary>
        /// Clear all registered schemas
        /// </summary>
        public void Clear()
        {
            _propertyInstances.Clear();
            _objectSignatures.Clear();
        }
        
        /// <summary>
        /// Get a summary of all registered properties - useful for debugging
        /// </summary>
        public string GetSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Schema Registry has {_propertyInstances.Count} registered paths:");
            
            foreach (var kvp in _propertyInstances.OrderBy(p => p.Key))
            {
                string instanceTypes = string.Join(", ", 
                    kvp.Value
                        .GroupBy(v => v.GetType().Name)
                        .Select(g => $"{g.Key}({g.Count()})"));
                        
                sb.AppendLine($"- {kvp.Key}: {kvp.Value.Count} instances [{instanceTypes}]");
                
                // Show more details for objects
                foreach (var instance in kvp.Value.OfType<SaveObject>().Take(3))
                {
                    sb.AppendLine($"  - Object properties: {string.Join(", ", instance.Properties.Select(p => p.Key).Take(5))}");
                    if (instance.Properties.Count > 5)
                        sb.AppendLine("    (more properties...)");
                }
            }
            
            return sb.ToString();
        }

        // In the SchemaRegistry class, add a method to get a flattened set of all paths to help with finding related paths
        public HashSet<string> GetAllPathsStartingWith(string pathPrefix)
        {
            HashSet<string> matchingPaths = new HashSet<string>();
            
            foreach (var path in _propertyInstances.Keys)
            {
                if (path.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    matchingPaths.Add(path);
                }
            }
            
            return matchingPaths;
        }
    }

    private SchemaRegistry _schemaRegistry = new();

    public IEnumerable<SaveObjectAnalysis> AnalyzeAdditionalFile(AdditionalText text, CancellationToken cancellationToken)
    {
        // Reset state for each file
        _definedClassesBySignature.Clear();
        _generatedTopLevelClassNames.Clear();
        _schemaRegistry.Clear(); // Clear the schema registry

        // Create analysis object first
        var currentAnalysis = new SaveObjectAnalysis(Path.GetFileNameWithoutExtension(text.Path));

        string fileContent = text.GetText(cancellationToken)?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(fileContent))
        {
            currentAnalysis.AddDiagnostic("PDXSA100", "Empty File", $"File has no content: {text.Path}", DiagnosticSeverity.Warning);
            return Enumerable.Empty<SaveObjectAnalysis>();
        }

        try
        {
            SaveObject? root = Parser.Parse(fileContent);
            root = (SaveObject) TransformNestedDictionaries(root);

            currentAnalysis.AddDiagnostic("PDXSA001", "Root Element Type", $"Parsed root element type for {currentAnalysis.RootName}: {root?.GetType().Name ?? "null"}", DiagnosticSeverity.Info);
            
            // FIRST PASS: Collect schema information across all objects
            if (root != null)
            {
                CollectSchemaInformation(root);
                currentAnalysis.AddDiagnostic("PDXSA200", "Schema Collection", $"Collected schema information for {_schemaRegistry._propertyInstances.Count} unique property paths", DiagnosticSeverity.Info);
                
                // Add detailed schema summary as a diagnostic to help debug schema issues
                string schemaSummary = _schemaRegistry.GetSummary();
                currentAnalysis.AddDiagnostic("PDXSA201", "Schema Details", schemaSummary, DiagnosticSeverity.Info);
            }

            string filePath = Path.GetFileNameWithoutExtension(text.Path).ToLowerInvariant();
            string rootClassNameBase = ToPascalCase(filePath);
            
            // Add explicit diagnostic about the detected root class name
            currentAnalysis.AddDiagnostic("PDXSA101", "Root Class Name", $"Using root class name: {rootClassNameBase} for file: {text.Path}", DiagnosticSeverity.Info);
            
            // Generate root name and explicitly reserve it first
            string rootClassName = ToPascalCase(rootClassNameBase);
            
            // Explicitly check for reserved names
            if (IsReservedTypeName(rootClassName))
            {
                rootClassName = "Pdx" + rootClassName;
            }
            
            // Add it to the top-level names set directly
            _generatedTopLevelClassNames.Add(rootClassName);
            
            // SECOND PASS: Now perform the actual analysis with schema awareness
            if (root != null)
            {
                // Root node analysis doesn't have a parent
                var rootClassDef = AnalyzeNode(root, rootClassName, null, isRootNode: true);
                
                // Add information about the root class definition
                if (rootClassDef != null)
                {
                    currentAnalysis.AddDiagnostic("PDXSA102", "Root Definition", $"Created root definition {rootClassDef.Name} with {rootClassDef.Properties.Count} properties", DiagnosticSeverity.Info);
                    
                    // Now add the root class definition by NAME
                    _definedClassesBySignature[rootClassName] = rootClassDef;
                }
                else
                {
                    currentAnalysis.AddDiagnostic("PDXSA103", "Root Definition Error", $"Failed to create root definition for {rootClassName}", DiagnosticSeverity.Error);
                }
            }
            else
            {
                currentAnalysis.AddDiagnostic("PDXSA104", "Null Root", $"Root element is null after parsing file: {text.Path}", DiagnosticSeverity.Error);
            }

            // Log the final class definition count
            var count = _definedClassesBySignature.Count;
            if (count == 0)
            {
                currentAnalysis.AddDiagnostic("PDXSA105", "No Definitions", $"No class definitions were created for file: {text.Path}", DiagnosticSeverity.Error);
            }

            // Assign the dictionary containing top-level definitions
            currentAnalysis.ClassDefinitions = _definedClassesBySignature
                .Where(kvp => kvp.Value != null && !kvp.Value.IsSimpleType)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            currentAnalysis.AddDiagnostic("PDXSA002", "Definitions Found", $"Analysis for {currentAnalysis.RootName} finished. Found {currentAnalysis.ClassDefinitions.Count} unique class definitions.", DiagnosticSeverity.Info);
        }
        catch (Exception ex)
        {
            currentAnalysis.AddDiagnostic("PDXSA003", "Analysis Exception", $"Analysis for {currentAnalysis.RootName} failed: {ex.GetType().Name} - {ex.Message}", DiagnosticSeverity.Error);
            currentAnalysis.SetError(ex);
        }

        // Return single successful analysis or analysis with error
        return new[] { currentAnalysis };
    }

    // Generates a unique signature for a SaveObject based on its properties (name and type)
    private string GenerateObjectSignature(SaveObject obj)
    {
        var propertySignatures = obj.Properties
            .Select(kvp => $"{kvp.Key}:{GetPropertyTypeSignature(kvp.Value)}")
            .OrderBy(s => s) // Order alphabetically for consistency
            .ToList();
        return $"{{{string.Join(";", propertySignatures)}}}";
    }

    // Helper for GenerateObjectSignature to get the type part of the signature
    private string GetPropertyTypeSignature(SaveElement element)
    {
        return element.Type switch
        {
            SaveType.Object => GenerateObjectSignature((SaveObject)element), // Recursive for nested objects
            SaveType.Array => GetArrayTypeSignature((SaveArray)element),
            SaveType.Int32 => "int",
            SaveType.Int64 => "long",
            SaveType.String => "string",
            SaveType.Identifier => "id", // Use 'id' for identifier type
            SaveType.Bool => "bool",
            SaveType.Float => "float",
            SaveType.Date => "date",
            SaveType.Guid => "guid",
            _ => "unknown"
        };
    }

    // Helper for array signatures
    private string GetArrayTypeSignature(SaveArray array)
    {
        if (!array.Items.Any()) return "list<empty>";
        // Assume homogeneity, use the signature of the first element
        return $"list<{GetPropertyTypeSignature(array.Items[0])}>";
    }

    // Helper to generate a unique PascalCase class name, handling collisions with numeric suffixes
    private string GenerateUniqueClassName(string baseName, ClassDefinition? parentDefinition)
    {
        string className = ToPascalCase(baseName);
        if (string.IsNullOrEmpty(className))
        {
            className = "UnnamedClass"; // Fallback for empty names
        }

        // Ensure the name is a valid C# identifier
        if (className.Length > 0 && !char.IsLetter(className[0]) && className[0] != '_')
        {
            className = "_" + className;
        }
        className = new string(className.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_').ToArray());
        // Handle completely invalid names after sanitization
        if (string.IsNullOrEmpty(className) || (className.Length > 0 && !char.IsLetter(className[0]) && className[0] != '_'))
        {
            className = "_InvalidName" + Guid.NewGuid().ToString("N").Substring(0, 4); // Ensure valid start
        }

        // Ensure we never use a C# keyword or built-in type name for a class
        if (IsReservedTypeName(className))
        {
            className = "Pdx" + className; // Prefix with "Pdx" to avoid conflicts
        }

        // Determine the correct set to check for collisions
        HashSet<string> nameScope = parentDefinition?.NestedClassNames ?? _generatedTopLevelClassNames;

        // Check for collisions using the relevant scope and append numbers if needed
        if (!nameScope.Contains(className))
        {
            return className;
        }

        // If collision, append number
        int count = 1;
        string uniqueName;
        do
        {
            count++;
            uniqueName = $"{className}{count}";
        } while (nameScope.Contains(uniqueName));

        return uniqueName;
    }

    // Helper to check if a name is a C# keyword or built-in type
    private static bool IsReservedTypeName(string name) => ReservedTypeNames.Contains(name);

    // Modify the first-pass schema collection method to handle nested properties in arrays
    private void CollectSchemaInformation(SaveObject root, string basePath = "")
    {
        // First ensure we register this object at this path
        _schemaRegistry.RegisterProperty(basePath, root);
        
        foreach (var property in root.Properties)
        {
            string propertyPath = string.IsNullOrEmpty(basePath) 
                ? property.Key 
                : $"{basePath}.{property.Key}";
                
            _schemaRegistry.RegisterProperty(propertyPath, property.Value);
            
            // If it's an object, register its signature and process recursively
            if (property.Value is SaveObject childObj)
            {
                string signature = GenerateObjectSignature(childObj);
                _schemaRegistry.RegisterObjectSignature(propertyPath, signature);
                
                // Special handling for empty objects to ensure they get registered properly
                if (childObj.Properties.Count == 0)
                {
                    // Even though it's empty, we need to ensure it gets registered
                    // so properties from other instances at same path can be found
                    _schemaRegistry.RegisterProperty(propertyPath, childObj);
                }
                
                // Recursively collect schema information
                CollectSchemaInformation(childObj, propertyPath);
            }
            // If it's an array, register each item and process nested objects
            else if (property.Value is SaveArray childArray)
            {
                // Register the array path
                string arrayPath = $"{propertyPath}";
                
                // If any item is a SaveObject, we need to analyze their structure too
                int index = 0;
                foreach (var item in childArray.Items)
                {
                    // Also register each item with its index
                    string itemPath = $"{propertyPath}[{index}]";
                    _schemaRegistry.RegisterProperty(itemPath, item);
                    
                    if (item is SaveObject arrayObj)
                    {
                        string signature = GenerateObjectSignature(arrayObj);
                        
                        // Register at both the array path level (for type tracking)
                        // and the individual item level (for schema merging)
                        _schemaRegistry.RegisterObjectSignature($"{propertyPath}[]", signature);
                        _schemaRegistry.RegisterObjectSignature(itemPath, signature);
                        
                        // Special handling for empty objects in arrays
                        if (arrayObj.Properties.Count == 0)
                        {
                            // Register an empty object so we can still find this path
                            _schemaRegistry.RegisterProperty(itemPath, arrayObj);
                        }
                        
                        // Recursively collect schema for this array object
                        CollectSchemaInformation(arrayObj, itemPath);
                        CollectSchemaInformation(arrayObj, $"{propertyPath}[]");
                    }
                    else if (item is SaveArray nestedArray)
                    {
                        // Handle nested arrays similarly
                        _schemaRegistry.RegisterProperty($"{propertyPath}[]", nestedArray);
                        
                        // Process each item in the nested array
                        ProcessNestedArray(nestedArray, $"{itemPath}");
                    }
                    
                    index++;
                }
            }
        }
    }

    // Fix SaveArray detection to handle possible null values safely
    private void ProcessNestedArray(SaveArray array, string basePath)
    {
        if (array == null)
            return;
        
        int index = 0;
        foreach (var item in array.Items)
        {
            string itemPath = $"{basePath}[{index}]";
            _schemaRegistry.RegisterProperty(itemPath, item);
            
            if (item is SaveObject arrayObj)
            {
                string signature = GenerateObjectSignature(arrayObj);
                _schemaRegistry.RegisterObjectSignature($"{basePath}[]", signature);
                _schemaRegistry.RegisterObjectSignature(itemPath, signature);
                
                // Recursively collect schema for this array object
                CollectSchemaInformation(arrayObj, itemPath);
                CollectSchemaInformation(arrayObj, $"{basePath}[]");
            }
            else if (item is SaveArray nestedArray)
            {
                // Recursively process nested arrays (with null check)
                ProcessNestedArray(nestedArray, itemPath);
            }
            
            index++;
        }
    }

    // Modify the AnalyzeNode method to examine all path-related schema instances for properties
    private ClassDefinition AnalyzeNode(SaveElement element, string preferredClassName, ClassDefinition? parentDefinition, bool isRootNode = false, string currentPath = "")
    {
        if (element is SaveObject saveObject)
        {
            // Check non-empty instances even if this one is empty, using a more aggressive path search
            bool hasNonEmptyInstances = false;
            
            // Try direct path first
            hasNonEmptyInstances = _schemaRegistry.HasNonEmptyInstances(currentPath);
            
            // If not found, check all related paths (starting with the same prefix)
            if (!hasNonEmptyInstances && !string.IsNullOrEmpty(currentPath))
            {
                // Get a base path by removing array indices and segments after last dot
                string basePath = currentPath;
                
                // Remove array indices
                if (basePath.Contains("["))
                {
                    basePath = string.Join("", basePath.Split('[').Select((part, i) => 
                        i == 0 ? part : (part.Contains("]") ? part.Substring(part.IndexOf("]") + 1) : "")));
                }
                
                // Get all paths that start with this base path
                var relatedPaths = _schemaRegistry.GetAllPathsStartingWith(basePath);
                
                // Check each related path for non-empty instances
                foreach (var path in relatedPaths)
                {
                    if (_schemaRegistry.HasNonEmptyInstances(path))
                    {
                        hasNonEmptyInstances = true;
                        break;
                    }
                }
            }
            
            // Even if this object is empty but has non-empty instances elsewhere, we should analyze it as a complex object
            bool isEmptyWithContent = saveObject.Properties.Count == 0 && hasNonEmptyInstances;
            
            // Check if this is a data dictionary
            bool isLikelyDataDictionary = !isEmptyWithContent && IsLikelyDataDictionary(saveObject);
            
            // Check if this is an indexed dictionary
            bool isIndexedDictionary = !isEmptyWithContent && saveObject.Properties.Any() && 
                saveObject.Properties.All(kvp => int.TryParse(kvp.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out _));
                
            if ((isIndexedDictionary || isLikelyDataDictionary) && !isEmptyWithContent)
            {
                // Handle as dictionary type - unchanged
                // ...
                // [Rest of the dictionary handling code remains the same]
                // This object represents a Dictionary<int, T> or Dictionary<string, T>, not a class.
                if (saveObject.Properties.Any())
                {
                    // Create property profile for all values
                    var valueProfile = new PropertyProfile("DictionaryValues");
                    
                    // Profile all values to determine the most appropriate type
                    foreach (var prop in saveObject.Properties)
                    {
                        valueProfile.AddValue(prop.Value);
                    }
                    
                    // Get a representative value for type analysis, but possibly modify if there are mixed types
                    var firstValue = saveObject.Properties.First().Value;
                    
                    // If there are mixed numeric types, prefer analyzing with float
                    if (valueProfile.HasMixedNumericTypes && firstValue is Scalar<int> intValue)
                    {
                        // Use a float scalar for analysis instead of the int
                        var floatValue = new Scalar<float>("value", (float)intValue.Value);
                        firstValue = floatValue;
                    }
                    
                    // Determine item base name
                    string itemBaseName = preferredClassName;
                    if (itemBaseName.EndsWith("s") && itemBaseName.Length > 1) 
                        itemBaseName = itemBaseName.Substring(0, itemBaseName.Length - 1);
                    else 
                        itemBaseName = $"{itemBaseName}Item";
                    
                    // Pass parent context along when analyzing the item type
                    var itemType = AnalyzeNode(firstValue, itemBaseName, parentDefinition, false, $"{currentPath}.item");
                    
                    // Override to a more permissive type if needed based on mixed types profile
                    if (valueProfile.HasMixedNumericTypes)
                    {
                        if (itemType.Name == "int" || itemType.Name == "long")
                        {
                            return GetOrCreateClassDefinitionForSimpleType("float", "float");
                        }
                    }
                    
                    return itemType;
                }
                else
                {
                    // Empty dictionary? Treat value type as object.
                    return GetOrCreateClassDefinitionForSimpleType("object", "object");
                }
            }

            // Regular Object - Signature & Property Analysis
            string signature = GenerateObjectSignature(saveObject);

            // New Class Definition
            string uniqueSimpleName; // Simple name (unique within scope)
            string calculatedFullName; // Concatenated full name

            if (isRootNode)
            {
                // Root node: Simple name is pre-reserved, FullName is the same.
                uniqueSimpleName = preferredClassName; // Already unique globally
                calculatedFullName = uniqueSimpleName;
                // Add to global scope tracking
                _generatedTopLevelClassNames.Add(uniqueSimpleName);
            }
            else
            {
                // Non-root: Generate a simple name unique within the parent's scope (or global if no parent)
                uniqueSimpleName = GenerateUniqueClassName(preferredClassName, parentDefinition);
                // Calculate FullName based on parent
                calculatedFullName = parentDefinition != null
                    ? parentDefinition.FullName + uniqueSimpleName // Append simple name to parent's full name
                    : uniqueSimpleName; // Top-level, non-root node

                // Add simple name to the correct scope tracking
                HashSet<string> nameScope = parentDefinition?.NestedClassNames ?? _generatedTopLevelClassNames;
                nameScope.Add(uniqueSimpleName);
            }

            var classDef = new ClassDefinition(uniqueSimpleName); // Create with SIMPLE name
            classDef.FullName = calculatedFullName; // Assign the calculated FULL name
            classDef.OriginalPath = currentPath; // Track the original path for schema merging

            // Add to parent's nested list if applicable
            if (parentDefinition != null)
            {
                parentDefinition.NestedClasses.Add(classDef);
            }
            else if (!isRootNode) // Add non-root, top-level classes to global signature dict
            {
                // Note: Using signature for potential reuse of identical top-level structures.
                if (_definedClassesBySignature.ContainsKey(signature)) {
                    return _definedClassesBySignature[signature]; // Reuse existing top-level def by signature
                }
                if (!classDef.IsSimpleType && !IsSimpleTypeName(classDef.Name))
                {
                    _definedClassesBySignature.Add(signature, classDef);
                }
            }

            // This set tracks property keys we've already processed
            HashSet<string> processedKeys = new HashSet<string>();
            
            // First process the actual properties in this object
            foreach (var kvp in saveObject.Properties)
            {
                processedKeys.Add(kvp.Key);
                AnalyzeProperty(kvp.Key, kvp.Value, classDef, $"{currentPath}.{kvp.Key}");
            }
            
            // Now check for additional properties from non-empty instances in the schema registry
            // We need to check several potential paths to ensure we get all related properties
            
            // 1. Check the direct path first
            if (_schemaRegistry.HasNonEmptyInstances(currentPath))
            {
                foreach (var instance in _schemaRegistry.GetPropertyInstances(currentPath))
                {
                    if (instance is SaveObject obj)
                    {
                        ProcessAdditionalProperties(obj, classDef, processedKeys, currentPath);
                    }
                }
            }
            
            // 2. Get all related paths and check each one
            if (!string.IsNullOrEmpty(currentPath))
            {
                // Build a base path to search for related paths
                string searchBase = currentPath;
                
                // Remove array indices
                if (searchBase.Contains("["))
                {
                    searchBase = string.Join("", searchBase.Split('[').Select((part, i) => 
                        i == 0 ? part : (part.Contains("]") ? part.Substring(part.IndexOf("]") + 1) : "")));
                }
                
                // Get all paths in the schema registry that start with this path
                var relatedPaths = _schemaRegistry.GetAllPathsStartingWith(searchBase);
                
                // Check each related path for properties
                foreach (var path in relatedPaths)
                {
                    foreach (var instance in _schemaRegistry.GetPropertyInstances(path))
                    {
                        if (instance is SaveObject obj)
                        {
                            // Use the found object to extract properties
                            ProcessAdditionalProperties(obj, classDef, processedKeys, currentPath);
                        }
                    }
                }
            }
            
            return classDef;
        }
        else if (element is SaveArray saveArray)
        {
            // Handle arrays
            // [Rest of the array handling code remains largely the same]
            if (saveArray.Items.Any())
            {
                // Create profile for all array items
                var arrayProfile = new PropertyProfile("ArrayItems");
                
                // Profile all array items to determine the most appropriate type
                foreach (var item in saveArray.Items)
                {
                    arrayProfile.AddValue(item);
                }
                
                // Get first item for analysis, but possibly modify for mixed types
                var firstItem = saveArray.Items.First();
                
                // If there are mixed numeric types, prefer analyzing with float
                if (arrayProfile.HasMixedNumericTypes && firstItem is Scalar<int> intValue)
                {
                    // Use a float scalar for analysis instead of the int
                    firstItem = new Scalar<float>("value", (float)intValue.Value);
                }
                
                // preferredClassName here is the base SIMPLE name intended for the item type
                string itemBaseName = preferredClassName;
                
                // Standard analysis with potentially modified first item
                var firstItemType = AnalyzeNode(firstItem, itemBaseName, parentDefinition, false, $"{currentPath}[]");
                
                // Override type if we found mixed numeric types
                if (arrayProfile.HasMixedNumericTypes && 
                    (firstItemType.Name == "int" || firstItemType.Name == "long"))
                {
                    return GetOrCreateClassDefinitionForSimpleType("float", "float");
                }
                
                return firstItemType;
            }
            else
            {
                // Check if we have any non-empty instances in the schema registry
                if (_schemaRegistry.HasNonEmptyInstances($"{currentPath}[]"))
                {
                    // Find the first non-empty instance
                    foreach (var instance in _schemaRegistry.GetPropertyInstances($"{currentPath}[]"))
                    {
                        if (instance is SaveArray nonEmptyArray && nonEmptyArray.Items.Any())
                        {
                            // Use this non-empty array for analysis instead
                            return AnalyzeNode(nonEmptyArray, preferredClassName, parentDefinition, false, currentPath);
                        }
                    }
                }
                
                // Also check related paths for array elements
                string arrayParentPath = currentPath;
                if (arrayParentPath.Contains("["))
                {
                    // Strip indices to create a base path
                    arrayParentPath = string.Join("", arrayParentPath.Split('[').Select((part, i) => 
                        i == 0 ? part : (part.Contains("]") ? part.Substring(part.IndexOf("]") + 1) : "")));
                    
                    // Check for instances at this base path that might be array containers
                    foreach (var instance in _schemaRegistry.GetPropertyInstances(arrayParentPath))
                    {
                        if (instance is SaveArray nonEmptyArray && nonEmptyArray.Items.Any())
                        {
                            // Use this non-empty array for analysis
                            return AnalyzeNode(nonEmptyArray, preferredClassName, parentDefinition, false, currentPath);
                        }
                    }
                }
                
                // Still empty after checking the registry, treat item type as object
                return GetOrCreateClassDefinitionForSimpleType("object", "object");
            }
        }
        else // Scalar types
        {
            // For scalars, return a placeholder definition representing the primitive type.
            string typeName = GetSimpleTypeName(element, preferredClassName);
            return GetOrCreateClassDefinitionForSimpleType(typeName, typeName);
        }
    }

    // Helper method to process additional properties from schema registry
    private void ProcessAdditionalProperties(SaveObject obj, ClassDefinition classDef, HashSet<string> processedKeys, string currentPath)
    {
        foreach (var kvp in obj.Properties)
        {
            // Skip properties we've already processed from this instance
            if (processedKeys.Contains(kvp.Key))
                continue;
                
            processedKeys.Add(kvp.Key);
            AnalyzeProperty(kvp.Key, kvp.Value, classDef, $"{currentPath}.{kvp.Key}");
        }
    }

    // Helper to get a simple C# type name string for a scalar SaveElement
    private string GetSimpleTypeName(SaveElement element, string propertyName = "")
    {
        // Get the basic type based on the element's Type enum
        string baseType = element.Type switch
        {
            SaveType.Int32 => "int",
            SaveType.Int64 => "long",
            SaveType.String => "string",
            SaveType.Identifier => "string", // Treat identifiers as strings in C#
            SaveType.Bool => "bool",
            SaveType.Float => "float",
            SaveType.Date => "DateTime", // Change from DateOnly to DateTime for .NET Standard 2.0 compatibility
            SaveType.Guid => "Guid",
            _ => "object" // Fallback
        };
        
        // We no longer hardcode property names since this should work for any game
        return baseType;
    }

    // Helper to get or create a placeholder ClassDefinition for simple types
    private ClassDefinition GetOrCreateClassDefinitionForSimpleType(string signature, string name)
    {
        // Create a transient definition that won't be used in global class list
        var simpleDef = new ClassDefinition(name); // Name is the C# type name (e.g., "int", "string")
        
        // Mark it as a simple type to prevent it from being added to class list
        simpleDef.IsSimpleType = true;
        
        return simpleDef;
    }

    // Helper to detect the specific PDX Dictionary pattern
    private bool IsPdxDictionaryPattern(SaveArray array, out string? detectedKeyType, out SaveElement? firstValueElement)
    {
        detectedKeyType = null;
        firstValueElement = null;

        if (array == null || !array.Items.Any())
            return false; // Not an array or empty

        // Pattern check: SaveArray<SaveArray<[ScalarID, DataObject]>>
        if (array.Items.All(item => item is SaveArray innerArray && innerArray.Items.Count == 2
                && (innerArray.Items[0] is Scalar<int> || innerArray.Items[0] is Scalar<long>)
                && innerArray.Items[1] is SaveObject))
        {
            // Get the first inner array to determine key/value structure
            var firstInnerArray = (SaveArray)array.Items.First();
            
            // Determine key type based on the first key's scalar type
            if (firstInnerArray.Items[0] is Scalar<int>)
                detectedKeyType = "int";
            else if (firstInnerArray.Items[0] is Scalar<long>)
                detectedKeyType = "long";
            else 
                return false; // Shouldn't happen due to the check above, but just in case
                
            // Set firstValueElement to the value part (the SaveObject)
            firstValueElement = firstInnerArray.Items[1];
            
            // Profile all properties across all objects to determine proper type inference
            Dictionary<string, PropertyProfile> propertyProfiles = new Dictionary<string, PropertyProfile>(StringComparer.OrdinalIgnoreCase);
            
            // Scan all items to build profiles for each property name
            foreach (var item in array.Items)
            {
                if (item is SaveArray innerArray && innerArray.Items.Count == 2 && 
                    innerArray.Items[1] is SaveObject valueObj)
                {
                    // Profile each property in this object
                    foreach (var prop in valueObj.Properties)
                    {
                        // Get or create profile for this property name
                        if (!propertyProfiles.TryGetValue(prop.Key, out var profile))
                        {
                            profile = new PropertyProfile(prop.Key);
                            propertyProfiles[prop.Key] = profile;
                        }
                        
                        // Update profile based on this property value's type
                        profile.AddValue(prop.Value);
                    }
                }
            }
            
            // Check if any property has mixed numeric types (int/float) and needs promotion
            bool hasMixedNumericProperties = propertyProfiles.Values.Any(p => p.HasMixedNumericTypes);
            
            if (hasMixedNumericProperties)
            {
                // Use a generic approach to promote types that need promotion
                if (firstValueElement is SaveObject firstObj)
                {
                    // Create a new set of properties for the first value element
                    var newProps = new List<KeyValuePair<string, SaveElement>>();
                    
                    foreach (var prop in firstObj.Properties)
                    {
                        if (propertyProfiles.TryGetValue(prop.Key, out var profile) && 
                            profile.HasMixedNumericTypes &&
                            profile.RecommendedType == "float" && 
                            prop.Value is Scalar<int> intValue)
                        {
                            // Convert int to float to ensure proper type inference
                            newProps.Add(new KeyValuePair<string, SaveElement>(
                                prop.Key, 
                                new Scalar<float>(prop.Key, (float)intValue.Value)));
                        }
                        else
                        {
                            newProps.Add(prop);
                        }
                    }
                    
                    // Replace the first value element with our modified version
                    firstValueElement = new SaveObject(newProps);
                }
            }
            
            return true;
        }

        // Original pattern check: SaveArray<SaveObject<SingleKeyValuePair>>
        if (!array.Items.All(item => item is SaveObject so && so.Properties.Count == 1))
            return false;

        // Analyze the first item to determine potential key and value types
        var firstItem = array.Items.First() as SaveObject;
        if (firstItem == null) return false; // Should not happen due to previous check

        var firstKvp = firstItem.Properties.First();
        var key = firstKvp.Key;
        var value = firstKvp.Value;

        // Value must be a SaveObject for this pattern
        if (!(value is SaveObject))
            return false;

        firstValueElement = value; // This is the element whose type we need to analyze

        // Determine key type (int, long, or string/identifier)
        if (int.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            detectedKeyType = "int";
        else if (long.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            detectedKeyType = "long";
        else if (!string.IsNullOrEmpty(key)) // Assume string/identifier otherwise
            detectedKeyType = "string";
        else
            return false; // Invalid key
        
        return true;
    }

    // Helper class to track property type profiles across multiple instances
    private class PropertyProfile
    {
        public string Name { get; }
        public bool HasFloatValues { get; private set; }
        public bool HasIntValues { get; private set; }
        public bool HasStringValues { get; private set; }
        public bool HasBoolValues { get; private set; }
        public bool HasDateValues { get; private set; }
        public bool HasObjectValues { get; private set; }
        public bool HasArrayValues { get; private set; }
        public bool HasOtherValues { get; private set; }
        
        public PropertyProfile(string name)
        {
            Name = name;
        }
        
        public void AddValue(SaveElement value)
        {
            if (value is Scalar<float>) HasFloatValues = true;
            else if (value is Scalar<int> || value is Scalar<long>) HasIntValues = true;
            else if (value is Scalar<string>) HasStringValues = true;
            else if (value is Scalar<bool>) HasBoolValues = true;
            else if (value is Scalar<DateTime>) HasDateValues = true;
            else if (value is SaveObject) HasObjectValues = true;
            else if (value is SaveArray) HasArrayValues = true;
            else HasOtherValues = true;
        }
        
        // True if property has both float and int values
        public bool HasMixedNumericTypes => HasFloatValues && HasIntValues;
        
        // Get the most appropriate type based on observed values
        public string RecommendedType
        {
            get
            {
                if (HasMixedNumericTypes) return "float"; // Promote to float if mixed
                if (HasFloatValues) return "float";
                if (HasIntValues) return "int";
                if (HasStringValues) return "string";
                if (HasBoolValues) return "bool";
                if (HasDateValues) return "DateTime";
                if (HasObjectValues) return "object";
                if (HasArrayValues) return "array";
                return "object"; // Default
            }
        }
    }

    // Update GetPropertyType to include path information
    private (string PropertyTypeSyntax, bool IsCollection, bool IsDictionary, bool IsPdxDictionaryPatternFlag, ClassDefinition? ValueTypeDef) 
        GetPropertyType(SaveElement value, string preferredNestedClassName, ClassDefinition parentDefinitionForNestedTypes, string currentPath = "")
    {
        bool isCollection = false;
        bool isDictionary = false;
        bool isPdxDictionaryPatternFlag = false;
        string baseTypeSyntax = "object";
        ClassDefinition? valueTypeDef = null;

        if (value is SaveObject obj)
        {
            // Check if this is a data dictionary with numeric keys first
            bool isIndexedDictionary = obj.Properties.Any() && obj.Properties.All(kvp => int.TryParse(kvp.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out _));
            
            // Also check if it's a data dictionary with string keys
            bool isStringDictionary = !isIndexedDictionary && IsLikelyDataDictionary(obj);
            
            if (isIndexedDictionary)
            {
                isDictionary = true;
                if (obj.Properties.Any())
                {
                    // Check ALL values for numeric type inference
                    bool hasFloatValues = false;
                    
                    foreach (var prop in obj.Properties)
                    {
                        if (prop.Value is SaveObject valueObj)
                        {
                            // For SaveObjects, check if they have numeric properties that are floats
                            foreach (var innerProp in valueObj.Properties)
                            {
                                if (innerProp.Value is Scalar<float>)
                                {
                                    hasFloatValues = true;
                                }
                            }
                        }
                        else if (prop.Value is Scalar<float>)
                        {
                            hasFloatValues = true;
                        }
                    }
                    
                    // Suggest simple name for dictionary value type
                    string itemBaseName = preferredNestedClassName + "Item";
                    // Pass parent context and path for dictionary value type analysis
                    var valueTypeResult = GetPropertyType(obj.Properties.First().Value, itemBaseName, parentDefinitionForNestedTypes, $"{currentPath}.item");
                    
                    // Override numeric types to float if we found any float values
                    string finalValueType = valueTypeResult.PropertyTypeSyntax;
                    if (hasFloatValues && (finalValueType == "int" || finalValueType == "long"))
                    {
                        finalValueType = "float";
                        valueTypeDef = GetOrCreateClassDefinitionForSimpleType("float", "float");
                    }
                    else
                    {
                        valueTypeDef = valueTypeResult.ValueTypeDef;
                    }
                    
                    // Use the FULL NAME of the value type in the dictionary signature
                    baseTypeSyntax = $"Dictionary<int, {finalValueType}>";
                }
                else
                {
                    baseTypeSyntax = "Dictionary<int, object>";
                    valueTypeDef = GetOrCreateClassDefinitionForSimpleType("object", "object");
                }
            }
            else if (isStringDictionary)
            {
                // Handle dictionary with string keys
                isDictionary = true;
                
                if (obj.Properties.Any())
                {
                    // Suggest simple name for dictionary value type
                    string itemBaseName = preferredNestedClassName + "Item";
                    // Pass parent context and path for dictionary value type analysis
                    var valueTypeResult = GetPropertyType(obj.Properties.First().Value, itemBaseName, parentDefinitionForNestedTypes, $"{currentPath}.item");
                    baseTypeSyntax = $"Dictionary<string, {valueTypeResult.PropertyTypeSyntax}>";
                    valueTypeDef = valueTypeResult.ValueTypeDef;
                }
                else
                {
                    baseTypeSyntax = "Dictionary<string, object>";
                    valueTypeDef = GetOrCreateClassDefinitionForSimpleType("object", "object");
                }
            }
            else
            {
                // Normal object - treat as its own class
                valueTypeDef = AnalyzeNode(obj, preferredNestedClassName, parentDefinitionForNestedTypes, false, currentPath);
                baseTypeSyntax = valueTypeDef.FullName;
            }
        }
        else if (value is SaveArray array)
        {
            // Check for PDX Dictionary Pattern
            if (IsPdxDictionaryPattern(array, out string? detectedKeyType, out SaveElement? firstValueElement) && detectedKeyType != null && firstValueElement != null)
            {
                isDictionary = true;
                isCollection = false;
                isPdxDictionaryPatternFlag = true;

                // Analyze the type of the value element with path
                string itemBaseName = preferredNestedClassName + "Item";
                valueTypeDef = AnalyzeNode(firstValueElement, itemBaseName, parentDefinitionForNestedTypes, false, $"{currentPath}.dictValue");

                if (valueTypeDef != null)
                {
                    baseTypeSyntax = $"Dictionary<{detectedKeyType}, {valueTypeDef.FullName}>";
                }
                else // Fallback if value type analysis fails
                {
                    baseTypeSyntax = $"Dictionary<{detectedKeyType}, object>";
                    valueTypeDef = GetOrCreateClassDefinitionForSimpleType("object", "object");
                }
            }
            else // Regular SaveArray (List<T>)
            {
                isCollection = true;
                
                // Check for an empty array that might have content in other instances
                if (!array.Items.Any() && _schemaRegistry.HasNonEmptyInstances(currentPath))
                {
                    // Try to find a non-empty array for analysis
                    SaveArray nonEmptyArray = null;
                    
                    foreach (var instance in _schemaRegistry.GetPropertyInstances(currentPath))
                    {
                        if (instance is SaveArray arr && arr.Items.Any())
                        {
                            nonEmptyArray = arr;
                            break;
                        }
                    }
                    
                    if (nonEmptyArray != null)
                    {
                        // Use the non-empty array for analysis
                        string itemBaseName = preferredNestedClassName + "Item";
                        var elementTypeResult = GetPropertyType(nonEmptyArray.Items.First(), itemBaseName, parentDefinitionForNestedTypes, $"{currentPath}[]");
                        baseTypeSyntax = $"List<{elementTypeResult.PropertyTypeSyntax}>";
                        valueTypeDef = elementTypeResult.ValueTypeDef;
                    }
                    else
                    {
                        // Still empty, default to object
                        baseTypeSyntax = "List<object>";
                        valueTypeDef = GetOrCreateClassDefinitionForSimpleType("object", "object");
                    }
                }
                else if (array.Items.Any())
                {
                    // Check for mixed numeric types in the array
                    bool hasFloatValues = false;
                    bool hasNumericValues = false;
                    
                    foreach (var item in array.Items)
                    {
                        if (item is Scalar<float>)
                        {
                            hasFloatValues = true;
                            hasNumericValues = true;
                        }
                        else if (item is Scalar<int> || item is Scalar<long>)
                        {
                            hasNumericValues = true;
                        }
                    }
                    
                    string itemBaseName = preferredNestedClassName + "Item";
                    var elementTypeResult = GetPropertyType(array.Items.First(), itemBaseName, parentDefinitionForNestedTypes, $"{currentPath}[]");
                    
                    // Promote to float if we have mixed numeric types
                    if (hasFloatValues && hasNumericValues && 
                        (elementTypeResult.PropertyTypeSyntax == "int" || elementTypeResult.PropertyTypeSyntax == "long"))
                    {
                        baseTypeSyntax = "List<float>";
                        valueTypeDef = GetOrCreateClassDefinitionForSimpleType("float", "float");
                    }
                    else
                    {
                        baseTypeSyntax = $"List<{elementTypeResult.PropertyTypeSyntax}>";
                        valueTypeDef = elementTypeResult.ValueTypeDef;
                    }
                }
                else
                {
                    baseTypeSyntax = "List<object>";
                    valueTypeDef = GetOrCreateClassDefinitionForSimpleType("object", "object");
                }
            }
        }
        else if (value != null)
        {
            baseTypeSyntax = GetSimpleTypeName(value, preferredNestedClassName);
            valueTypeDef = GetOrCreateClassDefinitionForSimpleType(baseTypeSyntax, baseTypeSyntax);
        }
        else
        {
            baseTypeSyntax = "object";
            valueTypeDef = GetOrCreateClassDefinitionForSimpleType("object", "object");
        }
        
        return (baseTypeSyntax, isCollection, isDictionary, isPdxDictionaryPatternFlag, valueTypeDef);
    }

    // PascalCase conversion helper
    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        // Replace separators with space, then use TextInfo for title casing
        name = name.Replace('_', ' ').Replace('-', ' ');
        name = TextInfo.ToTitleCase(name).Replace(" ", "");

        // Handle numeric start
        if (!string.IsNullOrEmpty(name) && char.IsDigit(name[0]))
            return $"N{name}"; // Prefix with 'N' if starts with digit

        // Ensure the result is not empty after replacements
        if (string.IsNullOrEmpty(name)) return "_"; // Return underscore if name becomes empty

        // Handle C# keywords
        if (IsReservedTypeName(name.ToLowerInvariant()))
        {
            return $"@{name}"; // Prefix with @ if it's a keyword
        }

        // Basic check for invalid characters (replace with underscore)
        var sb = new StringBuilder(name.Length);
        if (!char.IsLetter(name[0]) && name[0] != '_' && name[0] != '@') // Ensure start is valid
        {
            sb.Append('_');
        }
        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                sb.Append(c);
            }
            else if (sb.Length > 0 || char.IsLetter(c)) // Allow letters after initial invalid char replaced
            {
                sb.Append('_'); // Replace other invalid chars
            }
        }
        name = sb.ToString();

        // Final check if still empty or invalid
        if (string.IsNullOrEmpty(name)) return "_FallbackName";
        if (!char.IsLetter(name[0]) && name[0] != '_' && name[0] != '@') return $"_{name}";

        return name;
    }

    // Helper method to transform PDX-specific nested dictionary patterns
    private SaveObject DetectAndTransformNestedDictionary(SaveObject originalObject)
    {
        // Check if this object matches the PDX nested dictionary pattern:
        // {
        //   "0": <key value (usually a number)>,
        //   "1": { ... actual object properties ... }
        // }
        
        // Early exit if this doesn't look like our target pattern
        if (originalObject.Properties.Count == 0) 
            return originalObject;
        
        // Check if all properties have sequential numeric keys
        bool hasSequentialNumericKeys = true;
        foreach (var kvp in originalObject.Properties)
        {
            if (!int.TryParse(kvp.Key, out int index) || index < 0)
            {
                hasSequentialNumericKeys = false;
                break;
            }
        }
        
        if (!hasSequentialNumericKeys)
            return originalObject; // Not our pattern
            
        // This might be a dictionary of nested dictionary entries
        var transformedEntries = new List<KeyValuePair<string, SaveElement>>();
        
        // Group properties by int key / 2 to find entry pairs
        var groupedProps = originalObject.Properties
            .Where(p => int.TryParse(p.Key, out _))
            .Select(p => (int.Parse(p.Key), p.Value))
            .GroupBy(p => p.Item1 / 2);
            
        foreach (var group in groupedProps)
        {
            var entries = group.OrderBy(p => p.Item1).ToList();
            
            // Check if we have a pair (0: key, 1: value)
            if (entries.Count == 2)
            {
                var keyElement = entries[0].Value;
                var valueElement = entries[1].Value;
                
                // Ensure keyElement is a scalar (number/identifier/string) we can use as a key
                if (keyElement is Scalar<int> intKey)
                {
                    transformedEntries.Add(new KeyValuePair<string, SaveElement>(intKey.Value.ToString(), valueElement));
                }
                else if (keyElement is Scalar<long> longKey)
                {
                    transformedEntries.Add(new KeyValuePair<string, SaveElement>(longKey.Value.ToString(), valueElement));
                }
                else if (keyElement is Scalar<string> strKey)
                {
                    transformedEntries.Add(new KeyValuePair<string, SaveElement>(strKey.Value, valueElement));
                }                
            }
        }
        
        // If we transformed anything, return a new object with the transformed entries
        if (transformedEntries.Count > 0)
        {
            return new SaveObject(transformedEntries);
        }
        
        // Otherwise return the original
        return originalObject;
    }
    
    // Helper method to recursively transform any nested dictionaries
    private SaveElement TransformNestedDictionaries(SaveElement element)
    {
        if (element is SaveObject obj)
        {
            // First transform any nested objects in properties
            var transformedProperties = new List<KeyValuePair<string, SaveElement>>();
            foreach (var prop in obj.Properties)
            {
                transformedProperties.Add(new KeyValuePair<string, SaveElement>(prop.Key, TransformNestedDictionaries(prop.Value)));
            }
            
            // Create a new SaveObject with the transformed properties
            var transformedObj = new SaveObject(transformedProperties);
            
            // Then check if this object itself is a nested dictionary pattern
            return DetectAndTransformNestedDictionary(transformedObj);
        }
        else if (element is SaveArray arr)
        {
            // Transform any nested objects in array items
            var transformedItems = new List<SaveElement>();

            foreach (var item in arr.Items)
            {
                transformedItems.Add(TransformNestedDictionaries(item));
            }
            
            return new SaveArray(transformedItems);
        }
        
        // For scalar values, just return as is
        return element;
    }

    // Helper method to determine if an object is likely a data dictionary rather than a schema element
    private bool IsLikelyDataDictionary(SaveObject obj)
    {
        int numericKeys = 0;

        foreach (KeyValuePair<string, SaveElement> prop in obj.Properties)
        {
            if (int.TryParse(prop.Key, out _) || long.TryParse(prop.Key, out _))
                numericKeys++;
        }
        
        // Check for unusually large numeric keys (like 4294967295) which are clearly data values
        foreach (KeyValuePair<string, SaveElement> prop in obj.Properties)
        {
            if (long.TryParse(prop.Key, out long value) && (value > 1000000 || value < 0))
                return true;
        }
        
        return false;
    }

    // Helper method to check if a name is a simple type
    private bool IsSimpleTypeName(string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return false;
        
        // Check common simple type names using the shared ReservedTypeNames collection
        return ReservedTypeNames.Contains(typeName) && 
               (typeName == "string" || typeName == "int" || typeName == "long" || 
                typeName == "float" || typeName == "double" || typeName == "bool" || 
                typeName == "DateTime" || typeName == "Guid" || typeName == "object");
    }

    // Fix the AnalyzeProperty method to safely handle null objects in case they're encountered
    private void AnalyzeProperty(string key, SaveElement value, ClassDefinition classDef, string propertyPath)
    {
        if (value == null || classDef == null || string.IsNullOrEmpty(key))
            return;
        
        string originalName = key;
        string csPropertyName = ToPascalCase(originalName);
        
        // Check if we already have this property
        if (classDef.Properties.Any(p => p.OriginalName == originalName))
            return;
        
        // Handle the property based on value type
        if (value is SaveArray arrayValue)
        {
            // Check if this is a duplicate key pattern (not in this instance but across the schema)
            bool isDuplicateKeyPattern = _schemaRegistry.GetPropertyInstances(propertyPath).OfType<SaveArray>().Count() > 1;
            
            if (isDuplicateKeyPattern)
            {
                // This is a List<T> from duplicate keys
                string itemPreferredName = csPropertyName + "Item";
                
                // Create profile for all values with this key
                var valueProfile = new PropertyProfile(originalName);
                
                // Profile all values to determine the most appropriate type
                foreach (var instance in _schemaRegistry.GetPropertyInstances(propertyPath))
                {
                    if (instance is SaveArray arr)
                    {
                        foreach (var item in arr.Items)
                        {
                            valueProfile.AddValue(item);
                        }
                    }
                }
                
                // Get the value type from the first item if array is not empty
                SaveElement firstValueElement = null;
                
                if (arrayValue.Items.Any())
                {
                    firstValueElement = arrayValue.Items.First();
                }
                else
                {
                    // Try to find a non-empty array in the registry
                    foreach (var instance in _schemaRegistry.GetPropertyInstances(propertyPath))
                    {
                        if (instance is SaveArray arr && arr.Items.Any())
                        {
                            firstValueElement = arr.Items.First();
                            break;
                        }
                    }
                }
                
                // If no item found at all, default to object
                if (firstValueElement == null)
                {
                    classDef.Properties.Add(new PropertyDefinition(
                        name: csPropertyName,
                        propertyType: "List<object>",
                        originalName: originalName,
                        isCollection: true,
                        isDictionary: false,
                        representsDuplicateKeys: true,
                        isPdxDictionaryPattern: false
                    ));
                    return;
                }
                
                // If there are mixed numeric types, prefer analyzing with float
                if (valueProfile.HasMixedNumericTypes && firstValueElement is Scalar<int> intValue)
                {
                    // Use a float scalar for analysis instead of the int
                    firstValueElement = new Scalar<float>(originalName, (float)intValue.Value);
                }
                
                // Pass the CURRENT classDef as the parent for the item's type analysis
                var itemTypeResult = GetPropertyType(firstValueElement, itemPreferredName, classDef, propertyPath);
                string listPropertyTypeSyntax = $"List<{itemTypeResult.PropertyTypeSyntax}>";
                
                // Override type if we found mixed numeric types
                if (valueProfile.HasMixedNumericTypes && 
                    (itemTypeResult.PropertyTypeSyntax == "int" || itemTypeResult.PropertyTypeSyntax == "long"))
                {
                    listPropertyTypeSyntax = "List<float>";
                }

                classDef.Properties.Add(new PropertyDefinition(
                    name: csPropertyName,
                    propertyType: listPropertyTypeSyntax,
                    originalName: originalName,
                    isCollection: true,
                    isDictionary: false,
                    representsDuplicateKeys: true,
                    isPdxDictionaryPattern: false
                ));
            }
            else
            {
                // Regular array property
                string nestedPreferredName = csPropertyName;
                var propertyTypeResult = GetPropertyType(value, nestedPreferredName, classDef, propertyPath);
                
                classDef.Properties.Add(new PropertyDefinition(
                    name: csPropertyName,
                    propertyType: propertyTypeResult.PropertyTypeSyntax,
                    originalName: originalName,
                    isCollection: propertyTypeResult.IsCollection,
                    isDictionary: propertyTypeResult.IsDictionary,
                    representsDuplicateKeys: false,
                    isPdxDictionaryPattern: propertyTypeResult.IsPdxDictionaryPatternFlag
                ));
            }
        }
        else // SaveObject or scalar
        {
            string nestedPreferredName = csPropertyName;
            var propertyTypeResult = GetPropertyType(value, nestedPreferredName, classDef, propertyPath);
            
            classDef.Properties.Add(new PropertyDefinition(
                name: csPropertyName,
                propertyType: propertyTypeResult.PropertyTypeSyntax,
                originalName: originalName,
                isCollection: propertyTypeResult.IsCollection,
                isDictionary: propertyTypeResult.IsDictionary,
                representsDuplicateKeys: false,
                isPdxDictionaryPattern: propertyTypeResult.IsPdxDictionaryPatternFlag
            ));
        }
    }
}