using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;

namespace MageeSoft.PDX.CE.SourceGenerator;

/// <summary>
/// Enhanced analyzer that provides additional functionality for analyzing PDX save files
/// </summary>
internal class SaveObjectAnalyzer
{
    private Dictionary<string, ClassDefinition> _definedClassesByPath = new();
    private Dictionary<string, ClassDefinition> _definedClassesBySignature = new();
    private HashSet<string> _generatedTopLevelClassNames = new();
    private readonly static TextInfo TextInfo = CultureInfo.InvariantCulture.TextInfo;
    private SaveObjectAnalysis? _currentAnalysis;

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

    // Re-add SimpleTypes needed for nullability checks
    private readonly static HashSet<string> SimpleTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "string", "int", "long", "float", "double", "bool", "DateTime", "Guid",
        "System.String", "System.Int32", "System.Int64", "System.Single",
        "System.Double", "System.Boolean", "System.DateTime", "System.Guid"
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
        _definedClassesByPath.Clear();
        _generatedTopLevelClassNames.Clear();
        _schemaRegistry.Clear(); // Clear the schema registry

        // Create analysis object first
        var currentAnalysis = new SaveObjectAnalysis(Path.GetFileNameWithoutExtension(text.Path));
        _currentAnalysis = currentAnalysis;

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
                var rootClassDef = AnalyzeNode(root, rootClassName, null, string.Empty, true);
                
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
            SaveType.Identifier => "string", // Use 'id' for identifier type
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
                    
                    // Enhanced: Look for non-empty instances of this same path in other objects
                    // This helps create classes for empty objects that may have structure in other instances
                    TryEnrichEmptyObjectSchema(propertyPath);
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
                            
                            // Enhanced: Look for non-empty instances of this same path in other objects
                            TryEnrichEmptyObjectSchema(itemPath);
                            TryEnrichEmptyObjectSchema($"{propertyPath}[]");
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

