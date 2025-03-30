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
    
    // Dictionary to keep track of generated TOP-LEVEL class names to avoid collisions
    private HashSet<string> _generatedTopLevelClassNames = new(); // Renamed for clarity
    private readonly static TextInfo TextInfo = CultureInfo.InvariantCulture.TextInfo; // Needed for ToPascalCase

    public IEnumerable<SaveObjectAnalysis> AnalyzeAdditionalFile(AdditionalText text, CancellationToken cancellationToken)
    {
        // Log the file path being analyzed
        System.Diagnostics.Debug.WriteLine($"ANALYZER: Processing file: {text.Path}");

        // Reset state for each file
        _definedClassesBySignature.Clear();
        _generatedTopLevelClassNames.Clear(); // Use the renamed set

        // Create analysis object first
        var currentAnalysis = new SaveObjectAnalysis(Path.GetFileNameWithoutExtension(text.Path));

        string fileContent = text.GetText(cancellationToken)?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(fileContent))
        {
            // Return empty list if no content, this is not an error
            System.Diagnostics.Debug.WriteLine($"ANALYZER: Empty content for file: {text.Path}");
            currentAnalysis.AddDiagnostic("PDXSA100", "Empty File", $"File has no content: {text.Path}", DiagnosticSeverity.Warning);
            return Enumerable.Empty<SaveObjectAnalysis>();
        }

        try
        {
            SaveObject? root = Parser.Parse(fileContent);
            root = (SaveObject) TransformNestedDictionaries(root);

            currentAnalysis.AddDiagnostic("PDXSA001", "Root Element Type", $"Parsed root element type for {currentAnalysis.RootName}: {root?.GetType().Name ?? "null"}", DiagnosticSeverity.Info);
            
            string filePath = Path.GetFileNameWithoutExtension(text.Path).ToLowerInvariant();
            string rootClassNameBase = ToPascalCase(filePath);
            
            System.Diagnostics.Debug.WriteLine($"ANALYZER: File path (lowercase): {filePath}");
            
            // Add explicit diagnostic about the detected root class name
            currentAnalysis.AddDiagnostic("PDXSA101", "Root Class Name", $"Using root class name: {rootClassNameBase} for file: {text.Path}", DiagnosticSeverity.Info);
            
            // IMPORTANT: Generate root name and EXPLICITLY RESERVE IT FIRST before any analysis
            // This prevents any subsequent elements inside the file from getting the primary name
            string rootClassName = ToPascalCase(rootClassNameBase);
            
            // Explicitly check for reserved names here too
            if (IsReservedTypeName(rootClassName))
            {
                rootClassName = "Pdx" + rootClassName;
                System.Diagnostics.Debug.WriteLine($"ANALYZER: Prefixed reserved name: {rootClassName}");
            }
            
            System.Diagnostics.Debug.WriteLine($"ANALYZER: Final root class name: {rootClassName}");
            
            // Just add it to the top-level names set directly - guarantees uniqueness for the root
            _generatedTopLevelClassNames.Add(rootClassName);
            
            // Start analysis from the root with the pre-reserved name
            if (root != null) // Only analyze if parsing succeeded
            {
                // Root node analysis doesn't have a parent
                var rootClassDef = AnalyzeNode(root, rootClassName, null, isRootNode: true);
                
                // Add information about the root class definition
                if (rootClassDef != null)
                {
                    System.Diagnostics.Debug.WriteLine($"ANALYZER: Root class definition created: {rootClassDef.Name} with {rootClassDef.Properties.Count} properties");
                    currentAnalysis.AddDiagnostic("PDXSA102", "Root Definition", $"Created root definition {rootClassDef.Name} with {rootClassDef.Properties.Count} properties", DiagnosticSeverity.Info);
                    
                    // Now add the root class definition by NAME
                    _definedClassesBySignature[rootClassName] = rootClassDef;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ANALYZER: Root class definition is null!");
                    currentAnalysis.AddDiagnostic("PDXSA103", "Root Definition Error", $"Failed to create root definition for {rootClassName}", DiagnosticSeverity.Error);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ANALYZER: Root element is null after parsing!");
                currentAnalysis.AddDiagnostic("PDXSA104", "Null Root", $"Root element is null after parsing file: {text.Path}", DiagnosticSeverity.Error);
            }

            // Log the final class definition count
            var count = _definedClassesBySignature.Count;
            System.Diagnostics.Debug.WriteLine($"ANALYZER: Final class definition count: {count}");
            if (count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"ANALYZER: No class definitions created!");
                currentAnalysis.AddDiagnostic("PDXSA105", "No Definitions", $"No class definitions were created for file: {text.Path}", DiagnosticSeverity.Error);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ANALYZER: Class definitions created: {string.Join(", ", _definedClassesBySignature.Keys)}");
            }

            // RESTORED: Assign the dictionary containing top-level definitions (keyed by signature or root name)
            // Nested definitions are accessed via the top-level definitions' NestedClasses property.
            currentAnalysis.ClassDefinitions = _definedClassesBySignature
                .Where(kvp => kvp.Value != null && !kvp.Value.IsSimpleType)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value); // Use original key (signature or root name)

            // Log the final filtered class definitions
            System.Diagnostics.Debug.WriteLine($"ANALYZER: Final top-level class definitions count: {currentAnalysis.ClassDefinitions.Count}");
            System.Diagnostics.Debug.WriteLine($"ANALYZER: Final top-level class definitions: {string.Join(", ", currentAnalysis.ClassDefinitions.Keys)}");

            // --- DIAGNOSTIC ADDED ---
            currentAnalysis.AddDiagnostic("PDXSA002", "Definitions Found", $"Analysis for {currentAnalysis.RootName} finished. Found {currentAnalysis.ClassDefinitions.Count} unique class definitions.", DiagnosticSeverity.Info);

        }
        catch (Exception ex)
        {
            // --- DIAGNOSTIC ADDED ---
            System.Diagnostics.Debug.WriteLine($"ANALYZER EXCEPTION: {ex.GetType().Name} - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
    // Now accepts an optional parent definition to handle nested scopes.
    // Returns the simple, unique name within the scope. Tracking is handled by the caller.
    private string GenerateUniqueClassName(string baseName, ClassDefinition? parentDefinition)
    {
        string className = ToPascalCase(baseName);
        if (string.IsNullOrEmpty(className))
        {
            className = "UnnamedClass"; // Fallback for empty names
        }

        // Ensure the name is a valid C# identifier (basic check)
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

        // CRITICAL: Ensure we never use a C# keyword or built-in type name for a class
        // This is especially important for root class names
        if (IsReservedTypeName(className))
        {
            className = "Pdx" + className; // Prefix with "Pdx" to avoid conflicts
        }

        // Determine the correct set to check for collisions
        HashSet<string> nameScope = parentDefinition?._nestedClassNames ?? _generatedTopLevelClassNames;

        // Check for collisions using the relevant scope and append numbers if needed
        // Return the first available name (base or with suffix)
        if (!nameScope.Contains(className))
        {
            // nameScope.Add(className); // REMOVED: Tracking handled by caller
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

        // nameScope.Add(uniqueName); // REMOVED: Tracking handled by caller
        return uniqueName;
    }

    // Helper to check if a name is a C# keyword or built-in type
    private static bool IsReservedTypeName(string name)
    {
        // Common C# keywords and built-in types
        var reservedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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
        
        return reservedNames.Contains(name);
    }

    // Modify AnalyzeNode to detect and ignore likely data keys
    private ClassDefinition AnalyzeNode(SaveElement element, string preferredClassName, ClassDefinition? parentDefinition, bool isRootNode = false)
    {
        if (element is SaveObject saveObject)
        {
            // NEW: First, check if this is a data dictionary (lots of numeric keys or very large keys)
            // If so, we'll treat it as an indexed dictionary instead of creating properties for each key
            bool isLikelyDataDictionary = IsLikelyDataDictionary(saveObject);
            
            // --- Indexed Dictionary Check (Handles integer keys first) ---
            bool isIndexedDictionary = saveObject.Properties.Any() && saveObject.Properties.All(kvp => int.TryParse(kvp.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out _));
            if (isIndexedDictionary || isLikelyDataDictionary)
            {
                // This object represents a Dictionary<int, T> or Dictionary<string, T>, not a class.
                // We need the type 'T'. Analyze the first item's value.
                if (saveObject.Properties.Any())
                {
                    var firstValue = saveObject.Properties.First().Value;
                    string itemBaseName = preferredClassName;
                    if (itemBaseName.EndsWith("s") && itemBaseName.Length > 1) itemBaseName = itemBaseName.Substring(0, itemBaseName.Length - 1);
                    else itemBaseName = $"{itemBaseName}Item";
                    // Pass parent context along when analyzing the item type
                    // Note: For dictionaries, the item type doesn't define a new named class itself,
                    // it describes the type T in Dictionary<Key, T>. So we don't calculate a FullName here yet.
                    var itemType = AnalyzeNode(firstValue, itemBaseName, parentDefinition);

                    // Create a simple type definition representing a dictionary
                    // We'll define the actual dictionary property in the parent
                    return itemType;
                }
                else
                {
                    // Empty dictionary? Treat value type as object.
                    return GetOrCreateClassDefinitionForSimpleType("object", "object"); // Placeholder for T=object
                }
            }

            // --- Regular Object - Signature & Property Analysis ---
            string signature = GenerateObjectSignature(saveObject);

            // Check if this structure is ALREADY DEFINED globally (even if nested elsewhere)
            // We aim to reuse the definition if the structure is identical.
            // TODO: Revisit this - do we want structural sharing across different parent scopes?
            // For now, let's prioritize nesting: if a parent exists, always create nested.
            // If we want structural sharing later, we'd check _definedClassesBySignature first.

            // --- New Class Definition ---
            string uniqueSimpleName; // This will be the simple name (unique within scope)
            string calculatedFullName; // This will be the concatenated full name

            if (isRootNode)
            {
                // Root node: Simple name is pre-reserved, FullName is the same.
                uniqueSimpleName = preferredClassName; // Already unique globally
                calculatedFullName = uniqueSimpleName;
                // Add to global scope tracking
                _generatedTopLevelClassNames.Add(uniqueSimpleName); // Add the simple name
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
                HashSet<string> nameScope = parentDefinition?._nestedClassNames ?? _generatedTopLevelClassNames;
                nameScope.Add(uniqueSimpleName);
            }

            var classDef = new ClassDefinition(uniqueSimpleName); // Create with SIMPLE name
            classDef.FullName = calculatedFullName; // Assign the calculated FULL name

            // Add to parent's nested list if applicable
            if (parentDefinition != null)
            {
                parentDefinition.NestedClasses.Add(classDef);
                // TODO: Still need a way to potentially reuse structurally identical nested classes?
                // Maybe a global lookup by FullName could work here?
            }
            else if (!isRootNode) // Add non-root, top-level classes to global signature dict
            {
                // Note: Using signature for potential reuse of identical top-level structures.
                // Could switch to FullName if strict nesting is always preferred.
                if (_definedClassesBySignature.ContainsKey(signature)) {
                    return _definedClassesBySignature[signature]; // Reuse existing top-level def by signature
                }
                if (!classDef.IsSimpleType && !IsSimpleTypeName(classDef.Name))
                {
                    _definedClassesBySignature.Add(signature, classDef);
                }
            }
            // For isRootNode=true, it's added by NAME (which is also FullName) in the calling method.

            // Group properties by their original key to detect duplicates
            var propertiesGroupedByKey = saveObject.Properties.GroupBy(kvp => kvp.Key);

            foreach (var group in propertiesGroupedByKey)
            {
                string originalName = group.Key;
                string csPropertyName = ToPascalCase(originalName); // PascalCase name for C# property
                var firstKvp = group.First(); // Use the first element for type analysis
                SaveElement firstValueElement = firstKvp.Value;

                if (group.Count() > 1) // DUPLICATE KEYS FOUND -> Treat as List<T>
                {
                    string itemPreferredName = csPropertyName + "Item";
                    // Pass the CURRENT classDef as the parent for the item's type analysis
                    var itemTypeResult = GetPropertyType(firstValueElement, itemPreferredName, classDef);
                    string listPropertyTypeSyntax = $"List<{itemTypeResult.PropertyTypeSyntax}>";

                    if (!classDef.Properties.Any(p => p.OriginalName == originalName))
                    {
                        classDef.Properties.Add(new PropertyDefinition(
                            name: csPropertyName,
                            propertyType: listPropertyTypeSyntax,
                            originalName: originalName,
                            isCollection: true,
                            isDictionary: false,
                            representsDuplicateKeys: true,
                            isPdxDictionaryPattern: false // Explicitly false for duplicate key lists
                        ));
                    }
                }
                else // SINGLE KEY -> Treat as regular property (scalar, object, array, dict)
                {
                    string nestedPreferredName = csPropertyName;
                    // Pass the CURRENT classDef as the parent for the nested type analysis
                    (string propertyTypeSyntax, bool isCollection, bool isDictionary, bool isPdxPattern, ClassDefinition? valueTypeDef) = GetPropertyType(firstValueElement, nestedPreferredName, classDef);

                    if (!classDef.Properties.Any(p => p.OriginalName == originalName))
                    {
                        classDef.Properties.Add(new PropertyDefinition(
                            name: csPropertyName,
                            propertyType: propertyTypeSyntax,
                            originalName: originalName,
                            isCollection: isCollection,
                            isDictionary: isDictionary,
                            representsDuplicateKeys: false,
                            isPdxDictionaryPattern: isPdxPattern // Use the flag returned from GetPropertyType
                        ));
                    }
                }
            }
            return classDef;
        }
        else if (element is SaveArray saveArray)
        {
            if (saveArray.Items.Any())
            {
                // preferredClassName here is the base SIMPLE name intended for the item type (e.g., "ArmyItem")
                // This is determined by the caller (GetPropertyType)
                string itemBaseName = preferredClassName; // Use the name provided by caller directly
                // Pass parent context along for array item analysis
                // AnalyzeNode will generate the unique simple name and full name for the item type
                return AnalyzeNode(saveArray.Items.First(), itemBaseName, parentDefinition);
            }
            else
            {
                // Empty array, treat item type as object
                return GetOrCreateClassDefinitionForSimpleType("object", "object"); // Placeholder for item type T=object
            }
        }
        else // Scalar types
        {
            // For scalars, return a placeholder definition representing the primitive type.
            // This avoids adding primitive types like "int" as full ClassDefinitions in the main dictionary.
            string typeName = GetSimpleTypeName(element); // e.g., "int", "string"
            return GetOrCreateClassDefinitionForSimpleType(typeName, typeName); // Signature and Name are the type name
        }
    }

    // Helper to get a simple C# type name string for a scalar SaveElement
    private string GetSimpleTypeName(SaveElement element)
    {
        return element.Type switch
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
    }


    // Helper to get or create a placeholder ClassDefinition for simple types
    // These are NOT added to the main _definedClassesBySignature dictionary
    private ClassDefinition GetOrCreateClassDefinitionForSimpleType(string signature, string name)
    {
        // CRITICAL: Simple types like "string", "int", etc. should NEVER be added to the 
        // global _definedClassesBySignature dictionary. They should only be used for 
        // determining property types.
        
        // Create a transient definition that won't be used in global class list
        var simpleDef = new ClassDefinition(name); // Name is the C# type name (e.g., "int", "string")
        
        // Mark it as a simple type to prevent it from being added to class list
        simpleDef.IsSimpleType = true;
        
        return simpleDef;
    }

    // NEW: Helper to detect the specific PDX Dictionary pattern: List<{ { key: { value_obj } } }>
    // Returns true if the pattern is detected, along with the determined key type and the first value element for analysis.
    private bool IsPdxDictionaryPattern(SaveArray array, out string? detectedKeyType, out SaveElement? firstValueElement)
    {
        detectedKeyType = null;
        firstValueElement = null;

        if (array == null || !array.Items.Any())
            return false;

        // Check if all items are SaveObjects with exactly one property
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

        // Optional: Could add a check here to verify subsequent items have compatible key types,
        // but for now, we assume homogeneity based on the first item.

        return true;
    }

    // Modify GetPropertyType to accept and pass along the parent context
    // preferredNestedClassName is the suggested *simple* base name for the nested type definition
    // RETURN TYPE UPDATED: Removed AttributeType
    private (string PropertyTypeSyntax, /*string AttributeType,*/ bool IsCollection, bool IsDictionary, bool IsPdxDictionaryPatternFlag, ClassDefinition? ValueTypeDef) GetPropertyType(SaveElement value, string preferredNestedClassName, ClassDefinition parentDefinitionForNestedTypes)
    {
        bool isCollection = false;
        bool isDictionary = false;
        bool isPdxDictionaryPatternFlag = false; // Initialize new flag
        // string attributeType = "SaveScalar"; // Removed AttributeType
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
                // attributeType = "SaveIndexedDictionary"; // Removed
                isDictionary = true;
                if (obj.Properties.Any())
                {
                    // Suggest simple name for dictionary value type
                    string itemBaseName = preferredNestedClassName + "Item";
                    // Pass parent context for dictionary value type analysis
                    var valueTypeResult = GetPropertyType(obj.Properties.First().Value, itemBaseName, parentDefinitionForNestedTypes);
                    // Use the FULL NAME of the value type in the dictionary signature
                    baseTypeSyntax = $"Dictionary<int, {valueTypeResult.PropertyTypeSyntax}>"; // Should use FullName implicitly
                    valueTypeDef = valueTypeResult.ValueTypeDef;
                }
                else
                {
                    baseTypeSyntax = "Dictionary<int, object>";
                    valueTypeDef = GetOrCreateClassDefinitionForSimpleType("object", "object");
                }
            }
            else if (isStringDictionary)
            {
                // Handle dictionary with string keys (recognized as data dictionary)
                // attributeType = "SaveObject"; // Removed
                isDictionary = true; // Mark as dictionary conceptually
                
                if (obj.Properties.Any())
                {
                    // Suggest simple name for dictionary value type
                    string itemBaseName = preferredNestedClassName + "Item";
                    // Pass parent context for dictionary value type analysis
                    var valueTypeResult = GetPropertyType(obj.Properties.First().Value, itemBaseName, parentDefinitionForNestedTypes);
                    // Use the FULL NAME of the value type in the dictionary signature
                    baseTypeSyntax = $"Dictionary<string, {valueTypeResult.PropertyTypeSyntax}>"; // Should use FullName implicitly
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
                // attributeType = "SaveObject"; // Removed
                valueTypeDef = AnalyzeNode(obj, preferredNestedClassName, parentDefinitionForNestedTypes);
                baseTypeSyntax = valueTypeDef.FullName;
            }
        }
        else if (value is SaveArray array)
        {
            // --- Check for PDX Dictionary Pattern ---
            if (IsPdxDictionaryPattern(array, out string? detectedKeyType, out SaveElement? firstValueElement) && detectedKeyType != null && firstValueElement != null)
            {
                // attributeType = "SavePdxDictionary"; // Removed
                isDictionary = true;
                isCollection = false; // It's logically a dictionary
                isPdxDictionaryPatternFlag = true; // *** SET THE NEW FLAG ***

                // Analyze the type of the value element
                string itemBaseName = preferredNestedClassName + "Item"; // Suggest name for value type
                valueTypeDef = AnalyzeNode(firstValueElement, itemBaseName, parentDefinitionForNestedTypes);

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
            // --- End Pattern Check ---
            else // Regular SaveArray (List<T>)
            {
                // attributeType = "SaveArray"; // Removed
                isCollection = true;
                if (array.Items.Any())
                {
                    string itemBaseName = preferredNestedClassName + "Item";
                    var elementTypeResult = GetPropertyType(array.Items.First(), itemBaseName, parentDefinitionForNestedTypes);
                    baseTypeSyntax = $"List<{elementTypeResult.PropertyTypeSyntax}>";
                    valueTypeDef = elementTypeResult.ValueTypeDef;
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
            baseTypeSyntax = GetSimpleTypeName(value);
            // attributeType = "SaveScalar"; // Removed
            valueTypeDef = GetOrCreateClassDefinitionForSimpleType(baseTypeSyntax, baseTypeSyntax);
        }
        else
        {
            baseTypeSyntax = "object";
            // attributeType = "SaveScalar"; // Removed
            valueTypeDef = GetOrCreateClassDefinitionForSimpleType("object", "object");
        }
        // RETURN VALUE UPDATED: Removed AttributeType, Added IsPdxDictionaryPatternFlag
        return (baseTypeSyntax, /*attributeType,*/ isCollection, isDictionary, isPdxDictionaryPatternFlag, valueTypeDef);
    }


    // Keep ToPascalCase helper (or move to a utility class)
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

        // Optional: Handle C# keywords (consider a more robust check if needed)
        var keywords = new HashSet<string> { "class", "event", "object", "string", "int", "float", "bool", "long", "double", "decimal", "public", "private", "protected", "internal", "static", "void", "namespace", "using", "get", "set" /* ... add more as needed */ };
        if (keywords.Contains(name.ToLowerInvariant()))
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

    // Add this helper method to transform PDX-specific nested dictionary patterns
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
        // Each entry follows pattern: { 0: key, 1: value }
        
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
            // Add this diagnostic line
            System.Diagnostics.Debug.WriteLine($"Transformed nested dictionary with {transformedEntries.Count} entries. First key: {(transformedEntries.Count > 0 ? transformedEntries[0].Key : "none")}");
            
            return new SaveObject(transformedEntries);
        }
        
        // Otherwise return the original
        return originalObject;
    }
    
    // Add this helper method to recursively transform any nested dictionaries
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
        // Further check: are most keys numeric or extremely long?
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

    // Add a new helper method to check if a name is a simple type
    private bool IsSimpleTypeName(string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return false;
        
        // Check common simple type names
        var simpleTypeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "string", "int", "long", "float", "double", "bool", "DateTime", "Guid",
            "object", "byte", "sbyte", "short", "ushort", "uint", "ulong", "char",
            "decimal", "void"
        };
        
        return simpleTypeNames.Contains(typeName);
    }
}