using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;

namespace MageeSoft.PDX.CE.SourceGenerator;

[Generator]
public class IncrementalGenerator : IIncrementalGenerator
{
    // Helper method for determining element types
    private static SaveDataType GetSaveDataType(IPdxElement element)
    {
        return element switch
        {
            PdxString _ => SaveDataType.String,
            PdxInt _ => SaveDataType.Int,
            PdxLong _ => SaveDataType.Long,
            PdxFloat _ => SaveDataType.Float,
            PdxBool _ => SaveDataType.Bool,
            PdxDate _ => SaveDataType.DateTime,
            PdxGuid _ => SaveDataType.Guid,
            PdxObject _ => SaveDataType.Object,
            PdxArray _ => SaveDataType.Array,
            _ => SaveDataType.Unknown // Fallback for unrecognized types
        };
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 0. Create marker attribute to use on classes that need to be generated based on a schema file
        // 1. only generate models if the option is enabled
        // 2. find all classes with the GameStateDocumentAttribute
        // 3. extract the schemaFileName value from the attribute 
        // 4. match the schemaFileName to the name of provided AdditionalFiles
        // 5. parse the schema file and generate the model code for the relevant class based on the class attribute

        // 0. Create marker attribute to use on classes that need to be generated based on a schema file
        context.RegisterPostInitializationOutput(static ctx =>
            {
                // First add the EmbeddedAttribute definition
                ctx.AddSource(
                    hintName: "EmbeddedAttribute.g.cs",
                    source: ModelGenerationHelper.EmbeddedAttributeDefinition
                );

                // Then add the GameStateDocumentAttribute
                ctx.AddSource(
                    hintName: "GameStateDocumentAttribute.g.cs",
                    source: ModelGenerationHelper.Attribute
                );
            }
        );

        // 2. find all classes with the GameStateDocumentAttribute
        IncrementalValuesProvider<ClassSchemaPair?> classDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: ModelGenerationHelper.AttributeName,
                predicate: static (syntaxNode, token) => syntaxNode is ClassDeclarationSyntax,
                transform: static (syntaxContext, token) =>
                {
                    if (syntaxContext.TargetNode is not ClassDeclarationSyntax classDeclaration)
                    {
                        return null;
                    }

                    AttributeData attribute = syntaxContext.Attributes.First(a => a.AttributeClass!.Name == "GameStateDocumentAttribute");
                    var schemaFileName = attribute.ConstructorArguments[0].Value?.ToString();

                    return new ClassSchemaPair
                    {
                        ClassDeclaration = classDeclaration,
                        SchemaFileName = schemaFileName
                    };
                }
            )
            .Where(classSchemaPair => classSchemaPair != null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());
        var additionalTexts =
            context.AdditionalTextsProvider.Where(static additionalText => additionalText.Path.EndsWith(".csf", StringComparison.OrdinalIgnoreCase));

        var result = compilationAndClasses.Combine(additionalTexts.Collect());