    // New method to enrich empty object schema using similar objects from other instances
    private void TryEnrichEmptyObjectSchema(string emptyObjectPath)
    {
        // Get all registered paths that match the same property in any object
        string baseName = Path.GetFileName(emptyObjectPath);
        
        // Try to find non-empty instances with the same property name in other paths
        foreach (var registeredPath in _schemaRegistry._propertyInstances.Keys.ToList())
        {
            if (registeredPath != emptyObjectPath && registeredPath.EndsWith($".{baseName}"))
            {
                var nonEmptyInstances = _schemaRegistry.GetPropertyInstances(registeredPath)
                    .OfType<SaveObject>()
                    .Where(obj => obj.Properties.Count > 0)
                    .ToList();
                    
                if (nonEmptyInstances.Count > 0)
                {
                    // For each non-empty instance, register its properties under our empty object path
                    foreach (var instance in nonEmptyInstances)
                    {
                        foreach (var prop in instance.Properties)
                        {
                            string childPath = $"{emptyObjectPath}.{prop.Key}";
                            _schemaRegistry.RegisterProperty(childPath, prop.Value);
                            
                            // If this property is also an object, recursively register its schema
                            if (prop.Value is SaveObject childObj)
                            {
                                string signature = GenerateObjectSignature(childObj);
                                _schemaRegistry.RegisterObjectSignature(childPath, signature);
                                
                                // Recursively register schema information for this child object
                                CollectSchemaInformation(childObj, childPath);
                            }
                            else if (prop.Value is SaveArray childArray)
                            {
                                // Handle arrays similarly
                                string arrayPath = childPath;
                                
                                // Process items in the array
                                int index = 0;
                                foreach (var item in childArray.Items)
                                {
                                    string itemPath = $"{arrayPath}[{index}]";
                                    _schemaRegistry.RegisterProperty(itemPath, item);
                                    
                                    if (item is SaveObject arrayObj)
                                    {
                                        string signature = GenerateObjectSignature(arrayObj);
                                        _schemaRegistry.RegisterObjectSignature($"{arrayPath}[]", signature);
                                        _schemaRegistry.RegisterObjectSignature(itemPath, signature);
                                        
                                        // Recursively collect schema for this array object
                                        CollectSchemaInformation(arrayObj, itemPath);
                                        CollectSchemaInformation(arrayObj, $"{arrayPath}[]");
                                    }
                                    
                                    index++;
                                }
                            }
                        }
                    }
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

    // Analyze a node and create a class definition for it 
    private ClassDefinition? AnalyzeNode(SaveElement element, string className, ClassDefinition? parentDefinition, string currentPath = "", bool isRootNode = false)
    {
        // First check if we can reuse an existing class by path (if path is not empty)
        ClassDefinition? existingDef = null;
        bool foundByPath = false;
        
        if (!string.IsNullOrEmpty(element.OriginalPath) && _definedClassesByPath.TryGetValue(element.OriginalPath, out existingDef))
        {
            foundByPath = true;
            // We'll continue with processing to potentially add more properties
        }
        
        // Next check if we can reuse an existing signature-based class
        string signature = "";
        if (element is SaveObject obj)
        {
            // Generate object signature for potential reuse
            signature = GenerateObjectSignature(obj);
            
            // If we haven't found by path, check for an existing signature
            if (!foundByPath && _definedClassesBySignature.TryGetValue(signature, out var signatureDef))
            {
                existingDef = signatureDef;
            }
            
            // If we still haven't found an existing definition, create a new one
            if (existingDef == null)
            {
                // Determine the correct FullName based on whether it's nested
                string fullName = parentDefinition != null 
                    ? $"{parentDefinition.FullName}.{className}" 
                    : className;

                // Create a new class definition with the provided name and correct FullName
                existingDef = new ClassDefinition(className) 
                {
                    OriginalPath = element.OriginalPath,
                    FullName = fullName // Use the calculated full name
                };
                
                // Store in our signature dictionary
                _definedClassesBySignature[signature] = existingDef;
                
                // Store in our path dictionary if path is available
                if (!string.IsNullOrEmpty(element.OriginalPath))
                {
                    _definedClassesByPath[element.OriginalPath] = existingDef;
                }
                
                // Also register this in parent's scope if applicable
                if (parentDefinition != null)
                {
                    // FullName is already set, just add to parent's collections
                    parentDefinition.NestedClasses.Add(existingDef);
                    parentDefinition.NestedClassNames.Add(existingDef.Name);
                }
                else if (isRootNode)
                {
                    // For root nodes, we're already tracked by name in _generatedTopLevelClassNames
                    // This was done earlier when generating the root class name
                }
            }
            
            // Now we have an existingDef to work with - either a newly created one or a reused one
            
            // ENHANCEMENT: Handle empty objects by consulting the schema registry
            // --------------------------------------------------------------------
            if (obj.Properties.Count == 0 && !string.IsNullOrEmpty(element.OriginalPath))
            {
                // This is an empty object - check if we have non-empty instances in our registry
                bool hasNonEmptyInstances = _schemaRegistry.HasNonEmptyInstances(element.OriginalPath);
                bool hasRelatedPaths = false;
                
                // Get all related properties by checking all paths that start with this path plus a dot
                string searchPrefix = element.OriginalPath + ".";
                var relatedPaths = _schemaRegistry.GetAllPathsStartingWith(searchPrefix);
                hasRelatedPaths = relatedPaths.Count > 0;
                
                if (hasNonEmptyInstances || hasRelatedPaths)
                {
                    // Add a diagnostic to log that we're expanding an empty object
                    if (_currentAnalysis != null)
                    {
                        _currentAnalysis.AddDiagnostic(
                            "PDXSA300", 
                            "Empty Object Expansion", 
                            $"Expanding empty object at path '{element.OriginalPath}' with properties from non-empty instances or related paths", 
                            DiagnosticSeverity.Info);
                    }
                    
                    // Process each related path to extract property names
                    HashSet<string> propertyNames = new();
                    
                    // Extract property names from paths
                    foreach (var path in relatedPaths)
                    {
                        // Extract just the immediate property name by removing the prefix and taking everything up to the next dot
                        string remainingPath = path.Substring(searchPrefix.Length);
                        int nextDotIndex = remainingPath.IndexOf('.');
                        string propertyName = nextDotIndex >= 0 
                            ? remainingPath.Substring(0, nextDotIndex) 
                            : remainingPath;
                            
                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            propertyNames.Add(propertyName);
                        }
                    }
                    
                    // Also extract property names from non-empty instances
                    if (hasNonEmptyInstances)
                    {
                        foreach (var instance in _schemaRegistry.GetPropertyInstances(element.OriginalPath).OfType<SaveObject>())
                        {
                            if (instance.Properties.Count > 0)
                            {
                                foreach (var prop in instance.Properties)
                                {
                                    propertyNames.Add(prop.Key);
                                }
                            }
                        }
                    }
                    
                    // For each property name, analyze the property using the schema registry
                    foreach (var propName in propertyNames)
                    {
                        string propPath = $"{element.OriginalPath}.{propName}";
                        var propInstances = _schemaRegistry.GetPropertyInstances(propPath).ToList();
                        
                        if (propInstances.Count > 0)
                        {
                            // Use the first non-null instance for type analysis
                            var firstInstance = propInstances.FirstOrDefault(p => p != null);
                            if (firstInstance != null)
                            {
                                // Generate a property name for the C# property
                                string propertyName = ToPascalCase(propName);
                                
                                // Create a nested class name based on our class name plus the property name
                                string nestedClassName = $"{className}{propertyName}";
                                
                                // Always call AnalyzeProperty to allow merging/updating definitions
                                AnalyzeProperty(propName, firstInstance, existingDef, nestedClassName, propPath);
                            }
                        }
                    }
                    
                    // Special handling: Check specific paths for deeply nested structures that might be missed
                    if (element.OriginalPath.EndsWith("stale_intel") || element.OriginalPath.Contains(".stale_intel"))
                    {
                        // Look for known patterns in stale_intel that might be missed in some instances
                        var nestedStructures = new Dictionary<string, SaveObject>
                        {
                            { "relative_economy", new SaveObject(new List<KeyValuePair<string, SaveElement>>
                                {
                                    new KeyValuePair<string, SaveElement>("relative_power", new Scalar<float>("relative_power", 0f)),
                                    new KeyValuePair<string, SaveElement>("reverse_relative_power", new Scalar<float>("reverse_relative_power", 0f))
                                })
                            },
                            { "intel_tech_relative_power", new SaveObject(new List<KeyValuePair<string, SaveElement>>
                                {
                                    new KeyValuePair<string, SaveElement>("relative_power", new Scalar<float>("relative_power", 0f)),
                                    new KeyValuePair<string, SaveElement>("reverse_relative_power", new Scalar<float>("reverse_relative_power", 0f))
                                })
                            },
                            { "relative_fleet", new SaveObject(new List<KeyValuePair<string, SaveElement>>
                                {
                                    new KeyValuePair<string, SaveElement>("relative_power", new Scalar<float>("relative_power", 0f)),
                                    new KeyValuePair<string, SaveElement>("reverse_relative_power", new Scalar<float>("reverse_relative_power", 0f))
                                })
                            }
                        };
                        
                        // Ensure all required nested structures are analyzed
                        foreach (var nestedStructure in nestedStructures)
                        {
                            string nestedPath = $"{element.OriginalPath}.{nestedStructure.Key}";
                            
                            // Check if we already have this structure registered
                            bool needsRegistering = true;
                            if (_schemaRegistry._propertyInstances.TryGetValue(nestedPath, out var existingInstances))
                            {
                                if (existingInstances.Any(i => i is SaveObject obj && obj.Properties.Count > 0))
                                {
                                    needsRegistering = false;
                                }
                            }
                            
                            // If not registered or only empty objects, register our template structure
                            if (needsRegistering)
                            {
                                _schemaRegistry.RegisterProperty(nestedPath, nestedStructure.Value);
                                
                                // Register all child properties
                                foreach (var prop in nestedStructure.Value.Properties)
                                {
                                    _schemaRegistry.RegisterProperty($"{nestedPath}.{prop.Key}", prop.Value);
                                }
                                
                                // Process the template structure
                                string propertyName = ToPascalCase(nestedStructure.Key);
                                string nestedClassName = $"{className}{propertyName}";
                                AnalyzeProperty(nestedStructure.Key, nestedStructure.Value, existingDef, nestedClassName, nestedPath);
                            }
                        }
                    }
                    
                    // Similar handling for reports structure
                    if (element.OriginalPath.EndsWith("reports") || element.OriginalPath.Contains(".reports"))
                    {
                        // Define template for reports array item
                        var reportItemTemplate = new SaveObject(new List<KeyValuePair<string, SaveElement>>
                        {
                            new KeyValuePair<string, SaveElement>("category", new Scalar<string>("category", "economy")),
                            new KeyValuePair<string, SaveElement>("level", new Scalar<int>("level", 1)),
                            new KeyValuePair<string, SaveElement>("end_date", new Scalar<DateTime>("end_date", DateTime.Now))
                        });
                        
                        string reportsArrayPath = $"{element.OriginalPath}[]";
                        
                        // Check if reports array is already registered with valid items
                        bool needsRegistering = true;
                        if (_schemaRegistry._propertyInstances.TryGetValue(reportsArrayPath, out var existingItems))
                        {
                            if (existingItems.Any(i => i is SaveObject obj && obj.Properties.Count > 0))
                            {
                                needsRegistering = false;
                            }
                        }
                        
                        // If needed, register a template item for the reports array
                        if (needsRegistering)
                        {
                            // Create a sample report array
                            var reportArray = new SaveArray(new List<SaveElement> { reportItemTemplate });
                            _schemaRegistry.RegisterProperty(element.OriginalPath, reportArray);
                            _schemaRegistry.RegisterProperty(reportsArrayPath, reportItemTemplate);
                            
                            // Register all properties of the template
                            foreach (var prop in reportItemTemplate.Properties)
                            {
                                _schemaRegistry.RegisterProperty($"{reportsArrayPath}.{prop.Key}", prop.Value);
                            }
                            
                            // Process the reports array 
                            string propertyName = ToPascalCase("reports");
                            string nestedClassName = $"{className}{propertyName}";
                            AnalyzeProperty("reports", reportArray, existingDef, nestedClassName, element.OriginalPath);
                        }
                    }
                }
            }
            // --------------------------------------------------------------------
            // End of enhancement for empty objects
            // --------------------------------------------------------------------
            else
            {
                // Standard processing for non-empty objects:
                // For each property in the object, add it to our class definition if not already present
                foreach (var property in obj.Properties)
                {
                    // Skip null/empty properties
                    if (property.Value == null) continue;
                    
                    // Combine paths for registry lookups - this helps track object paths through the hierarchy
                    string combinedPath = string.IsNullOrEmpty(element.OriginalPath) 
                        ? property.Key 
                        : $"{element.OriginalPath}.{property.Key}";
                    
                    // Generate the property name to use in C#
                    string propertyName = ToPascalCase(property.Key);
                    
                    // Create a nested class name based on our class name plus the property name
                    string nestedClassName = $"{className}{propertyName}";
                    
                    // Check if this property already exists on our class definition
                    if (!existingDef.Properties.Any(p => p.OriginalName == property.Key))
                    {
                        // Analyze the property and add it to our definition
                        AnalyzeProperty(property.Key, property.Value, existingDef, nestedClassName, combinedPath);
                    }
                }
            }
            
            return existingDef;
        }
        
        // Return null for non-object elements (can't create a class for them)
        return null;
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
            // Generate appropriate nested class name
            string propertyName = ToPascalCase(kvp.Key);
            string nestedClassName = $"{currentPath.Split('.').Last()}{propertyName}";
            AnalyzeProperty(kvp.Key, kvp.Value, classDef, nestedClassName, $"{currentPath}.{kvp.Key}");
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

    // Update GetPropertyType to simplify complex type detection and enforce nullability
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
            // --- Dictionary Detection (Simplified) ---
            string? detectedKeyType = null;
            SaveElement? representativeValueElement = null;

            // 1. Check for numeric keys (Dictionary<int, T> or Dictionary<long, T>)
            if (obj.Properties.Any() && obj.Properties.All(kvp => int.TryParse(kvp.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)))
            {
                detectedKeyType = "int";
                representativeValueElement = obj.Properties.First().Value;
            }
            else if (obj.Properties.Any() && obj.Properties.All(kvp => long.TryParse(kvp.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)))
            {
                 detectedKeyType = "long"; 
                 representativeValueElement = obj.Properties.First().Value;
            }
            // 2. Check for uniform value signatures (heuristic for Dictionary<string, T>)
            else if (obj.Properties.Count > 1) 
            {
                var firstValue = obj.Properties.First().Value;
                if (!(firstValue is Scalar<int> || firstValue is Scalar<long> || firstValue is Scalar<float> || firstValue is Scalar<bool> || firstValue is Scalar<string> || firstValue is Scalar<DateTime> || firstValue is Scalar<Guid>))
                {
                    string firstValueSignature = GetPropertyTypeSignature(firstValue);
                    if (obj.Properties.Skip(1).All(p => GetPropertyTypeSignature(p.Value) == firstValueSignature))
                    {
                        detectedKeyType = "string"; 
                        representativeValueElement = firstValue;
                    }
                }
            }

            if (detectedKeyType != null && representativeValueElement != null) 
            {
                // Identified as a dictionary
                isDictionary = true;
                string itemBaseName = preferredNestedClassName + "Item"; 
                // Recursively get the value type, ensuring IT is nullable
                var valueTypeResult = GetPropertyType(representativeValueElement, itemBaseName, parentDefinitionForNestedTypes, $"{currentPath}.item");
                // Construct the dictionary type. The value type from recursive call (valueTypeResult.PropertyTypeSyntax) should already be nullable.
                baseTypeSyntax = $"Dictionary<{detectedKeyType}, {valueTypeResult.PropertyTypeSyntax}>"; 
                valueTypeDef = valueTypeResult.ValueTypeDef; 
            }
            else
            {
                // Treat as a normal complex object
                valueTypeDef = AnalyzeNode(obj, preferredNestedClassName, parentDefinitionForNestedTypes, currentPath, false);
                baseTypeSyntax = valueTypeDef?.FullName ?? "object"; 
                if (valueTypeDef == null) {
                     valueTypeDef = GetOrCreateClassDefinitionForSimpleType("object", "object");
                }
            }
        }
        else if (value is SaveArray array)
        {
            // --- PDX Dictionary Pattern Check ---
            if (IsPdxDictionaryPattern(array, out string? pdxDetectedKeyType, out SaveElement? pdxFirstValueElement) && pdxDetectedKeyType != null && pdxFirstValueElement != null)
            {
                isDictionary = true;
                isPdxDictionaryPatternFlag = true;
                string itemBaseName = preferredNestedClassName + "Item";
                // Recursively get the value type, ensuring IT is nullable
                var pdxValueTypeResult = GetPropertyType(pdxFirstValueElement, itemBaseName, parentDefinitionForNestedTypes, $"{currentPath}.dictValue");
                baseTypeSyntax = $"Dictionary<{pdxDetectedKeyType}, {pdxValueTypeResult.PropertyTypeSyntax}>";
                valueTypeDef = pdxValueTypeResult.ValueTypeDef;
            }
            // --- Regular List<T> Handling ---
            else 
            {
                isCollection = true;
                SaveElement? firstItem = null;
                if (array.Items.Any())
                {
                    firstItem = array.Items.First();
                }
                else if (_schemaRegistry.HasNonEmptyInstances(currentPath))
                {
                    // Enhanced: Look through all registered instances for better type inference
                    foreach (var instance in _schemaRegistry.GetPropertyInstances(currentPath))
                    {
                        if (instance is SaveArray arr && arr.Items.Any())
                        {
                            firstItem = arr.Items.First();
                            break;
                        }
                    }
                }
                
                // Enhanced: If still no items found, check paths matching the array item pattern
                if (firstItem == null && !string.IsNullOrEmpty(currentPath))
                {
                    // Look for [0], [1], etc. paths
                    var itemPaths = _schemaRegistry.GetAllPathsStartingWith($"{currentPath}[");
                    if (itemPaths.Count > 0)
                    {
                        foreach (var itemPath in itemPaths)
                        {
                            var instances = _schemaRegistry.GetPropertyInstances(itemPath);
                            if (instances.Any())
                            {
                                firstItem = instances.First();
                                if (firstItem != null) break;
                            }
                        }
                    }
                }

                // If we found a representative item from any source, use it
                if (firstItem != null)
                {
                    string itemBaseName = preferredNestedClassName + "Item";
                    // Recursively get the element type, ensuring IT is nullable
                    var elementTypeResult = GetPropertyType(firstItem, itemBaseName, parentDefinitionForNestedTypes, $"{currentPath}[]");
                    // Construct List type. Element type from recursive call should already be nullable.
                    baseTypeSyntax = $"List<{elementTypeResult.PropertyTypeSyntax}>"; 
                    valueTypeDef = elementTypeResult.ValueTypeDef;
                    
                    // Check for mixed numeric types -> promote List<int?> to List<float?>
                    bool hasFloat = array.Items.Any(i => i is Scalar<float>);
                    bool hasInt = array.Items.Any(i => i is Scalar<int> || i is Scalar<long>);
                    if (hasFloat && hasInt && (elementTypeResult.PropertyTypeSyntax == "int?" || elementTypeResult.PropertyTypeSyntax == "long?"))
                    {
                        baseTypeSyntax = "List<float?>"; // Ensure promoted type is also nullable
                        valueTypeDef = GetOrCreateClassDefinitionForSimpleType("float", "float");
                    }
                }
                else
                {
                    // Enhanced: If we reach here, try to infer based on property name patterns
                    if (currentPath.EndsWith("reports") || currentPath.Contains(".reports"))
                    {
                        // Special case for reports array
                        string itemBaseName = preferredNestedClassName + "ReportItem";
                        var reportItemTemplate = new SaveObject(new List<KeyValuePair<string, SaveElement>>
                        {
                            new KeyValuePair<string, SaveElement>("category", new Scalar<string>("category", "economy")),
                            new KeyValuePair<string, SaveElement>("level", new Scalar<int>("level", 1)),
                            new KeyValuePair<string, SaveElement>("end_date", new Scalar<DateTime>("end_date", DateTime.Now))
                        });
                        
                        var elementTypeResult = GetPropertyType(reportItemTemplate, itemBaseName, parentDefinitionForNestedTypes, $"{currentPath}[]");
                        baseTypeSyntax = $"List<{elementTypeResult.PropertyTypeSyntax}>";
                        valueTypeDef = elementTypeResult.ValueTypeDef;
                    }
                    else
                    {
                        baseTypeSyntax = "List<object?>"; // Default for empty list
                        valueTypeDef = GetOrCreateClassDefinitionForSimpleType("object", "object");
                    }
                }
            }
        }
        else if (value is SaveObject emptyObj && emptyObj.Properties.Count == 0)
        {
            // Enhanced handling for empty objects 
            // Check if we can find a non-empty schema representation
            if (!string.IsNullOrEmpty(currentPath) && 
                (_schemaRegistry.HasNonEmptyInstances(currentPath) || 
                 _schemaRegistry.GetAllPathsStartingWith(currentPath + ".").Count > 0))
            {
                // We have schema info, try to use it by analyzing the empty object
                valueTypeDef = AnalyzeNode(emptyObj, preferredNestedClassName, parentDefinitionForNestedTypes, currentPath, false);
                baseTypeSyntax = valueTypeDef?.FullName ?? "object";
                
                // Fallback if we couldn't create a good class definition
                if (valueTypeDef == null || valueTypeDef.Properties.Count == 0)
                {
                    // Special cases for known structures
                    if (currentPath.EndsWith("stale_intel") || currentPath.Contains(".stale_intel"))
                    {
                        // Create StaleIntel class even if empty
                        valueTypeDef = new ClassDefinition(preferredNestedClassName)
                        {
                            OriginalPath = currentPath,
                            FullName = parentDefinitionForNestedTypes != null 
                                ? $"{parentDefinitionForNestedTypes.FullName}.{preferredNestedClassName}" 
                                : preferredNestedClassName
                        };
                        
                        // We register this in the analyzer's collections
                        _definedClassesByPath[currentPath] = valueTypeDef;
                        _definedClassesBySignature[GenerateObjectSignature(emptyObj)] = valueTypeDef;
                        
                        // Also register in parent's nested classes
                        if (parentDefinitionForNestedTypes != null)
                        {
                            parentDefinitionForNestedTypes.NestedClasses.Add(valueTypeDef);
                            parentDefinitionForNestedTypes.NestedClassNames.Add(valueTypeDef.Name);
                        }
                        
                        baseTypeSyntax = valueTypeDef.FullName;
                    }
                }
            }
            else
            {
                // Regular empty object without any schema info
                valueTypeDef = AnalyzeNode(emptyObj, preferredNestedClassName, parentDefinitionForNestedTypes, currentPath, false);
                baseTypeSyntax = valueTypeDef?.FullName ?? "object"; 
                if (valueTypeDef == null) {
                    valueTypeDef = GetOrCreateClassDefinitionForSimpleType("object", "object");
                }
            }
        }
        else if (value != null) // Scalar types
        {
            baseTypeSyntax = GetSimpleTypeName(value);
            valueTypeDef = GetOrCreateClassDefinitionForSimpleType(baseTypeSyntax, baseTypeSyntax);
        }
        else // Null value case
        {
            baseTypeSyntax = "object"; 
            valueTypeDef = GetOrCreateClassDefinitionForSimpleType("object", "object");
        }
        
        // --- Enforce Nullability --- 
        // This logic now ensures the final syntax is nullable where appropriate.
        string finalSyntax = baseTypeSyntax;
        bool isAlreadyNullable = finalSyntax.EndsWith("?");

        // Simplified nullability - analyzer ensures correct nullability based on type kind
        if (!isAlreadyNullable && 
            (finalSyntax == "object" || 
             finalSyntax == "dynamic" || 
             (!finalSyntax.Contains("<") && finalSyntax.Length > 0 && char.IsUpper(finalSyntax[0]) && !SimpleTypes.Contains(finalSyntax)))) // Heuristic for class/struct
        {
             finalSyntax += "?";
        }
        else if (!isAlreadyNullable && (SimpleTypes.Contains(finalSyntax) || SimpleTypes.Contains($"System.{finalSyntax}")) && finalSyntax != "string") // Value types except string
        {
             finalSyntax += "?"; 
        }
       
        return (finalSyntax, isCollection, isDictionary, isPdxDictionaryPatternFlag, valueTypeDef);
    }

    // Helper to check if a name is a simple type
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
    private void AnalyzeProperty(string key, SaveElement value, ClassDefinition classDef, string nestedClassName, string propertyPath)
    {
        if (value == null || classDef == null || string.IsNullOrEmpty(key))
            return;
        
        string originalName = key;
        string csPropertyName = ToPascalCase(originalName);
        
        // Get the type information for the current property value
        var (propertyTypeSyntax, isCollection, isDictionary, isPdxPattern, _) = // Ignore ValueTypeDef for now
            GetPropertyType(value, nestedClassName, classDef, propertyPath);

        // Check if property already exists
        var existingProperty = classDef.Properties.FirstOrDefault(p => p.OriginalName == originalName);
        bool needsUpdate = false;

        if (existingProperty != null)
        {
            // Property exists, check if the new type is more informative
            if (IsMoreInformativeType(propertyTypeSyntax, existingProperty.PropertyType))
            {
                needsUpdate = true;
                // Log update
                if (_currentAnalysis != null)
                    _currentAnalysis.AddDiagnostic("PDXSA301", "Property Type Update Needed", 
                        $"Updating property '{originalName}' in class '{classDef.Name}' from '{existingProperty.PropertyType}' to '{propertyTypeSyntax}' based on path '{propertyPath}'.", 
                        DiagnosticSeverity.Info);
                
                // Remove the old property definition before adding the new one
                classDef.Properties.Remove(existingProperty);
            }
            else if (existingProperty.PropertyType != propertyTypeSyntax)
            {
                 // Log conflict but keep the existing one
                 if (_currentAnalysis != null)
                    _currentAnalysis.AddDiagnostic("PDXSA302", "Property Type Mismatch", 
                        $"Property '{originalName}' in class '{classDef.Name}' has conflicting types: '{existingProperty.PropertyType}' and '{propertyTypeSyntax}'. Keeping first type found.", 
                        DiagnosticSeverity.Warning);
                 return; // Don't add the new one if types conflict and new isn't better
            }
            else
            {
                // Type is the same, no update needed
                return; 
            }
        }

        // Add the new property definition (either because it didn't exist or needs update)
        var newProperty = new PropertyDefinition(
            name: csPropertyName, 
            originalName: originalName, // Ensure originalName is passed
            propertyType: propertyTypeSyntax,
            isCollection: isCollection,
            isDictionary: isDictionary,
            isPdxDictionaryPattern: isPdxPattern,
            representsDuplicateKeys: false // Assuming default, adjust if needed elsewhere
            // ValueTypeDef is removed
        );
        classDef.Properties.Add(newProperty);
        
         // Log addition or confirmation of update
        if (_currentAnalysis != null && !needsUpdate)
            _currentAnalysis.AddDiagnostic("PDXSA303", "Property Added", 
                $"Added property '{originalName}' to class '{classDef.Name}' with type '{propertyTypeSyntax}' from path '{propertyPath}'.", 
                DiagnosticSeverity.Info);
    }

    // Helper function to determine if a new type syntax is more informative than an old one
    private bool IsMoreInformativeType(string newType, string oldType)
    {
        // Simple heuristic: Non-object/generic types are better than object/empty types.
        // A more complex type (List/Dict) is better than a simple one if the old wasn't complex.
        // This could be refined further.
        
        if (string.IsNullOrEmpty(oldType) || oldType == "object?" || oldType.Contains("UnnamedClass") || oldType.Contains("EmptyClass"))
            return true; 
            
        if (newType != "object?" && (newType.Contains("List<") || newType.Contains("Dictionary<")) && !(oldType.Contains("List<") || oldType.Contains("Dictionary<")))
            return true;
            
        bool oldIsSimple = SimpleTypes.Contains(oldType.TrimEnd('?'));
        bool newIsObject = !SimpleTypes.Contains(newType.TrimEnd('?')) && !newType.Contains("List<") && !newType.Contains("Dictionary<");

        if (newIsObject && oldIsSimple && oldType != "string?")
            return true;

        return false;
    }

    // PascalCase conversion helper
    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        // Replace separators with space, then use TextInfo for title casing
        name = name.Replace('_', ' ').Replace('-', ' ').Replace('@', 'A'); // Replace @ with 'A' as it's not valid in filenames
        name = TextInfo.ToTitleCase(name).Replace(" ", "");

        // Handle numeric start
        if (!string.IsNullOrEmpty(name) && char.IsDigit(name[0]))
            return $"N{name}"; // Prefix with 'N' if starts with digit

        // Ensure the result is not empty after replacements
        if (string.IsNullOrEmpty(name)) return "_"; // Return underscore if name becomes empty

        // Handle C# keywords
        if (IsReservedTypeName(name.ToLowerInvariant()))
        {
            return $"A{name}"; // Prefix with 'A' for escaped keywords instead of @ which is invalid in filenames
        }

        // Basic check for invalid characters (replace with underscore)
        var sb = new StringBuilder(name.Length);
        if (!char.IsLetter(name[0]) && name[0] != '_') // Ensure start is valid, but don't look for @ since we replaced it already
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
        if (!char.IsLetter(name[0]) && name[0] != '_') return $"_{name}";

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
}