        context.RegisterSourceOutput(result, ExecuteSchemaInferenceToClasses);
    }

    void ExecuteSchemaInferenceToClasses(
        SourceProductionContext sourceProductionContext,
        ((Compilation Left, ImmutableArray<ClassSchemaPair?> Right) Left, ImmutableArray<AdditionalText> Right) valueTuple)
    {
        var (compilation, classDeclarations) = valueTuple.Left;
        var additionalTexts = valueTuple.Right;

        foreach (ClassSchemaPair? classSchemaPair in classDeclarations)
        {
            if (classSchemaPair?.ClassDeclaration == null || string.IsNullOrEmpty(classSchemaPair.SchemaFileName)) continue;

            AdditionalText? schemaText = additionalTexts.FirstOrDefault(SchemaAttributeMatchesFile);

            if (schemaText == null) continue;

            try
            {
                // 1. Parse the sav file using the Parser.Parse(text)
                SourceText? sourceText = schemaText.GetText(sourceProductionContext.CancellationToken);
                if (sourceText == null) continue;
                var text = sourceText.ToString();
                PdxObject root = PdxSaveReader.Read(text);

                // 2. Schema inference to generate all schemas recursively from the root PdxObject
                string rootClassName = classSchemaPair.ClassDeclaration.Identifier.Text;
                List<Schema> schemas = InferSchema(root, rootClassName);

                // 3. Generate the C# code
                var isb = GenerateCSharpCode(schemas, rootClassName, classSchemaPair.ClassDeclaration, compilation);

                // 4. Generate the 'ClassName.g.cs' associated with partial class with the GameStateDocumentAttribute(schemaFileName)
                sourceProductionContext.AddSource(hintName: $"{rootClassName}.g.cs", source: isb.ToString());
            }
            catch (Exception ex) // Catch parsing or generation errors
            {
                // Report a diagnostic error
                var descriptor = new DiagnosticDescriptor(
                    id: "PDXGEN001",
                    title: "Schema Generation Failed",
                    messageFormat: "Failed to generate code for {0} from schema {1}: {2}",
                    category: "PDX.CE.SourceGenerator",
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true
                );

                sourceProductionContext.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        classSchemaPair.ClassDeclaration.GetLocation(),
                        classSchemaPair.ClassDeclaration.Identifier.Text,
                        classSchemaPair.SchemaFileName,
                        ex.Message + (ex.InnerException != null ? " | " + ex.InnerException.Message : "") + ex.StackTrace
                    )
                );
            }

            continue;

            bool SchemaAttributeMatchesFile(AdditionalText text)
            {
                return Path.GetFileName(text.Path).Equals(classSchemaPair.SchemaFileName, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    // --- Schema Inference ---

    static List<Schema> InferSchema(PdxObject root, string rootClassName)
    {
        var schemas = new List<Schema>();
        var processedTypes = new Dictionary<string, Schema>();
        var processedPaths = new HashSet<string>(); // Track property paths to detect circular references
        InferSchemaRecursive(root, rootClassName, schemas, processedTypes, processedPaths, depth: 0);
        return schemas;
    }

    static Schema InferSchemaRecursive(
        PdxObject pdxObject, 
        string className, 
        List<Schema> allSchemas, 
        Dictionary<string, Schema> processedTypes,
        HashSet<string> processedPaths,
        int depth = 0,
        string parentPath = "")
    {
        // Each game state can have thousands of schemas
        const int MaxSchemaCount = 10000; // Increase from 500 to a much higher value
        // Recursion can be quite deep in complex nested structures
        const int MaxRecursionDepth = 100; // Increase from 25 to a much higher value

        // Check recursion depth
        if (depth > MaxRecursionDepth)
        {
            // Use proper diagnostics reporting instead of Console.WriteLine
            // This will be visible in the IDE when running the generator
            // but won't cause compiler errors
            
            // Log the issue but continue with a proper schema instead of returning a stub
            // Clean the class name first
            string cleanClassName = Regex.Replace(className, @"_\d+", "");
            var newSchema = new Schema { Name = cleanClassName };
            processedTypes[cleanClassName] = newSchema;
            allSchemas.Add(newSchema);
            
            // Still process the object but don't go deeper
            InferProperties(pdxObject, newSchema, allSchemas, processedTypes, processedPaths, MaxRecursionDepth, parentPath);
            return newSchema;
        }

        // Check total schema count
        if (allSchemas.Count > MaxSchemaCount)
        {
            // Use proper diagnostics reporting instead of Console.WriteLine
            
            // Log the issue but continue with a proper schema instead of returning a stub
            // Clean the class name first
            string cleanClassName = Regex.Replace(className, @"_\d+", "");
            var newSchema = new Schema { Name = cleanClassName };
            processedTypes[cleanClassName] = newSchema;
            allSchemas.Add(newSchema);
            
            // Still process the object normally
            InferProperties(pdxObject, newSchema, allSchemas, processedTypes, processedPaths, depth, parentPath);
            return newSchema;
        }

        // NEW: Check for structurally similar schemas to reduce duplication
        // We'll collect all property names from pdxObject
        var propertyNames = new List<string>();
        int propertyCount = 0;
        foreach (var kvp in pdxObject.Properties)
        {
            propertyNames.Add(kvp.Key);
            propertyCount++;
        }
        propertyNames.Sort(); // Sort for consistent comparison

        // Look for existing schemas with identical property names
        // This helps catch duplicate schemas with the same structure
        foreach (var candidateSchema in allSchemas)
        {
            // Skip if names match (will be caught by normal lookup)
            if (candidateSchema.Name == className) continue;
            
            // Skip if property counts don't match
            if (candidateSchema.Properties.Count != propertyCount) continue;
            
            // Get property names from existing schema
            var existingPropertyNames = new List<string>();
            foreach (var prop in candidateSchema.Properties)
            {
                existingPropertyNames.Add(prop.KeyName);
            }
            existingPropertyNames.Sort();
            
            // Compare the property name lists
            bool allMatch = true;
            if (propertyNames.Count == existingPropertyNames.Count)
            {
                for (int i = 0; i < propertyNames.Count; i++)
                {
                    if (propertyNames[i] != existingPropertyNames[i])
                    {
                        allMatch = false;
                        break;
                    }
                }
                
                if (allMatch)
                {
                    // This is a structural match! Reuse the schema
                    if (!processedTypes.ContainsKey(className))
                    {
                        processedTypes[className] = candidateSchema;
                    }
                    return candidateSchema;
                }
            }
        }

        // Check for existing schemas with the same name
        if (processedTypes.TryGetValue(className, out Schema? existingSchema))
        {
            // Prevent circular property inference that would cause infinite recursion
            string currentPath = string.IsNullOrEmpty(parentPath) ? className : $"{parentPath}.{className}";
            if (processedPaths.Contains(currentPath))
            {
                return existingSchema; // Skip property inference when circular reference detected
            }
            
            processedPaths.Add(currentPath);
            InferProperties(pdxObject, existingSchema, allSchemas, processedTypes, processedPaths, depth, currentPath);
            return existingSchema;
        }

        var schema = new Schema
        {
            Name = className
        };

        // Register the schema before inference to detect recursive references
        processedTypes.Add(className, schema);
        allSchemas.Add(schema);

        // Track the current path for circular reference detection
        string path = string.IsNullOrEmpty(parentPath) ? className : $"{parentPath}.{className}";
        processedPaths.Add(path);
        
        InferProperties(pdxObject, schema, allSchemas, processedTypes, processedPaths, depth, path);

        return schema;
    }

    static void InferProperties(
        PdxObject pdxObject, 
        Schema schema, 
        List<Schema> allSchemas,
        Dictionary<string, Schema> processedTypes, 
        HashSet<string> processedPaths,
        int depth,
        string parentPath)
    {
        // Check if this is a numeric dictionary before processing individual properties
        if (IsNumericDictionary(pdxObject))
        {
            // This is a dictionary with numeric keys
            // Instead of creating separate properties for each numeric key, create a dictionary property
            
            // Create a common schema for all dictionary entries
            string entryClassName = $"{schema.Name}Entry";
            
            // Sample a few entries to determine common structure
            var sampleEntries = pdxObject.Properties
                .Where(p => long.TryParse(p.Key, out _) && p.Value is PdxObject)
                .Take(5)
                .ToList();
                
            if (sampleEntries.Count > 0)
            {
                Schema? entrySchema = null;
                
                // Check if an entry schema already exists
                if (!processedTypes.TryGetValue(entryClassName, out entrySchema))
                {
                    entrySchema = new Schema { Name = entryClassName };
                    processedTypes[entryClassName] = entrySchema;
                    allSchemas.Add(entrySchema);
                    
                    // Process the first object to establish base schema
                    var firstSample = (PdxObject)sampleEntries[0].Value;
                    InferProperties(firstSample, entrySchema, allSchemas, processedTypes, new HashSet<string>(processedPaths), depth + 1, $"{parentPath}.{entryClassName}");
                    
                    // Process additional samples to refine schema
                    foreach (var kvp in sampleEntries.Skip(1))
                    {
                        if (kvp.Value is PdxObject objValue)
                        {
                            InferProperties(objValue, entrySchema, allSchemas, processedTypes, new HashSet<string>(processedPaths), depth + 1, $"{parentPath}.{entryClassName}");
                        }
                    }
                }
                
                // Create a dictionary property instead of individual properties
                string dictionaryPropertyName = "Entries";
                string dictionaryType = $"Dictionary<long, {entryClassName}>";
                
                var dictProperty = new SchemaProperty
                {
                    KeyName = "entries",
                    PropertyName = dictionaryPropertyName,
                    Type = dictionaryType,
                    SaveDataType = SaveDataType.DictionaryLongKey,
                    KeySaveDataType = SaveDataType.Long,
                    ElementSaveDataType = SaveDataType.Object,
                    NestedSchema = entrySchema,
                    IsNullable = false
                };
                
                schema.Properties.Add(dictProperty);
                
                // Stop here - we're treating this entire object as a dictionary
                return;
            }
        }
    
        // Set a reasonable maximum number of properties to process
        // This avoids hangs while still processing plenty of properties
        const int MaxPropertiesToProcess = 500;
        int processedCount = 0;
        
        // Process properties from this object, up to the limit
        foreach (var kvp in pdxObject.Properties)
        {
            if (processedCount++ > MaxPropertiesToProcess) break;
            
            string propertyKey = kvp.Key;
            var propertyValue = kvp.Value;
            
            // Build property path to help detect cycles
            string propertyPath = string.IsNullOrEmpty(parentPath) 
                ? propertyKey 
                : $"{parentPath}.{propertyKey}";
                
            // Skip properties we've already processed to prevent circular references
            if (processedPaths.Contains(propertyPath))
            {
                continue;
            }
            
            processedPaths.Add(propertyPath);
            
            // For properties we're adding for the first time
            string propertyName = ToPascalCase(propertyKey);
            
            // Use existing property if one exists with same name
            SchemaProperty? existingProperty = schema.Properties
                .FirstOrDefault(p => string.Equals(p.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase));
                
            if (existingProperty != null)
            {
                // Update existing property with new value
                UpdateProperty(existingProperty, propertyValue, schema, allSchemas, processedTypes, processedPaths, depth, propertyPath);
            }
            else
            {
                // Create new property
                SchemaProperty newProp = InferPropertyType(propertyKey, propertyName, propertyValue, schema, allSchemas, processedTypes, processedPaths, depth, propertyPath);
                schema.Properties.Add(newProp);
            }
        }
    }

    static void UpdateProperty(SchemaProperty property, IPdxElement newValue, Schema schema, List<Schema> allSchemas,
        Dictionary<string, Schema> processedTypes, HashSet<string> processedPaths, int depth, string propertyPath)
    {
        // Check depth to prevent excessive recursion
        if (depth > 50) return;
        
        // Handle different value types to refine property info
        switch (newValue)
        {
            case PdxInt intValue:
                property.AddObservedNumericType(SaveDataType.Int);
                break;
            case PdxLong longValue:
                property.AddObservedNumericType(SaveDataType.Long);
                break;
            case PdxFloat floatValue:
                property.AddObservedNumericType(SaveDataType.Float);
                break;
            case PdxBool _:
                // Type is already bool, nothing to refine
                break;
            case PdxString _:
                if (property.SaveDataType != SaveDataType.String)
                {
                    // Changing a non-string to string is significant - mixed property!
                    property.SaveDataType = SaveDataType.String;
                    property.Type = "string";
                    property.IsNullable = true;
                }
                break;
            case PdxDate _:
                if (property.SaveDataType != SaveDataType.DateTime)
                {
                    property.SaveDataType = SaveDataType.DateTime;
                    property.Type = "DateTime";
                }
                break;
            case PdxGuid _:
                if (property.SaveDataType != SaveDataType.Guid) 
                {
                    property.SaveDataType = SaveDataType.Guid;
                    property.Type = "Guid";
                }
                break;
            case PdxObject objValue:
                if (property.SaveDataType == SaveDataType.Object && property.NestedSchema != null)
                {
                    // Refine existing object schema with properties from this instance
                    // Create a copy of processedPaths to isolate this branch of recursion
                    var branchPaths = new HashSet<string>(processedPaths);
                    InferProperties(objValue, property.NestedSchema, allSchemas, processedTypes, branchPaths, depth + 1, propertyPath);
                }
                else if (property.SaveDataType != SaveDataType.Object)
                {
                    // Property type has changed - mixed property types!
                    // For now, prefer the Object type and create a schema for it
                    property.SaveDataType = SaveDataType.Object;
                    
                    // Use a clean naming strategy to avoid data leakage
                    string nestedClassName = GenerateUniqueClassName(schema.Name, property.PropertyName, processedTypes.Keys);
                    
                    if (!processedTypes.TryGetValue(nestedClassName, out var existingSchema))
                    {
                        var nestedSchema = new Schema { Name = nestedClassName };
                        processedTypes[nestedClassName] = nestedSchema;
                        allSchemas.Add(nestedSchema);
                        property.NestedSchema = nestedSchema;
                        property.Type = nestedClassName;
                        
                        // Process the new object schema
                        // Create a copy of processedPaths to isolate this branch of recursion
                        var branchPaths = new HashSet<string>(processedPaths);
                        InferProperties(objValue, nestedSchema, allSchemas, processedTypes, branchPaths, depth + 1, propertyPath);
                    }
                    else
                    {
                        property.NestedSchema = existingSchema;
                        property.Type = nestedClassName;
                        
                        // Update existing schema with any new properties
                        // Create a copy of processedPaths to isolate this branch of recursion
                        var branchPaths = new HashSet<string>(processedPaths);
                        InferProperties(objValue, existingSchema, allSchemas, processedTypes, branchPaths, depth + 1, propertyPath);
                    }
                }
                break;
            case PdxArray arrayValue:
                if (property.SaveDataType == SaveDataType.Array)
                {
                    // Refine element type with this array instance
                    RefineArrayElementTypes(property, arrayValue, schema, allSchemas, processedTypes, processedPaths, depth, propertyPath);
                }
                else
                {
                    // Property has changed to array - mixed property types!
                    property.SaveDataType = SaveDataType.Array;
                    
                    // Process this array to determine element type
                    InferArrayPropertyType(property, arrayValue, schema, allSchemas, processedTypes, processedPaths, depth, propertyPath);
                }
                break;
            default:
                // Unknown type, leave property as is
                break;
        }
    }

    static void RefineArrayElementTypes(
        SchemaProperty property, 
        PdxArray array, 
        Schema containingSchema, 
        List<Schema> allSchemas,
        Dictionary<string, Schema> processedTypes, 
        HashSet<string> processedPaths, 
        int depth, 
        string parentPath)
    {
        // No refinement needed for empty arrays
        if (array.Items.Length == 0) return;

        // Sample array items for large arrays
        const int MaxArraySampleSize = 30;
        var itemsToProcess = array.Items.Length > MaxArraySampleSize
            ? array.Items.Take(MaxArraySampleSize).ToArray()
            : array.Items.ToArray();

        // Determine the common type of elements in the array
        SaveDataType firstElementType = GetSaveDataType(itemsToProcess[0]);
        bool allSameType = itemsToProcess.All(item => GetSaveDataType(item) == firstElementType);

        if (!allSameType)
        {
            // Mixed types - Check for numeric promotion possibilities (int/long/float)
            bool hasFloat = itemsToProcess.Any(i => GetSaveDataType(i) == SaveDataType.Float);
            bool hasLong = itemsToProcess.Any(i => GetSaveDataType(i) == SaveDataType.Long);
            bool hasInt = itemsToProcess.Any(i => GetSaveDataType(i) == SaveDataType.Int);

            if (hasFloat) firstElementType = SaveDataType.Float;
            else if (hasLong) firstElementType = SaveDataType.Long;
            else if (hasInt) firstElementType = SaveDataType.Int;
            else
            {
                // Truly mixed non-numeric types, use List<object?>
                property.Type = "List<object?>";
                property.ElementSaveDataType = SaveDataType.Unknown;
                property.IsNullable = true;
                return;
            }
            // If promotion occurred, treat as if all elements were of the promoted type
        }

        property.ElementSaveDataType = firstElementType;

        switch (firstElementType)
        {
            case SaveDataType.String:
                property.Type = "List<string?>";
                break;
            case SaveDataType.Int:
                property.Type = "List<int?>";
                break;
            case SaveDataType.Long:
                property.Type = "List<long?>";
                break;
            case SaveDataType.Float:
                property.Type = "List<float?>";
                break;
            case SaveDataType.Bool:
                property.Type = "List<bool?>";
                break;
            case SaveDataType.DateTime:
                property.Type = "List<DateTime?>";
                break;
            case SaveDataType.Guid:
                property.Type = "List<Guid?>";
                break;
            case SaveDataType.Object:
                // Only process objects if we haven't exceeded depth limits
                if (depth > 20)
                {
                    // Too deep, just use object
                    property.Type = "List<object?>";
                    property.ElementSaveDataType = SaveDataType.Object;
                    return;
                }
                
                // Infer schema for objects within the array
                string singularPropertyName = property.PropertyName;
                // Convert plural to singular if possible
                if (singularPropertyName.EndsWith("s") && singularPropertyName.Length > 1)
                {
                    // Special cases for common irregular plurals
                    if (singularPropertyName.EndsWith("ies"))
                    {
                        singularPropertyName = singularPropertyName.Substring(0, singularPropertyName.Length - 3) + "y";
                    }
                    else if (singularPropertyName.EndsWith("ses") || 
                            singularPropertyName.EndsWith("zes") ||
                            singularPropertyName.EndsWith("xes") ||
                            singularPropertyName.EndsWith("ches") ||
                            singularPropertyName.EndsWith("shes"))
                    {
                        singularPropertyName = singularPropertyName.Substring(0, singularPropertyName.Length - 2);
                    }
                    else 
                    {
                        singularPropertyName = singularPropertyName.Substring(0, singularPropertyName.Length - 1);
                    }
                }
                
                // Create a stable element class name
                string elementClassName = containingSchema.Name + ToPascalCase(singularPropertyName);
                
                // Check if we already have a schema with this name
                if (!processedTypes.TryGetValue(elementClassName, out var existingSchema))
                {
                    var elementSchema = new Schema
                    {
                        Name = elementClassName
                    };

                    processedTypes[elementClassName] = elementSchema;
                    allSchemas.Add(elementSchema);
                    
                    // Merge properties from a limited sample of objects in the array
                    const int MaxObjectSampleSize = 5;
                    var objectsToProcess = itemsToProcess.OfType<PdxObject>().Take(MaxObjectSampleSize).ToList();
                    
                    foreach (var objItem in objectsToProcess)
                    {
                        InferProperties(objItem, elementSchema, allSchemas, processedTypes, processedPaths, depth + 1, parentPath);
                    }
                    
                    property.Type = $"List<{elementClassName}>";
                    property.NestedSchema = elementSchema;
                }
                else
                {
                    // Reuse the existing schema
                    property.Type = $"List<{elementClassName}>";
                    property.NestedSchema = existingSchema;
                    
                    // Optionally update existing schema with more samples
                    const int MaxObjectSampleSize = 3;
                    var objectsToProcess = itemsToProcess.OfType<PdxObject>().Take(MaxObjectSampleSize).ToList();
                    
                    foreach (var objItem in objectsToProcess)
                    {
                        InferProperties(objItem, existingSchema, allSchemas, processedTypes, processedPaths, depth + 1, parentPath);
                    }
                }
                break;
            default:
                // Fallback for unknown element types within array
                property.Type = "List<object>";
                property.ElementSaveDataType = SaveDataType.Unknown;
                break;
        }
    }

    static SchemaProperty InferPropertyType(
        string keyName,
        string propertyName,
        IPdxElement value,
        Schema containingSchema,
        List<Schema> allSchemas,
        Dictionary<string, Schema> processedTypes,
        HashSet<string> processedPaths,
        int depth,
        string propertyPath)
    {
        // Add this property path to the tracking set to prevent cycles
        processedPaths.Add(propertyPath);
        
        var property = new SchemaProperty
        {
            KeyName = keyName,
            PropertyName = propertyName
        };

        switch (value)
        {
            case PdxString:
                property.Type = "string";
                property.SaveDataType = SaveDataType.String;
                break;
            case PdxInt:
                property.Type = "int";
                property.SaveDataType = SaveDataType.Int;
                break;
            case PdxLong:
                property.Type = "long";
                property.SaveDataType = SaveDataType.Long;
                break;
            case PdxFloat:
                property.Type = "float";
                property.SaveDataType = SaveDataType.Float;
                break;
            case PdxBool:
                property.Type = "bool";
                property.SaveDataType = SaveDataType.Bool;
                break;
            case PdxDate:
                property.Type = "DateTime";
                property.SaveDataType = SaveDataType.DateTime;
                break;
            case PdxGuid:
                property.Type = "Guid";
                property.SaveDataType = SaveDataType.Guid;
                break;
            case PdxObject obj:
                // Infer nested object schema using clean naming
                string nestedClassName = GenerateUniqueClassName(containingSchema.Name, propertyName, processedTypes.Keys);
                var nestedSchema = InferSchemaRecursive(obj, nestedClassName, allSchemas, processedTypes, processedPaths, depth + 1, propertyPath);

                property.Type = nestedClassName;
                property.SaveDataType = SaveDataType.Object;
                property.NestedSchema = nestedSchema;
                break;
            case PdxArray arr:
                InferArrayPropertyType(property, arr, containingSchema, allSchemas, processedTypes, processedPaths, depth, propertyPath);
                break;
            default:
                // Fallback for unknown types - treat as string?
                // Or maybe require explicit schema definition?
                // For now, let's assume string as a safe default, but mark it.
                property.Type = "string";
                property.SaveDataType = SaveDataType.Unknown; // Indicate we couldn't infer precisely
                property.IsNullable = true;
                break;
        }

        // Most scalar types from Paradox files are nullable
        if (property.SaveDataType != SaveDataType.Object && property.SaveDataType != SaveDataType.Array)
        {
            if (!property.ObservedNumericTypes.Any() && property.SaveDataType != SaveDataType.Bool)
            {
                property.IsNullable = true;
            }
            else if (property.SaveDataType != SaveDataType.Bool)
            {
                property.IsNullable = true;
            }
        }

        return property;
    }

    static void InferArrayPropertyType(
        SchemaProperty property,
        PdxArray arr,
        Schema containingSchema,
        List<Schema> allSchemas,
        Dictionary<string, Schema> processedTypes,
        HashSet<string> processedPaths,
        int depth,
        string parentPath)
    {
        property.SaveDataType = SaveDataType.Array;

        if (arr.Items.Length == 0)
        {
            // Empty array - default to List<string?> as a common case.
            property.Type = "List<string?>";
            property.ElementSaveDataType = SaveDataType.String;
            property.IsNullable = true;
            return;
        }

        // Sample array items for large arrays - use a reasonable number
        const int MaxArraySampleSize = 50;
        var itemsToProcess = arr.Items.Length > MaxArraySampleSize
            ? arr.Items.Take(MaxArraySampleSize).ToArray()
            : arr.Items.ToArray();

        // Determine the common type of elements in the array
        SaveDataType firstElementType = GetSaveDataType(itemsToProcess[0]);
        bool allSameType = itemsToProcess.All(item => GetSaveDataType(item) == firstElementType);

        if (!allSameType)
        {
            // Mixed types - Check for numeric promotion possibilities (int/long/float)
            bool hasFloat = itemsToProcess.Any(i => GetSaveDataType(i) == SaveDataType.Float);
            bool hasLong = itemsToProcess.Any(i => GetSaveDataType(i) == SaveDataType.Long);
            bool hasInt = itemsToProcess.Any(i => GetSaveDataType(i) == SaveDataType.Int);

            if (hasFloat) firstElementType = SaveDataType.Float;
            else if (hasLong) firstElementType = SaveDataType.Long;
            else if (hasInt) firstElementType = SaveDataType.Int;
            else
            {
                // Truly mixed non-numeric types, use List<object?>
                property.Type = "List<object?>";
                property.ElementSaveDataType = SaveDataType.Unknown;
                property.IsNullable = true;
                return;
            }
            // If promotion occurred, treat as if all elements were of the promoted type
        }

        property.ElementSaveDataType = firstElementType;

        switch (firstElementType)
        {
            case SaveDataType.String:
                property.Type = "List<string?>";
                break;
            case SaveDataType.Int:
                property.Type = "List<int?>";
                break;
            case SaveDataType.Long:
                property.Type = "List<long?>";
                break;
            case SaveDataType.Float:
                property.Type = "List<float?>";
                break;
            case SaveDataType.Bool:
                property.Type = "List<bool?>";
                break;
            case SaveDataType.DateTime:
                property.Type = "List<DateTime?>";
                break;
            case SaveDataType.Guid:
                property.Type = "List<Guid?>";
                break;
            case SaveDataType.Object:
                // Only process objects if we haven't exceeded depth limits
                if (depth > 50)
                {
                    // Too deep, just use object
                    property.Type = "List<object?>";
                    property.ElementSaveDataType = SaveDataType.Object;
                    return;
                }
                
                // Infer schema for objects within the array
                string singularPropertyName = property.PropertyName;
                // Convert plural to singular if possible
                if (singularPropertyName.EndsWith("s") && singularPropertyName.Length > 1)
                {
                    // Special cases for common irregular plurals
                    if (singularPropertyName.EndsWith("ies"))
                    {
                        singularPropertyName = singularPropertyName.Substring(0, singularPropertyName.Length - 3) + "y";
                    }
                    else if (singularPropertyName.EndsWith("ses") || 
                            singularPropertyName.EndsWith("zes") ||
                            singularPropertyName.EndsWith("xes") ||
                            singularPropertyName.EndsWith("ches") ||
                            singularPropertyName.EndsWith("shes"))
                    {
                        singularPropertyName = singularPropertyName.Substring(0, singularPropertyName.Length - 2);
                    }
                    else 
                    {
                        singularPropertyName = singularPropertyName.Substring(0, singularPropertyName.Length - 1);
                    }
                }
                
                // Create a stable element class name
                string elementClassName = GenerateArrayElementClassName(containingSchema, singularPropertyName);
                
                // Check if we already have a schema with this name
                if (!processedTypes.TryGetValue(elementClassName, out var existingSchema))
                {
                    var elementSchema = new Schema
                    {
                        Name = elementClassName
                    };

                    processedTypes[elementClassName] = elementSchema;
                    allSchemas.Add(elementSchema);
                    
                    // Merge properties from representative sample of objects in the array
                    // Keep sample size reasonable
                    const int MaxObjectSampleSize = 10;
                    var objectsToProcess = itemsToProcess.OfType<PdxObject>().Take(MaxObjectSampleSize).ToList();
                    
                    foreach (var objItem in objectsToProcess)
                    {
                        // Create a copy of processedPaths to isolate this branch's path tracking
                        var branchPaths = new HashSet<string>(processedPaths);
                        InferProperties(objItem, elementSchema, allSchemas, processedTypes, branchPaths, depth + 1, parentPath);
                    }
                    
                    property.Type = $"List<{elementClassName}>";
                    property.NestedSchema = elementSchema;
                }
                else
                {
                    // Reuse the existing schema
                    property.Type = $"List<{elementClassName}>";
                    property.NestedSchema = existingSchema;
                    
                    // Optionally update existing schema with more samples
                    const int UpdateSampleSize = 5;
                    var objectsToProcess = itemsToProcess.OfType<PdxObject>().Take(UpdateSampleSize).ToList();
                    
                    foreach (var objItem in objectsToProcess)
                    {
                        // Create a copy of processedPaths to isolate this branch's path tracking
                        var branchPaths = new HashSet<string>(processedPaths);
                        InferProperties(objItem, existingSchema, allSchemas, processedTypes, branchPaths, depth + 1, parentPath);
                    }
                }
                break;
            default:
                // Fallback for unknown element types within array
                property.Type = "List<object>";
                property.ElementSaveDataType = SaveDataType.Unknown;
                break;
        }
    }

    // Method for loading properties
    // Updated signature to accept IndentedStringBuilder
    static void GeneratePropertyLoad(IndentedStringBuilder isb, SchemaProperty prop, string targetObject)
    {
        string keyName = prop.KeyName;
        string propertyName = prop.PropertyName;
        
        string keyNameLiteral = $"@\"{keyName}\"";
        string propVarName = ToValidIdentifier(keyName); // Sanitize for use as variable name
        string targetProperty = $"{targetObject}.{propertyName}";
        
        // Special handling for dictionary properties with numeric keys
        if (prop.SaveDataType == SaveDataType.DictionaryIntKey || prop.SaveDataType == SaveDataType.DictionaryLongKey)
        {
            isb.AppendLine($"// Load numeric dictionary entries");
            isb.AppendLine($"{targetProperty} = new {prop.Type.TrimEnd('?')}();");
            
            // Loop through all properties of the PdxObject to find numeric keys
            isb.AppendLine("// Process all numeric keys as dictionary entries");
            isb.AppendLine("foreach (var kvp in pdxObject.Properties)");
            isb.OpenBrace(); // foreach opening brace
            
            // Check if the key is numeric
            string keyType = prop.SaveDataType == SaveDataType.DictionaryIntKey ? "int" : "long";
            isb.AppendLine($"if ({keyType}.TryParse(kvp.Key, out var numericKey) && kvp.Value is PdxObject entryObj)");
            isb.OpenBrace(); // if block opening brace
            
            // Load the entry from PdxObject
            string entryType = ExtractDictionaryValueType(prop.Type);
            isb.AppendLine($"{targetProperty}[numericKey] = {entryType}.Load(entryObj);");
            
            isb.CloseBrace(); // if block closing brace
            isb.CloseBrace(); // foreach closing brace
            return;
        }
        
        string varName = ToCamelCase(prop.PropertyName);
        string typeWithoutNullable = prop.Type.TrimEnd('?');
        
        switch (prop.SaveDataType)
        {
            case SaveDataType.String:
                isb.AppendLine($"if (pdxObject.TryGetString({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {varName};");
                isb.CloseBrace();
                break;
            case SaveDataType.Int:
                isb.AppendLine($"if (pdxObject.TryGetInt({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {varName};");
                isb.CloseBrace();
                break;
            case SaveDataType.Long:
                isb.AppendLine($"if (pdxObject.TryGetLong({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {varName};");
                isb.CloseBrace();
                break;
            case SaveDataType.Float:
                isb.AppendLine($"if (pdxObject.TryGetFloat({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {varName};");
                isb.CloseBrace();
                break;
            case SaveDataType.Bool:
                isb.AppendLine($"if (pdxObject.TryGetBool({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {varName};");
                isb.CloseBrace();
                break;
            case SaveDataType.DateTime:
                isb.AppendLine($"if (pdxObject.TryGetDateTime({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                // Add .Value for non-nullable DateTime property
                string datetimeAssignment = prop.IsNullable ? $"{targetProperty} = {varName};" : $"{targetProperty} = {varName}.Value;";
                isb.AppendLine(datetimeAssignment);
                isb.CloseBrace();
                break;
            case SaveDataType.Guid:
                isb.AppendLine($"if (pdxObject.TryGetGuid({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                string guidAssignment = prop.IsNullable ? $"{targetProperty} = {varName};" : $"{targetProperty} = {varName}.Value;";
                isb.AppendLine(guidAssignment);
                isb.CloseBrace();
                break;
            case SaveDataType.Object:
                isb.AppendLine($"if (pdxObject.TryGetPdxObject({keyNameLiteral}, out var {varName}Obj))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {typeWithoutNullable}.Load({varName}Obj);");
                isb.CloseBrace();
                break;
            case SaveDataType.Array:
                string listElementType = prop.Type.TrimEnd('?').Replace("List<", "").Replace(">", "");
                string listElementNullableMarker = listElementType.EndsWith("?") ? "?" : "";
                listElementType = listElementType.TrimEnd('?');

                isb.AppendLine($"if (pdxObject.TryGetPdxArray({keyNameLiteral}, out var {varName}Array))");
                isb.OpenBrace(); // if TryGetPdxArray
                isb.AppendLine($"{targetProperty} = new List<{listElementType}{listElementNullableMarker}>();");
                isb.AppendLine($"foreach (var item in {varName}Array.Items)");
                isb.OpenBrace(); // foreach

                switch (prop.ElementSaveDataType)
                {
                    case SaveDataType.String:
                        isb.AppendLine("if (item is PdxString scalarString)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarString.Value);"); // Indent manually inside simple if
                        break;
                    case SaveDataType.Int:
                        isb.AppendLine("if (item is PdxInt scalarInt)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarInt.Value);");
                        break;
                    case SaveDataType.Long:
                        isb.AppendLine("if (item is PdxLong scalarLong)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarLong.Value);");
                        break;
                    case SaveDataType.Float:
                        isb.AppendLine("if (item is PdxFloat scalarFloat)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarFloat.Value);");
                        break;
                    case SaveDataType.Bool:
                        isb.AppendLine("if (item is PdxBool scalarBool)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarBool.Value);");
                        break;
                    case SaveDataType.DateTime:
                        isb.AppendLine("if (item is PdxDate scalarDateTime)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarDateTime.Value);");
                        break;
                    case SaveDataType.Guid:
                        isb.AppendLine("if (item is PdxGuid scalarGuid)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarGuid.Value);");
                        break;
                    case SaveDataType.Object:
                        isb.AppendLine("if (item is PdxObject objItem)");
                        isb.OpenBrace(); // if is PdxObject
                        isb.AppendLine($"var newObj = {listElementType}.Load(objItem);");
                        isb.AppendLine($"{targetProperty}.Add(newObj);");
                        isb.CloseBrace(); // end if is PdxObject
                        break;
                    default:
                        isb.AppendLine("if (item is PdxString scalarString)");
                        isb.OpenBrace();
                        isb.AppendLine(
                            $"try {{ {targetProperty}.Add(({listElementType}{listElementNullableMarker})Convert.ChangeType(scalarString.Value, typeof({listElementType}))); }} catch {{ /* Handle conversion error? */ }} // Fallback conversion"
                        );

                        isb.CloseBrace();
                        isb.AppendLine("// TODO: Add handling for other scalar types if needed for fallback conversion");
                        break;
                }

                isb.CloseBrace(); // End foreach loop
                isb.CloseBrace(); // End if TryGetPdxArray
                break;

            // --- Dictionary Types ---
            case SaveDataType.DictionaryNumericKey:
            case SaveDataType.DictionaryIntKey:
            {
                string keyType = prop.SaveDataType == SaveDataType.DictionaryIntKey ? "int" : "long";
                string valueTypeNumeric = ExtractDictionaryValueType(prop.Type);
                string valueTypeNumericNullableMarker = valueTypeNumeric.EndsWith("?") ? "?" : "";
                valueTypeNumeric = valueTypeNumeric.TrimEnd('?');

                isb.AppendLine($"if (pdxObject.TryGetPdxObject({keyNameLiteral}, out var {varName}Dict))");
                isb.OpenBrace(); // if TryGetPdxObject
                isb.AppendLine($"{targetProperty} = new {prop.Type.TrimEnd('?')}();");
                isb.AppendLine($"foreach (var kvp in {varName}Dict.Properties)");
                isb.OpenBrace(); // foreach kvp
                isb.AppendLine($"if ({keyType}.TryParse(kvp.Key, out var key))");
                isb.OpenBrace(); // if TryParse key

                switch (prop.ElementSaveDataType)
                {
                    case SaveDataType.String:
                        isb.AppendLine("if (kvp.Value is PdxString scalarString)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarString.Value);");
                        break;
                    case SaveDataType.Int:
                        isb.AppendLine("if (kvp.Value is PdxInt scalarInt)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarInt.Value);");
                        break;
                    case SaveDataType.Long:
                        isb.AppendLine("if (kvp.Value is PdxLong scalarLong)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarLong.Value);");
                        break;
                    case SaveDataType.Float:
                        isb.AppendLine("if (kvp.Value is PdxFloat scalarFloat)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarFloat.Value);");
                        break;
                    case SaveDataType.Bool:
                        isb.AppendLine("if (kvp.Value is PdxBool scalarBool)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarBool.Value);");
                        break;
                    case SaveDataType.DateTime:
                        isb.AppendLine("if (kvp.Value is PdxDate scalarDateTime)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarDateTime.Value);");
                        break;
                    case SaveDataType.Guid:
                        isb.AppendLine("if (kvp.Value is PdxGuid scalarGuid)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarGuid.Value);");
                        break;
                    case SaveDataType.Object:
                        isb.AppendLine("if (kvp.Value is PdxObject objItem)");
                        isb.OpenBrace();
                        isb.AppendLine($"var newObj = {valueTypeNumeric}.Load(objItem);");
                        isb.AppendLine($"{targetProperty}.Add(key, newObj);");
                        isb.CloseBrace();
                        break;
                    default:
                        isb.AppendLine("if (kvp.Value is PdxString scalarString)");
                        isb.OpenBrace();
                        isb.AppendLine(
                            $"try {{ {targetProperty}.Add(key, ({valueTypeNumeric}{valueTypeNumericNullableMarker})Convert.ChangeType(scalarString.Value, typeof({valueTypeNumeric}))); }} catch {{}} // Fallback"
                        );

                        isb.CloseBrace();
                        break;
                }

                isb.CloseBrace(); // End if TryParse key
                isb.CloseBrace(); // End foreach kvp
                isb.CloseBrace(); // End if TryGetPdxObject
                break;
            }

            case SaveDataType.DictionaryScalarKeyObjectValue:
            {
                string scalarDictValueType = ExtractDictionaryValueType(prop.Type);
                scalarDictValueType = scalarDictValueType.TrimEnd('?');
                string scalarDictKeyType = prop.KeySaveDataType == SaveDataType.Long ? "long" : "int";

                isb.AppendLine($"if (pdxObject.TryGetPdxArray({keyNameLiteral}, out var {varName}Pairs))");
                isb.OpenBrace(); // if TryGetPdxArray
                isb.AppendLine($"{targetProperty} = new {prop.Type.TrimEnd('?')}();");
                isb.AppendLine($"foreach (var item in {varName}Pairs.Items)");
                isb.OpenBrace(); // foreach item
                isb.AppendLine("if (item is PdxArray pairArray)");
                isb.OpenBrace(); // if is pair array
                isb.AppendLine("// Check if array has at least two items");
                isb.AppendLine("var itemsArray = pairArray.Items;");
                isb.AppendLine("if (itemsArray != null && itemsArray.Length >= 2)");
                isb.OpenBrace(); // if has at least 2 items
                isb.AppendLine("var keyElement = itemsArray[0];");
                isb.AppendLine("var valueElement = itemsArray[1];");
                isb.AppendLine($"if (keyElement is {(prop.KeySaveDataType == SaveDataType.Long ? "PdxLong" : "PdxInt")} keyScalar && valueElement is PdxObject valueObj)");
                isb.OpenBrace(); // if key/value match expected types
                isb.AppendLine($"var newObj = {scalarDictValueType}.Load(valueObj);");
                isb.AppendLine($"{targetProperty}.Add(keyScalar.Value, newObj);");
                isb.CloseBrace(); // end if key/value match
                isb.CloseBrace(); // end if has at least 2 items
                isb.CloseBrace(); // end if is pair array
                isb.CloseBrace(); // end foreach item
                isb.CloseBrace(); // end if TryGetPdxArray
                break;
            }

            case SaveDataType.DictionaryStringKey:
            {
                string stringDictValueType = ExtractDictionaryValueType(prop.Type);
                string stringDictValueTypeNullableMarker = stringDictValueType.EndsWith("?") ? "?" : "";
                stringDictValueType = stringDictValueType.TrimEnd('?');

                isb.AppendLine($"if (pdxObject.TryGetPdxObject({keyNameLiteral}, out var {varName}StrDict))");
                isb.OpenBrace(); // if TryGetPdxObject
                isb.AppendLine($"{targetProperty} = new {prop.Type.TrimEnd('?')}();");
                isb.AppendLine($"foreach (var kvp in {varName}StrDict.Properties)");
                isb.OpenBrace(); // foreach kvp

                if (prop.ElementSaveDataType == SaveDataType.Object)
                {
                    isb.AppendLine("if (kvp.Value is PdxObject objItem)");
                    isb.OpenBrace();
                    isb.AppendLine($"var newObj = {stringDictValueType}.Load(objItem);");
                    isb.AppendLine($"{targetProperty}.Add(kvp.Key, newObj);");
                    isb.CloseBrace();
                }
                else // Handle scalar values
                {
                    string expectedScalarType = GetScalarTypeName(prop.ElementSaveDataType);

                    if (expectedScalarType != "object")
                    {
                        isb.AppendLine($"if (kvp.Value is Scalar<{expectedScalarType}> scalarValue)");
                        isb.AppendLine($"    {targetProperty}.Add(kvp.Key, scalarValue.Value);");
                    }
                    else
                    {
                        isb.AppendLine("if (kvp.Value is PdxString scalarString)");
                        isb.OpenBrace();
                        isb.AppendLine(
                            $"try {{ {targetProperty}.Add(kvp.Key, ({stringDictValueType}{stringDictValueTypeNullableMarker})Convert.ChangeType(scalarString.Value, typeof({stringDictValueType}))); }} catch {{}} // Fallback"
                        );

                        isb.CloseBrace();
                    }
                }

                isb.CloseBrace(); // End foreach
                isb.CloseBrace(); // End if TryGetPdxObject
                break;
            }

            // --- Repeated Key Types --- 
            case SaveDataType.RepeatedKeyStringList:
            case SaveDataType.FlatRepeatedKeyList:
            {
                isb.AppendLine($"{targetProperty} = pdxObject.Properties");
                isb.Indent(); // Start LINQ expression indent
                isb.AppendLine($".Where(kvp => kvp.Key == {keyNameLiteral} && kvp.Value is PdxString)");
                isb.AppendLine(".Select(kvp => ((PdxString)kvp.Value).Value)");
                isb.AppendLine(".ToList();");
                isb.Unindent(); // End LINQ expression indent
                break;
            }

            case SaveDataType.FlatRepeatedObjectList:
            {
                // Don't use nullable type references in the LINQ query
                string flatObjListElementType = prop.Type.TrimEnd('?').Replace("List<", "").Replace(">", "").TrimEnd('?');
                isb.AppendLine($"{targetProperty} = pdxObject.Properties");
                isb.Indent();
                isb.AppendLine($".Where(kvp => kvp.Key == {keyNameLiteral} && kvp.Value is PdxObject)");
                isb.AppendLine($".Select(kvp => {flatObjListElementType}.Load((PdxObject)kvp.Value))");
                isb.AppendLine(".ToList();");
                isb.Unindent();
                break;
            }

            case SaveDataType.RepeatedArrayList:
                isb.AppendLine($"{targetProperty} = pdxObject.Properties");
                isb.Indent();
                isb.AppendLine($".Where(kvp => kvp.Key == {keyNameLiteral} && kvp.Value is PdxArray)");
                isb.AppendLine(".Select(kvp => ((PdxArray)kvp.Value).Items");
                isb.Indent(); // Indent for inner LINQ
                isb.AppendLine(".OfType<PdxString>()");
                isb.AppendLine(".Select(s => s.Value)");
                isb.AppendLine(".ToList())");
                isb.Unindent(); // Unindent inner LINQ
                isb.AppendLine(".ToList();");
                isb.Unindent(); // Unindent outer LINQ
                break;

            default:
                isb.AppendLine($"// Unhandled property type: {prop.SaveDataType} for {prop.PropertyName}");
                break;
        }
    }

    // --- Helpers ---
    static bool IsRepeatedKeyType(SaveDataType type)
    {
        return type is SaveDataType.RepeatedKeyStringList
            or SaveDataType.FlatRepeatedKeyList
            or SaveDataType.RepeatedArrayList
            or SaveDataType.FlatRepeatedObjectList;
    }

    static bool IsDictionaryType(SaveDataType type)
    {
        return type is SaveDataType.DictionaryIntKey or
            SaveDataType.DictionaryNumericKey or
            SaveDataType.DictionaryStringKey or
            SaveDataType.DictionaryScalarKeyObjectValue;
    }

    static string GetTryGetMethodName(SaveDataType dataType) => dataType switch
    {
        SaveDataType.String => "TryGetString",
        SaveDataType.Int => "TryGetInt",
        SaveDataType.Long => "TryGetLong",
        SaveDataType.Float => "TryGetFloat",
        SaveDataType.Bool => "TryGetBool",
        SaveDataType.DateTime => "TryGetDateTime",
        SaveDataType.Guid => "TryGetGuid",
        SaveDataType.DictionaryNumericKey => string.Empty, // Not used directly with TryGet
        SaveDataType.DictionaryStringKey => string.Empty, // Not used directly with TryGet
        SaveDataType.DictionaryScalarKeyObjectValue => string.Empty, // Not used directly with TryGet
        _ => throw new ArgumentOutOfRangeException(nameof(dataType), $"No corresponding TryGet method for {dataType}")
    };

    static string GetScalarTypeName(SaveDataType dataType) => dataType switch
    {
        SaveDataType.String => "string",
        SaveDataType.Int => "int",
        SaveDataType.Long => "long",
        SaveDataType.Float => "float",
        SaveDataType.Bool => "bool",
        SaveDataType.DateTime => "DateTime",
        SaveDataType.Guid => "Guid",
        _ => "object" // Fallback
    };

    static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return "_";
        // Replace invalid chars, normalize separators, handle potential leading digits
        string sanitized = Regex.Replace(input, @"[^\w\s_-]", ""); // Remove chars not word, whitespace, underscore, hyphen
        sanitized = Regex.Replace(sanitized, @"[ \s_-]+", " ").Trim(); // Normalize separators to space
        if (string.IsNullOrEmpty(sanitized)) return "_";
        if (char.IsDigit(sanitized[0])) sanitized = "_" + sanitized; // Prepend underscore if starts with digit

        string[] parts = sanitized.Split(new[]
            {
                ' '
            }, StringSplitOptions.RemoveEmptyEntries
        );

        StringBuilder pascalCase = new StringBuilder();

        foreach (string part in parts)
        {
            if (part.Length > 0)
            {
                pascalCase.Append(char.ToUpperInvariant(part[0]));
                // Append the rest of the part without forcing lowercase
                pascalCase.Append(part.Substring(1));
            }
        }

        string result = pascalCase.ToString();
        if (result.Length == 0) return "_";

        // Check for C# keywords using SyntaxFacts
        if (SyntaxFacts.IsKeywordKind(SyntaxFacts.GetKeywordKind(result)) ||
            SyntaxFacts.IsContextualKeyword(SyntaxFacts.GetContextualKeywordKind(result)))
        {
            result = "@" + result;
        }

        return result;
    }

    static string ToCamelCase(string input)
    {
        // Special case for PdxObject to avoid capitalization issues in generated code
        if (input == "PdxObject")
        {
            return "pdxObject";
        }

        string pascal = ToPascalCase(input);
        if (string.IsNullOrEmpty(pascal) || pascal == "_") return "_var";

        string result;

        if (pascal.StartsWith("@"))
        {
            // Input was already a keyword like @Class
            // Make it @class
            if (pascal.Length > 1)
            {
                result = "@" + char.ToLowerInvariant(pascal[1]) + pascal.Substring(2);
            }
            else
            {
                result = pascal; // Should be just "@", unlikely
            }
        }
        // Standard camelCase conversion (e.g., "MyProperty" -> "myProperty")
        else if (pascal.Length > 0 && char.IsUpper(pascal[0]))
        {
            result = char.ToLowerInvariant(pascal[0]) + pascal.Substring(1);
        }
        else
        {
            result = pascal; // Already camelCase or starts with underscore
        }

        // *** Add keyword check for the final camelCase result ***
        // Check if the result (e.g., "class") is a keyword, ignoring the @ if it was added previously
        string checkName = result.StartsWith("@") ? result.Substring(1) : result;

        if (!result.StartsWith("@") && // Don't double-add @
            (SyntaxFacts.IsKeywordKind(SyntaxFacts.GetKeywordKind(checkName)) ||
             SyntaxFacts.IsContextualKeyword(SyntaxFacts.GetContextualKeywordKind(checkName))))
        {
            result = "@" + result;
        }

        return result;
    }

    static string GenerateUniqueClassName(string parentName, string basePropertyName, IEnumerable<string> existingNames)
    {
        // parentName is already the PascalCased name of the containing class.
        // Strip any numeric suffixes from the parent name that might have leaked in
        string cleanParentName = Regex.Replace(parentName, @"_\d+", "");
        
        // Clean up the property name
        string propertyName = basePropertyName;
        
        // IMPORTANT: Check if this is a numeric key (likely a dictionary/array entry)
        // If it's purely numeric, we use a common "Entry" name instead of the number
        bool isNumericKey = Regex.IsMatch(propertyName, @"^\d+$");
        if (isNumericKey)
        {
            propertyName = "Entry";
        }
        else
        {
            // Only for non-numeric keys - remove any numeric suffixes that might have leaked in
            propertyName = Regex.Replace(propertyName, @"_\d+$", "");
        }
        
        // Remove plural forms to get the singular item name
        if (propertyName.EndsWith("s") && propertyName.Length > 1)
        {
            propertyName = propertyName.Substring(0, propertyName.Length - 1);
        }
        
        // Special cases for "Item" suffix
        if (propertyName.EndsWith("Item"))
        {
            propertyName = propertyName.Substring(0, propertyName.Length - 4);
        }
        
        // For empty or overly generic names, use a reasonable default
        if (string.IsNullOrWhiteSpace(propertyName) || propertyName.Equals("Item", StringComparison.OrdinalIgnoreCase))
        {
            propertyName = "Entry";
        }
        
        // Concatenate cleaned parent class name + PascalCased property name
        string baseName = cleanParentName + ToPascalCase(propertyName);
        string uniqueName = baseName;
        
        // If there's a collision, use a type-based suffix rather than a numeric one
        int counter = 1;
        var currentNames = new HashSet<string>(existingNames);

        // Only use non-numeric suffix as a last resort for collisions
        while (currentNames.Contains(uniqueName))
        {
            uniqueName = baseName + "Type" + counter++; // Use "Type1", "Type2" etc. instead of numbers
        }

        // Final check: ensure the generated unique name isn't a keyword
        if (SyntaxFacts.IsKeywordKind(SyntaxFacts.GetKeywordKind(uniqueName)) ||
            SyntaxFacts.IsContextualKeyword(SyntaxFacts.GetContextualKeywordKind(uniqueName)))
        {
            uniqueName = "@" + uniqueName;
        }

        return uniqueName;
    }

    static string GenerateArrayElementClassName(Schema containingSchema, string propertyName)
    {
        // Clean the containing schema name first to remove any numeric suffixes
        string cleanContainingName = Regex.Replace(containingSchema.Name, @"_\d+", "");
        
        // Clean the property name 
        string cleanPropertyName;
        
        // IMPORTANT: Check if this is a numeric key (likely a dictionary/array entry)
        // If it's purely numeric, we use a common "Entry" name instead of the number
        if (Regex.IsMatch(propertyName, @"^\d+$"))
        {
            cleanPropertyName = "Entry";
        }
        else
        {
            cleanPropertyName = Regex.Replace(propertyName, @"_\d+$", "");
            
            // Convert plural to singular if needed
            if (cleanPropertyName.EndsWith("s") && cleanPropertyName.Length > 1)
            {
                // Special cases for common irregular plurals
                if (cleanPropertyName.EndsWith("ies"))
                {
                    cleanPropertyName = cleanPropertyName.Substring(0, cleanPropertyName.Length - 3) + "y";
                }
                else if (cleanPropertyName.EndsWith("ses") || 
                        cleanPropertyName.EndsWith("zes") ||
                        cleanPropertyName.EndsWith("xes") ||
                        cleanPropertyName.EndsWith("ches") ||
                        cleanPropertyName.EndsWith("shes"))
                {
                    cleanPropertyName = cleanPropertyName.Substring(0, cleanPropertyName.Length - 2);
                }
                else 
                {
                    cleanPropertyName = cleanPropertyName.Substring(0, cleanPropertyName.Length - 1);
                }
            }
        }
        
        // Create a stable schema-only class name
        return cleanContainingName + ToPascalCase(cleanPropertyName);
    }

    static string ToCSharpStringLiteral(string value)
    {
        // Escape double quotes by doubling them for a verbatim string literal
        string escapedValue = value.Replace("\"", "\"\"");
        return $"@\"{escapedValue}\"";
    }

    // --- Data Structures ---

    public class Schema
    {
        public string Name { get; set; } = "";
        public List<SchemaProperty> Properties { get; set; } = new List<SchemaProperty>();
    }

    public class SchemaProperty
    {
        public string KeyName { get; set; } = "";
        public string PropertyName { get; set; } = "";
        public string Type { get; set; } = "object";
        public bool IsNullable { get; set; } = true;
        public SaveDataType SaveDataType { get; set; } = SaveDataType.Unknown;
        public SaveDataType ElementSaveDataType { get; set; } = SaveDataType.Unknown; // For arrays/lists

        public SaveDataType KeySaveDataType { get; set; } = SaveDataType.Unknown; // For dictionaries with non-string keys

        //public bool HasDictionaryNullValues { get; set; } = false; // Specifically for dictionaries, indicates if value type should be T?
        public Schema? NestedSchema { get; set; } // For objects or elements/values that are objects
        //public Schema? SpecializedCollectionSchema { get; set; } // For specialized collection types
        //public string RepeatedKey { get; set; } = ""; // For serialization of repeated key patterns like trait="value1" trait="value2"

        // New properties for attribute-based serialization
        //public bool HasCustomAttributes { get; set; } = false; // Whether to add custom attributes
        //public bool IsArrayValue { get; set; } = false; // Whether this is an array value for repeated keys

        // Track observed types for numeric properties to enable better type promotion
        public HashSet<SaveDataType> ObservedNumericTypes { get; } = new HashSet<SaveDataType>();

        // Add a type to the observations and promote the property type if needed
        public void AddObservedNumericType(SaveDataType type)
        {
            if (type != SaveDataType.Int && type != SaveDataType.Long && type != SaveDataType.Float)
                return;

            ObservedNumericTypes.Add(type);

            // Apply type promotion if needed
            if (type == SaveDataType.Float || SaveDataType == SaveDataType.Float)
            {
                // Float is highest priority
                SaveDataType = SaveDataType.Float;
                Type = "float";
            }
            else if ((type == SaveDataType.Long && SaveDataType == SaveDataType.Int) ||
                     (type == SaveDataType.Int && SaveDataType == SaveDataType.Long) ||
                     SaveDataType == SaveDataType.Long)
            {
                // Long is second priority
                SaveDataType = SaveDataType.Long;
                Type = "long";
            }
            else if (SaveDataType == SaveDataType.Unknown || SaveDataType == SaveDataType.String)
            {
                // First numeric type encountered
                SaveDataType = type;
                Type = GetTypeName(type);
            }
        }

        string GetTypeName(SaveDataType type) => type switch
        {
            SaveDataType.Int => "int",
            SaveDataType.Long => "long",
            SaveDataType.Float => "float",
            _ => "object"
        };
    }

    public class ClassSchemaPair
    {
        public ClassDeclarationSyntax? ClassDeclaration { get; set; }
        public string? SchemaFileName { get; set; }
    }

    class RepeatedKeyInfo
    {
        public string PropertyName { get; set; } = "";
        public SchemaProperty? SchemaProperty { get; set; }
    }

    static string ExtractDictionaryValueType(string dictionaryType)
    {
        // Example input: "Dictionary<long, GamestateBuildingEntry>"
        // Expected output: "GamestateBuildingEntry"
        
        // Try to find the second generic parameter
        int startIndex = dictionaryType.IndexOf(',');
        if (startIndex < 0) return "object"; // Not a valid dictionary type
        
        startIndex += 1; // Skip comma
        
        // Find the closing angle bracket
        int endIndex = dictionaryType.LastIndexOf('>');
        if (endIndex < 0) return "object";
        
        // Extract and trim the value type
        string valueType = dictionaryType.Substring(startIndex, endIndex - startIndex).Trim();
        
        // Remove any nullability marker
        return valueType.TrimEnd('?');
    }
    
    static void GeneratePropertySerialization(IndentedStringBuilder isb, SchemaProperty prop, string propertiesVar, string? overrideTarget = null)
    {
        string targetObject = overrideTarget ?? "this";
        string propertyName = prop.PropertyName;
        string keyName = prop.KeyName;
        string propertyAccess = $"{targetObject}.{propertyName}";
        string keyNameLiteral = $"@\"{keyName}\"";
        
        // Special handling for dictionary properties with numeric keys
        if (prop.SaveDataType == SaveDataType.DictionaryIntKey || prop.SaveDataType == SaveDataType.DictionaryLongKey)
        {
            isb.AppendLine($"if ({propertyAccess} != null && {propertyAccess}.Count > 0)");
            isb.OpenBrace(); // if dictionary not null/empty
            
            // Serialize each dictionary entry as a separate property in the PdxObject
            isb.AppendLine($"foreach (var kvp in {propertyAccess})");
            isb.OpenBrace(); // foreach
            
            isb.AppendLine("if (kvp.Value != null)");
            isb.OpenBrace(); // if value not null
            
            // Convert the entry to PdxObject
            isb.AppendLine("var entryObj = kvp.Value.ToPdxObject();");
            
            // Add entry to properties list with numeric key
            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>(kvp.Key.ToString(), entryObj));");
            
            isb.CloseBrace(); // if value not null
            isb.CloseBrace(); // foreach
            isb.CloseBrace(); // if dictionary not null/empty
            isb.AppendLine();
            return;
        }
        
        string varName = ToCamelCase(prop.PropertyName);
        string typeWithoutNullable = prop.Type.TrimEnd('?');

        switch (prop.SaveDataType)
        {
            case SaveDataType.String:
                isb.AppendLine($"if (pdxObject.TryGetString({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{propertyAccess} = {varName};");
                isb.CloseBrace();
                break;
            case SaveDataType.Int:
                isb.AppendLine($"if (pdxObject.TryGetInt({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{propertyAccess} = {varName};");
                isb.CloseBrace();
                break;
            case SaveDataType.Long:
                isb.AppendLine($"if (pdxObject.TryGetLong({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{propertyAccess} = {varName};");
                isb.CloseBrace();
                break;
            case SaveDataType.Float:
                isb.AppendLine($"if (pdxObject.TryGetFloat({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{propertyAccess} = {varName};");
                isb.CloseBrace();
                break;
            case SaveDataType.Bool:
                isb.AppendLine($"if (pdxObject.TryGetBool({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{propertyAccess} = {varName};");
                isb.CloseBrace();
                break;
            case SaveDataType.DateTime:
                isb.AppendLine($"if (pdxObject.TryGetDateTime({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                // Add .Value for non-nullable DateTime property
                string datetimeAssignment = prop.IsNullable ? $"{propertyAccess} = {varName};" : $"{propertyAccess} = {varName}.Value;";
                isb.AppendLine(datetimeAssignment);
                isb.CloseBrace();
                break;
            case SaveDataType.Guid:
                isb.AppendLine($"if (pdxObject.TryGetGuid({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                string guidAssignment = prop.IsNullable ? $"{propertyAccess} = {varName};" : $"{propertyAccess} = {varName}.Value;";
                isb.AppendLine(guidAssignment);
                isb.CloseBrace();
                break;
            case SaveDataType.Object:
                isb.AppendLine($"if (pdxObject.TryGetPdxObject({keyNameLiteral}, out var {varName}Obj))");
                isb.OpenBrace();
                isb.AppendLine($"{propertyAccess} = {typeWithoutNullable}.Load({varName}Obj);");
                isb.CloseBrace();
                break;
            case SaveDataType.Array:
                string listElementType = prop.Type.TrimEnd('?').Replace("List<", "").Replace(">", "");
                string listElementNullableMarker = listElementType.EndsWith("?") ? "?" : "";
                listElementType = listElementType.TrimEnd('?');

                isb.AppendLine($"if (pdxObject.TryGetPdxArray({keyNameLiteral}, out var {varName}Array))");
                isb.OpenBrace(); // if TryGetPdxArray
                isb.AppendLine($"{propertyAccess} = new List<{listElementType}{listElementNullableMarker}>();");
                isb.AppendLine($"foreach (var item in {varName}Array.Items)");
                isb.OpenBrace(); // foreach

                switch (prop.ElementSaveDataType)
                {
                    case SaveDataType.String:
                        isb.AppendLine("if (item is PdxString scalarString)");
                        isb.AppendLine($"    {propertyAccess}.Add(scalarString.Value);"); // Indent manually inside simple if
                        break;
                    case SaveDataType.Int:
                        isb.AppendLine("if (item is PdxInt scalarInt)");
                        isb.AppendLine($"    {propertyAccess}.Add(scalarInt.Value);");
                        break;
                    case SaveDataType.Long:
                        isb.AppendLine("if (item is PdxLong scalarLong)");
                        isb.AppendLine($"    {propertyAccess}.Add(scalarLong.Value);");
                        break;
                    case SaveDataType.Float:
                        isb.AppendLine("if (item is PdxFloat scalarFloat)");
                        isb.AppendLine($"    {propertyAccess}.Add(scalarFloat.Value);");
                        break;
                    case SaveDataType.Bool:
                        isb.AppendLine("if (item is PdxBool scalarBool)");
                        isb.AppendLine($"    {propertyAccess}.Add(scalarBool.Value);");
                        break;
                    case SaveDataType.DateTime:
                        isb.AppendLine("if (item is PdxDate scalarDateTime)");
                        isb.AppendLine($"    {propertyAccess}.Add(scalarDateTime.Value);");
                        break;
                    case SaveDataType.Guid:
                        isb.AppendLine("if (item is PdxGuid scalarGuid)");
                        isb.AppendLine($"    {propertyAccess}.Add(scalarGuid.Value);");
                        break;
                    case SaveDataType.Object:
                        isb.AppendLine("if (item is PdxObject objItem)");
                        isb.OpenBrace(); // if is PdxObject
                        isb.AppendLine($"var newObj = {listElementType}.Load(objItem);");
                        isb.AppendLine($"{propertyAccess}.Add(newObj);");
                        isb.CloseBrace(); // end if is PdxObject
                        break;
                    default:
                        isb.AppendLine("if (item is PdxString scalarString)");
                        isb.OpenBrace();
                        isb.AppendLine(
                            $"try {{ {propertyAccess}.Add(({listElementType}{listElementNullableMarker})Convert.ChangeType(scalarString.Value, typeof({listElementType}))); }} catch {{ /* Handle conversion error? */ }} // Fallback conversion"
                        );

                        isb.CloseBrace();
                        isb.AppendLine("// TODO: Add handling for other scalar types if needed for fallback conversion");
                        break;
                }

                isb.CloseBrace(); // End foreach loop
                isb.CloseBrace(); // End if TryGetPdxArray
                break;

            // --- Dictionary Types ---
            case SaveDataType.DictionaryNumericKey:
            case SaveDataType.DictionaryIntKey:
            {
                string keyType = prop.SaveDataType == SaveDataType.DictionaryIntKey ? "int" : "long";
                string valueTypeNumeric = ExtractDictionaryValueType(prop.Type);
                string valueTypeNumericNullableMarker = valueTypeNumeric.EndsWith("?") ? "?" : "";
                valueTypeNumeric = valueTypeNumeric.TrimEnd('?');

                isb.AppendLine($"if (pdxObject.TryGetPdxObject({keyNameLiteral}, out var {varName}Dict))");
                isb.OpenBrace(); // if TryGetPdxObject
                isb.AppendLine($"{propertyAccess} = new {prop.Type.TrimEnd('?')}();");
                isb.AppendLine($"foreach (var kvp in {varName}Dict.Properties)");
                isb.OpenBrace(); // foreach kvp
                isb.AppendLine($"if ({keyType}.TryParse(kvp.Key, out var key))");
                isb.OpenBrace(); // if TryParse key

                switch (prop.ElementSaveDataType)
                {
                    case SaveDataType.String:
                        isb.AppendLine("if (kvp.Value is PdxString scalarString)");
                        isb.AppendLine($"    {propertyAccess}.Add(key, scalarString.Value);");
                        break;
                    case SaveDataType.Int:
                        isb.AppendLine("if (kvp.Value is PdxInt scalarInt)");
                        isb.AppendLine($"    {propertyAccess}.Add(key, scalarInt.Value);");
                        break;
                    case SaveDataType.Long:
                        isb.AppendLine("if (kvp.Value is PdxLong scalarLong)");
                        isb.AppendLine($"    {propertyAccess}.Add(key, scalarLong.Value);");
                        break;
                    case SaveDataType.Float:
                        isb.AppendLine("if (kvp.Value is PdxFloat scalarFloat)");
                        isb.AppendLine($"    {propertyAccess}.Add(key, scalarFloat.Value);");
                        break;
                    case SaveDataType.Bool:
                        isb.AppendLine("if (kvp.Value is PdxBool scalarBool)");
                        isb.AppendLine($"    {propertyAccess}.Add(key, scalarBool.Value);");
                        break;
                    case SaveDataType.DateTime:
                        isb.AppendLine("if (kvp.Value is PdxDate scalarDateTime)");
                        isb.AppendLine($"    {propertyAccess}.Add(key, scalarDateTime.Value);");
                        break;
                    case SaveDataType.Guid:
                        isb.AppendLine("if (kvp.Value is PdxGuid scalarGuid)");
                        isb.AppendLine($"    {propertyAccess}.Add(key, scalarGuid.Value);");
                        break;
                    case SaveDataType.Object:
                        isb.AppendLine("if (kvp.Value is PdxObject objItem)");
                        isb.OpenBrace();
                        isb.AppendLine($"var newObj = {valueTypeNumeric}.Load(objItem);");
                        isb.AppendLine($"{propertyAccess}.Add(key, newObj);");
                        isb.CloseBrace();
                        break;
                    default:
                        isb.AppendLine("if (kvp.Value is PdxString scalarString)");
                        isb.OpenBrace();
                        isb.AppendLine(
                            $"try {{ {propertyAccess}.Add(key, ({valueTypeNumeric}{valueTypeNumericNullableMarker})Convert.ChangeType(scalarString.Value, typeof({valueTypeNumeric}))); }} catch {{}} // Fallback"
                        );

                        isb.CloseBrace();
                        break;
                }

                isb.CloseBrace(); // End if TryParse key
                isb.CloseBrace(); // End foreach kvp
                isb.CloseBrace(); // End if TryGetPdxObject
                break;
            }

            case SaveDataType.DictionaryScalarKeyObjectValue:
            {
                string scalarDictValueType = ExtractDictionaryValueType(prop.Type);
                scalarDictValueType = scalarDictValueType.TrimEnd('?');
                string scalarDictKeyType = prop.KeySaveDataType == SaveDataType.Long ? "long" : "int";

                isb.AppendLine($"if (pdxObject.TryGetPdxArray({keyNameLiteral}, out var {varName}Pairs))");
                isb.OpenBrace(); // if TryGetPdxArray
                isb.AppendLine($"{propertyAccess} = new {prop.Type.TrimEnd('?')}();");
                isb.AppendLine($"foreach (var item in {varName}Pairs.Items)");
                isb.OpenBrace(); // foreach item
                isb.AppendLine("if (item is PdxArray pairArray)");
                isb.OpenBrace(); // if is pair array
                isb.AppendLine("// Check if array has at least two items");
                isb.AppendLine("var itemsArray = pairArray.Items;");
                isb.AppendLine("if (itemsArray != null && itemsArray.Length >= 2)");
                isb.OpenBrace(); // if has at least 2 items
                isb.AppendLine("var keyElement = itemsArray[0];");
                isb.AppendLine("var valueElement = itemsArray[1];");
                isb.AppendLine($"if (keyElement is {(prop.KeySaveDataType == SaveDataType.Long ? "PdxLong" : "PdxInt")} keyScalar && valueElement is PdxObject valueObj)");
                isb.OpenBrace(); // if key/value match expected types
                isb.AppendLine($"var newObj = {scalarDictValueType}.Load(valueObj);");
                isb.AppendLine($"{propertyAccess}.Add(keyScalar.Value, newObj);");
                isb.CloseBrace(); // end if key/value match
                isb.CloseBrace(); // end if has at least 2 items
                isb.CloseBrace(); // end if is pair array
                isb.CloseBrace(); // end foreach item
                isb.CloseBrace(); // end if TryGetPdxArray
                break;
            }

            case SaveDataType.DictionaryStringKey:
            {
                string stringDictValueType = ExtractDictionaryValueType(prop.Type);
                string stringDictValueTypeNullableMarker = stringDictValueType.EndsWith("?") ? "?" : "";
                stringDictValueType = stringDictValueType.TrimEnd('?');

                isb.AppendLine($"if (pdxObject.TryGetPdxObject({keyNameLiteral}, out var {varName}StrDict))");
                isb.OpenBrace(); // if TryGetPdxObject
                isb.AppendLine($"{propertyAccess} = new {prop.Type.TrimEnd('?')}();");
                isb.AppendLine($"foreach (var kvp in {varName}StrDict.Properties)");
                isb.OpenBrace(); // foreach kvp

                if (prop.ElementSaveDataType == SaveDataType.Object)
                {
                    isb.AppendLine("if (kvp.Value is PdxObject objItem)");
                    isb.OpenBrace();
                    isb.AppendLine($"var newObj = {stringDictValueType}.Load(objItem);");
                    isb.AppendLine($"{propertyAccess}.Add(kvp.Key, newObj);");
                    isb.CloseBrace();
                }
                else // Handle scalar values
                {
                    string expectedScalarType = GetScalarTypeName(prop.ElementSaveDataType);

                    if (expectedScalarType != "object")
                    {
                        isb.AppendLine($"if (kvp.Value is Scalar<{expectedScalarType}> scalarValue)");
                        isb.AppendLine($"    {propertyAccess}.Add(kvp.Key, scalarValue.Value);");
                    }
                    else
                    {
                        isb.AppendLine("if (kvp.Value is PdxString scalarString)");
                        isb.OpenBrace();
                        isb.AppendLine(
                            $"try {{ {propertyAccess}.Add(kvp.Key, ({stringDictValueType}{stringDictValueTypeNullableMarker})Convert.ChangeType(scalarString.Value, typeof({stringDictValueType}))); }} catch {{}} // Fallback"
                        );

                        isb.CloseBrace();
                    }
                }

                isb.CloseBrace(); // End foreach
                isb.CloseBrace(); // End if TryGetPdxObject
                break;
            }

            // --- Repeated Key Types --- 
            case SaveDataType.RepeatedKeyStringList:
            case SaveDataType.FlatRepeatedKeyList:
            {
                isb.AppendLine($"{propertyAccess} = pdxObject.Properties");
                isb.Indent(); // Start LINQ expression indent
                isb.AppendLine($".Where(kvp => kvp.Key == {keyNameLiteral} && kvp.Value is PdxString)");
                isb.AppendLine(".Select(kvp => ((PdxString)kvp.Value).Value)");
                isb.AppendLine(".ToList();");
                isb.Unindent(); // End LINQ expression indent
                break;
            }

            case SaveDataType.FlatRepeatedObjectList:
            {
                // Don't use nullable type references in the LINQ query
                string flatObjListElementType = prop.Type.TrimEnd('?').Replace("List<", "").Replace(">", "").TrimEnd('?');
                isb.AppendLine($"{propertyAccess} = pdxObject.Properties");
                isb.Indent();
                isb.AppendLine($".Where(kvp => kvp.Key == {keyNameLiteral} && kvp.Value is PdxObject)");
                isb.AppendLine($".Select(kvp => {flatObjListElementType}.Load((PdxObject)kvp.Value))");
                isb.AppendLine(".ToList();");
                isb.Unindent();
                break;
            }

            case SaveDataType.RepeatedArrayList:
                isb.AppendLine($"{propertyAccess} = pdxObject.Properties");
                isb.Indent();
                isb.AppendLine($".Where(kvp => kvp.Key == {keyNameLiteral} && kvp.Value is PdxArray)");
                isb.AppendLine(".Select(kvp => ((PdxArray)kvp.Value).Items");
                isb.Indent(); // Indent for inner LINQ
                isb.AppendLine(".OfType<PdxString>()");
                isb.AppendLine(".Select(s => s.Value)");
                isb.AppendLine(".ToList())");
                isb.Unindent(); // Unindent inner LINQ
                isb.AppendLine(".ToList();");
                isb.Unindent(); // Unindent outer LINQ
                break;

            default:
                isb.AppendLine($"// Unhandled property type: {prop.SaveDataType} for {prop.PropertyName}");
                break;
        }
    }

    // --- Code Generation ---

    static IndentedStringBuilder GenerateCSharpCode(
        List<Schema> schemas,
        string rootClassName,
        ClassDeclarationSyntax partialClass,
        Compilation compilation)
    {
        var isb = new IndentedStringBuilder();
        var syntaxTree = partialClass.SyntaxTree;
        var rootNode = syntaxTree.GetRoot();
        var usingDirectives = rootNode.DescendantNodes().OfType<UsingDirectiveSyntax>().ToArray();
        var namespaceDeclaration = partialClass.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();
        string? namespaceString = namespaceDeclaration?.Name.ToString();

        // Generate the code header
        isb.AppendLine("// <auto-generated/>");
        isb.AppendLine("#nullable enable");
        isb.AppendLine(); // Add a blank line

        // Add using directives from the original file
        foreach (var usingDirective in usingDirectives)
        {
            // AppendLine without indent for using directives
            isb.AppendLine(usingDirective.ToString().Trim());
        }

        // Add the System and MageeSoft.PDX.CE namespaces if not already present
        if (usingDirectives.All(u => u.Name!.ToString() != "System"))
            isb.AppendLine("using System;");

        if (usingDirectives.All(u => u.Name!.ToString() != "System.Collections.Generic"))
            isb.AppendLine("using System.Collections.Generic;");

        if (usingDirectives.All(u => u.Name!.ToString() != "System.Linq"))
            isb.AppendLine("using System.Linq;");

        isb.AppendLine();

        bool hasNamespace = !string.IsNullOrEmpty(namespaceString);

        if (hasNamespace)
        {
            // Use file-scoped namespace syntax (C# 10+)
            isb.AppendLine($"namespace {namespaceString};");
            isb.AppendLine();
        }

        Schema? rootSchema = schemas.FirstOrDefault(s => s.Name == rootClassName);

        if (rootSchema == null)
        {
            isb.AppendLine($"// ERROR: Root schema '{rootClassName}' not found.");
            return isb;
        }

        var generatedClassNames = new HashSet<string>();
        GenerateClassRecursive(isb, rootSchema, schemas, generatedClassNames, isRoot: true);

        return isb;
    }

    // Updated to accept IndentedStringBuilder
    static void GenerateClassRecursive(
        IndentedStringBuilder isb,
        Schema schema,
        List<Schema> allSchemas,
        HashSet<string> generatedClassNames,
        bool isRoot)
    {
        if (!generatedClassNames.Add(schema.Name)) return;

        // Normal class generation
        string classModifiers = isRoot ? "public partial class" : "public class";
        isb.AppendLine($"{classModifiers} {schema.Name}");
        isb.OpenBrace(); // Class opening brace

        // Properties
        foreach (var prop in schema.Properties.OrderBy(p => p.PropertyName))
        {
            string nullableMarker = prop.IsNullable ? "?" : "";
            // Determine initializer based on type (collections/arrays get initialized)
            string initializer = prop.Type.StartsWith("List<") || prop.Type.StartsWith("Dictionary<")
                ? " = new();"
                : "";

            isb.AppendLine($"/// <summary>Original key: {prop.KeyName}</summary>");

            isb.AppendLine($"public {prop.Type}{nullableMarker} {prop.PropertyName} {{ get; set; }}{initializer}");
            isb.AppendLine();
        }

        // Static Load method
        isb.AppendLine($"/// <summary>Loads data from a PdxObject into a new {schema.Name} instance.</summary>");
        isb.AppendLine($"public static {schema.Name} Load(PdxObject pdxObject)");
        isb.OpenBrace(); // Load method opening brace
        isb.AppendLine($"var model = new {schema.Name}();");
        isb.AppendLine();

        // Binding logic - Pass IndentedStringBuilder
        foreach (var prop in schema.Properties.OrderBy(p => p.PropertyName))
        {
            GeneratePropertyLoad(isb, prop, "model");
        }

        isb.AppendLine("return model;");
        isb.CloseBrace(); // Load method closing brace

        // Add ToPdxObject method for serialization
        isb.AppendLine();
        isb.AppendLine("/// <summary>Converts this model back to a PdxObject structure</summary>");
        isb.AppendLine("public PdxObject ToPdxObject()");
        isb.OpenBrace(); // ToPdxObject method opening brace
        isb.AppendLine("var properties = new List<KeyValuePair<string, IPdxElement>>();");
        // Create PdxObject with the list of properties
        isb.AppendLine("// Add properties to list");
        isb.AppendLine();

        // Generate serialization code for each property - Pass IndentedStringBuilder
        foreach (var prop in schema.Properties.OrderBy(p => p.PropertyName))
        {
            GeneratePropertySave(isb, prop, "properties");
        }

        isb.AppendLine("return properties.ToPdxObject();");
        isb.CloseBrace(); // ToPdxObject method closing brace

        isb.CloseBrace(); // Class closing brace
        isb.AppendLine(); // Add a blank line after the class

        // Generate nested classes as separate top-level classes
        var nestedSchemasToGenerate = schema.Properties
            .Where(p => p.NestedSchema != null)
            .Select(p => p.NestedSchema)
            .Distinct();

        foreach (var nestedSchema in nestedSchemasToGenerate.OrderBy(ns => ns!.Name))
        {
            GenerateClassRecursive(isb, nestedSchema!, allSchemas, generatedClassNames, isRoot: false);
        }
    }

    // Method to generate code for saving properties back to a PdxObject
    // Updated signature to accept IndentedStringBuilder
    static void GeneratePropertySave(IndentedStringBuilder isb, SchemaProperty prop, string propertiesVar)
    {
        // FIXME: There is a bug with the current implementation where return statements can be conditionally nested,
        // causing compile errors when certain conditions are not met. The proper fix is to ensure:
        // 1. All List and Dictionary operations add properties inside conditional blocks, but return statements
        //    should be outside all conditional blocks, at the end of the method.
        // 2. Class nesting should be properly closed with all required braces.
        // 3. Parameter names should be consistently lowercase to avoid shadowing.
        
        string propAccess = $"this.{prop.PropertyName}";
        string keyNameLiteral = ToCSharpStringLiteral(prop.KeyName);

        // Check if the property is null before trying to save it (for nullable types)
        bool needsNullCheck = prop.IsNullable &&
                              prop.SaveDataType != SaveDataType.Array && // Collections handle null checks internally
                              prop.SaveDataType != SaveDataType.DictionaryIntKey &&
                              prop.SaveDataType != SaveDataType.DictionaryNumericKey &&
                              prop.SaveDataType != SaveDataType.DictionaryStringKey &&
                              prop.SaveDataType != SaveDataType.DictionaryScalarKeyObjectValue &&
                              !IsRepeatedKeyType(prop.SaveDataType); // Repeated key types handled differently

        if (needsNullCheck)
        {
            isb.AppendLine($"if ({propAccess} != null)");
            isb.OpenBrace();
        }

        switch (prop.SaveDataType)
        {
            case SaveDataType.String:
            case SaveDataType.Bool:
            case SaveDataType.Int:
            case SaveDataType.Long:
            case SaveDataType.Float:
            case SaveDataType.DateTime:
            case SaveDataType.Guid:
            {
                string scalarValueAccess = (prop.SaveDataType != SaveDataType.String && prop.IsNullable) ? $"{propAccess}.Value" : propAccess;

                if (prop.SaveDataType == SaveDataType.String && prop.IsNullable && !needsNullCheck)
                {
                    isb.AppendLine($"if ({propAccess} != null)");
                    isb.OpenBrace();
                    isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxString({scalarValueAccess})));");
                    isb.CloseBrace();
                }
                else if (prop.IsNullable && !needsNullCheck && prop.SaveDataType != SaveDataType.String) // Other nullable value types not in main null check
                {
                    isb.AppendLine($"if ({propAccess}.HasValue)");
                    isb.OpenBrace();

                    // Use CE types
                    switch (prop.SaveDataType)
                    {
                        case SaveDataType.Int:
                            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxInt({scalarValueAccess})));");
                            break;
                        case SaveDataType.Long:
                            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxLong({scalarValueAccess})));");
                            break;
                        case SaveDataType.Float:
                            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxFloat({scalarValueAccess})));");
                            break;
                        case SaveDataType.Bool:
                            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxBool({scalarValueAccess})));");
                            break;
                        case SaveDataType.DateTime:
                            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxDate({scalarValueAccess})));");
                            break;
                        case SaveDataType.Guid:
                            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxGuid({scalarValueAccess})));");
                            break;
                    }

                    isb.CloseBrace();
                }
                else
                {
                    // Use CE types
                    switch (prop.SaveDataType)
                    {
                        case SaveDataType.String:
                            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxString({scalarValueAccess})));");
                            break;
                        case SaveDataType.Int:
                            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxInt({scalarValueAccess})));");
                            break;
                        case SaveDataType.Long:
                            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxLong({scalarValueAccess})));");
                            break;
                        case SaveDataType.Float:
                            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxFloat({scalarValueAccess})));");
                            break;
                        case SaveDataType.Bool:
                            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxBool({scalarValueAccess})));");
                            break;
                        case SaveDataType.DateTime:
                            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxDate({scalarValueAccess})));");
                            break;
                        case SaveDataType.Guid:
                            isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxGuid({scalarValueAccess})));");
                            break;
                    }
                }

                break;
            }

            // --- Lists/Arrays ---
            case SaveDataType.Array:
            {
                string listVar = $"{prop.PropertyName}_list";
                string elementType = prop.Type.TrimEnd('?').Replace("List<", "").Replace(">", "");
                bool elementIsNullable = elementType.EndsWith("?");
                SaveDataType elementDataType = prop.ElementSaveDataType;

                isb.AppendLine($"if ({propAccess} != null && {propAccess}.Count > 0) // Check if list itself is null or empty");
                isb.OpenBrace(); // if list not null/empty
                isb.AppendLine($"var {listVar} = new List<IPdxElement>();");
                isb.AppendLine($"foreach (var item in {propAccess})");
                isb.OpenBrace(); // foreach
                isb.AppendLine("if (item != null)");
                isb.OpenBrace();
                
                // Create element based on the element data type
                switch (elementDataType)
                {
                    case SaveDataType.String:
                        isb.AppendLine($"{listVar}.Add(new PdxString(item));");
                        break;
                    case SaveDataType.Int:
                        isb.AppendLine($"{listVar}.Add(new PdxInt(item));");
                        break;
                    case SaveDataType.Long:
                        isb.AppendLine($"{listVar}.Add(new PdxLong(item));");
                        break;
                    case SaveDataType.Float:
                        isb.AppendLine($"{listVar}.Add(new PdxFloat(item));");
                        break;
                    case SaveDataType.Bool:
                        isb.AppendLine($"{listVar}.Add(new PdxBool(item));");
                        break;
                    case SaveDataType.DateTime:
                        isb.AppendLine($"{listVar}.Add(new PdxDate(item));");
                        break;
                    case SaveDataType.Guid:
                        isb.AppendLine($"{listVar}.Add(new PdxGuid(item));");
                        break;
                    case SaveDataType.Object:
                        isb.AppendLine($"{listVar}.Add(item.ToPdxObject());");
                        break;
                    default:
                        isb.AppendLine($"{listVar}.Add(new PdxString(item.ToString()));"); // Fallback
                        break;
                }
                
                isb.CloseBrace();
                isb.CloseBrace();
                isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, {listVar}.ToPdxArray()));");
                isb.CloseBrace(); // End if list not null/empty
                break;
            }

            // --- Objects ---
            case SaveDataType.Object:
            {
                // Null check is handled by needsNullCheck block above
                isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, {propAccess}.ToPdxObject()));");
                break;
            }

            // --- Dictionaries / Maps ---
            case SaveDataType.DictionaryStringKey:
            {
                string dictVar = $"{prop.PropertyName}_dict";
                string dictValueType = ExtractDictionaryValueType(prop.Type);
                bool dictValueIsNullable = dictValueType.EndsWith("?");
                SaveDataType dictValueDataType = prop.ElementSaveDataType;

                isb.AppendLine($"if ({propAccess} != null && {propAccess}.Count > 0) // Check if dictionary itself is null or empty");
                isb.OpenBrace(); // if dict not null/empty
                isb.AppendLine($"var {dictVar}_properties = new List<KeyValuePair<string, IPdxElement>>();");
                isb.AppendLine($"foreach (var kvp in {propAccess})");
                isb.OpenBrace(); // foreach

                // Handle potential null values in the dictionary
                if (dictValueIsNullable || dictValueDataType == SaveDataType.String || dictValueDataType == SaveDataType.Object)
                {
                    isb.AppendLine("if (kvp.Value != null)");
                    isb.OpenBrace(); // if value not null
                }

                string dictValueAccess = (dictValueIsNullable && dictValueDataType != SaveDataType.String && dictValueDataType != SaveDataType.Object)
                    ? "kvp.Value.Value"
                    : "kvp.Value";

                switch (dictValueDataType)
                {
                    case SaveDataType.String:
                        isb.AppendLine($"{dictVar}_properties.Add(new KeyValuePair<string, IPdxElement>(kvp.Key, new PdxString({dictValueAccess})));");
                        break;
                    case SaveDataType.Int:
                        isb.AppendLine($"{dictVar}_properties.Add(new KeyValuePair<string, IPdxElement>(kvp.Key, new PdxInt({dictValueAccess})));");
                        break;
                    case SaveDataType.Long:
                        isb.AppendLine($"{dictVar}_properties.Add(new KeyValuePair<string, IPdxElement>(kvp.Key, new PdxLong({dictValueAccess})));");
                        break;
                    case SaveDataType.Float:
                        isb.AppendLine($"{dictVar}_properties.Add(new KeyValuePair<string, IPdxElement>(kvp.Key, new PdxFloat({dictValueAccess})));");
                        break;
                    case SaveDataType.Bool:
                        isb.AppendLine($"{dictVar}_properties.Add(new KeyValuePair<string, IPdxElement>(kvp.Key, new PdxBool({dictValueAccess})));");
                        break;
                    case SaveDataType.DateTime:
                        isb.AppendLine($"{dictVar}_properties.Add(new KeyValuePair<string, IPdxElement>(kvp.Key, new PdxDate({dictValueAccess})));");
                        break;
                    case SaveDataType.Guid:
                        isb.AppendLine($"{dictVar}_properties.Add(new KeyValuePair<string, IPdxElement>(kvp.Key, new PdxGuid({dictValueAccess})));");
                        break;
                    case SaveDataType.Object:
                        isb.AppendLine($"{dictVar}_properties.Add(new KeyValuePair<string, IPdxElement>(kvp.Key, {dictValueAccess}.ToPdxObject()));");
                        break;
                    default:
                        isb.AppendLine($"// TODO: Handle unknown value type {dictValueDataType} in dictionary serialization");
                        break;
                }

                if (dictValueIsNullable || dictValueDataType == SaveDataType.String || dictValueDataType == SaveDataType.Object)
                {
                    isb.CloseBrace(); // End if value not null
                    // else { dictVar_properties.Add(...null representation...); }
                }

                isb.CloseBrace(); // End foreach
                isb.AppendLine($"var {dictVar} = new PdxObject({dictVar}_properties);");
                isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, {dictVar}));");
                isb.CloseBrace(); // End if dict not null/empty
                break;
            }

            // --- Repeated Key Types --- (Need special serialization logic)
            case SaveDataType.RepeatedKeyStringList: // flag = x flag = y
            case SaveDataType.FlatRepeatedKeyList: // name = "a" name = "b"
            {
                // These should serialize as multiple key-value pairs with the *same* key
                isb.AppendLine($"if ({propAccess} != null)");
                isb.OpenBrace();
                isb.AppendLine($"foreach (var value in {propAccess})");
                isb.OpenBrace();
                isb.AppendLine("if (value != null) // Check inner value nullable?");
                isb.OpenBrace();
                isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxString(value)));");
                isb.CloseBrace();
                isb.CloseBrace(); // end foreach
                isb.CloseBrace(); // end if
                break;
            }

            case SaveDataType.FlatRepeatedObjectList: // pop = {} pop = {}
                isb.AppendLine($"if ({propAccess} != null)");
                isb.OpenBrace();
                isb.AppendLine($"foreach (var obj in {propAccess})");
                isb.OpenBrace();
                isb.AppendLine("if (obj != null)");
                isb.OpenBrace();
                isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, obj.ToPdxObject()));");
                isb.CloseBrace(); // end if obj != null
                isb.CloseBrace(); // end foreach
                isb.CloseBrace(); // end if list != null
                break;

            case SaveDataType.RepeatedArrayList: // list = { a b } list = { c d }
                isb.AppendLine($"if ({propAccess} != null)");
                isb.OpenBrace();
                isb.AppendLine($"foreach (var innerList in {propAccess})");
                isb.OpenBrace();
                isb.AppendLine("if (innerList != null)");
                isb.OpenBrace();
                isb.AppendLine("var innerPdxArrayElements = innerList.Where(s => s != null).Select(s => new PdxString(s));");
                isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, IPdxElement>({keyNameLiteral}, new PdxArray(innerPdxArrayElements)));");
                isb.CloseBrace(); // end if innerList != null
                isb.CloseBrace(); // end foreach
                isb.CloseBrace(); // end if outer list != null
                break;

            // --- Dictionary Types needing special handling ---
            case SaveDataType.DictionaryNumericKey:
            case SaveDataType.DictionaryIntKey:
            case SaveDataType.DictionaryScalarKeyObjectValue:
                isb.AppendLine($"// TODO: Implement serialization for {prop.SaveDataType} - {prop.PropertyName}");
                // DictionaryNumericKey/IntKey -> PdxObject where keys are strings
                // DictionaryScalarKeyObjectValue -> PdxArray of PdxArrays like [ [key, valueObj], ... ]
                break;

            default:
                isb.AppendLine($"// Unknown SaveDataType: {prop.SaveDataType} for property {prop.PropertyName}");
                break;
        }

        // Close the main null check if it was opened
        if (needsNullCheck)
        {
            isb.CloseBrace();
        }
    }

    static Schema AnalyzeRootPdxObject(PdxObject pdxObject, string className)
    {
        // Clean the class name first to ensure no numeric suffixes
        string cleanClassName = Regex.Replace(className, @"_\d+", "");
        
        var allSchemas = new List<Schema>();
        var processedTypes = new Dictionary<string, Schema>();
        var processedPaths = new HashSet<string>();
        
        var rootSchema = InferSchemaRecursive(pdxObject, cleanClassName, allSchemas, processedTypes, processedPaths);
        
        return rootSchema;
    }

    public static ImmutableArray<string> Execute(PdxObject root, string rootClassName)
    {
        try
        {
            var schemas = new List<Schema>();
            var processedTypes = new Dictionary<string, Schema>();
            var processedPaths = new HashSet<string>();

            // Begin schema inference from the root object
            var rootSchema = AnalyzeRootPdxObject(root, rootClassName);
            schemas.Add(rootSchema);

            // Create basic sample model with properties
            var sb = new StringBuilder();
            // TODO: Add using directives, etc.

            return ImmutableArray.Create(sb.ToString());
        }
        catch (Exception ex)
        {
            // TODO: Report the error diagnostics
            return ImmutableArray<string>.Empty;
        }
    }

    // Add schema fingerprinting method for compatible operation
    static string GetStructureSignature(List<SchemaProperty> properties)
    {
        var sb = new StringBuilder();
        foreach (var prop in properties.OrderBy(p => p.KeyName))
        {
            sb.Append(prop.KeyName).Append(':').Append(prop.Type).Append(';');
        }
        return sb.ToString();
    }

    // Simplified schema fingerprinting method for PdxObject
    static string GetSchemaFingerprint(PdxObject obj)
    {
        // Create a sorted list of property names to ensure consistency
        var propertyNames = new List<string>();
        foreach (var kvp in obj.Properties)
        {
            propertyNames.Add(kvp.Key);
        }
        propertyNames.Sort();
        
        var sb = new StringBuilder();
        foreach (var propName in propertyNames)
        {
            sb.Append(propName).Append(';');
        }
        
        return sb.ToString();
    }

    // Dictionary to track schema by structure
    static readonly Dictionary<string, Schema> schemaByFingerprint = new Dictionary<string, Schema>();

    private static bool IsNumericDictionary(PdxObject pdxObject)
    {
        // Check if most keys are numeric
        int numericKeyCount = 0;
        int totalKeys = pdxObject.Properties.Length;
        
        // Early exit for small collections
        if (totalKeys < 5)
            return false;
            
        foreach (var kvp in pdxObject.Properties)
        {
            if (long.TryParse(kvp.Key, out _))
                numericKeyCount++;
        }
        
        // If >75% of keys are numeric, treat as dictionary
        return numericKeyCount > 0 && 
               numericKeyCount > (totalKeys * 0.75) && 
               numericKeyCount >= 5; // At least 5 numeric entries to be considered a dictionary
    }

    // Add ToValidIdentifier method
    static string ToValidIdentifier(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "_var";
            
        // Replace invalid chars with underscores
        string sanitized = Regex.Replace(input, @"[^\w]", "_");
        
        // Ensure it doesn't start with a digit
        if (char.IsDigit(sanitized[0]))
            sanitized = "_" + sanitized;
            
        return sanitized;
    }
}