using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MageeSoft.PDX.CE.SourceGenerator;

[Generator]
public class IncrementalGenerator : IIncrementalGenerator
{
    // --- Helper Class for Indentation ---
    class IndentedStringBuilder
    {
        readonly StringBuilder _sb = new StringBuilder(1024 * 1024); // Set an initial 1MB capacity
        int _indentationLevel;
        const int IndentSize = 4;
        string _currentIndent = "";

        public void Indent()
        {
            _indentationLevel++;
            _currentIndent = new string(' ', _indentationLevel * IndentSize);
        }

        public void Unindent()
        {
            if (_indentationLevel > 0)
            {
                _indentationLevel--;
                _currentIndent = new string(' ', _indentationLevel * IndentSize);
            }
        }

        public void AppendLine(string line = "")
        {
            if (!string.IsNullOrEmpty(line))
            {
                _sb.Append(_currentIndent);
            }

            _sb.AppendLine(line);
        }

        public void OpenBrace()
        {
            AppendLine("{");
            Indent();
        }

        public void CloseBrace()
        {
            Unindent();
            AppendLine("}");
        }

        // Allows setting the initial indentation level
        public void SetIndentLevel(int level)
        {
            _indentationLevel = Math.Max(0, level);
            _currentIndent = new string(' ', _indentationLevel * IndentSize);
        }

        public override string ToString() => _sb.ToString();
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
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
                hintName: "GameStateDocumentAttribute.g.cs",
                source: ModelGenerationHelper.Attribute
            )
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

                SaveObject root = Parser.Parse(sourceText.ToString());
                   
                   // 2. Schema inference to generate all schemas recursively from the root SaveObject
                string rootClassName = classSchemaPair.ClassDeclaration.Identifier.Text;
                List<Schema> schemas = InferSchema(root, rootClassName);
                   
                // 3. Generate the C# code
                StringBuilder sourceBuilder = GenerateCSharpCode(schemas, rootClassName, classSchemaPair.ClassDeclaration, compilation);
                   
                // 4. Generate the 'ClassName.g.cs' associated with partial class with the GameStateDocumentAttribute(schemaFileName)
                   sourceProductionContext.AddSource(
                    hintName: $"{rootClassName}.g.cs",
                    source: sourceBuilder.ToString()
                );
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

    static List<Schema> InferSchema(SaveObject root, string rootClassName)
    {
        var schemas = new List<Schema>();
        var processedTypes = new Dictionary<string, Schema>();
        InferSchemaRecursive(root, rootClassName, schemas, processedTypes);
        return schemas;
    }

    static Schema InferSchemaRecursive(SaveObject saveObject, string className, List<Schema> allSchemas, Dictionary<string, Schema> processedTypes)
    {
        if (processedTypes.TryGetValue(className, out Schema? existingSchema))
        {
            InferProperties(saveObject, existingSchema, allSchemas, processedTypes);
            return existingSchema;
        }

        var schema = new Schema
        {
            Name = className
        };

        processedTypes.Add(className, schema);
        allSchemas.Add(schema);

        InferProperties(saveObject, schema, allSchemas, processedTypes);

        return schema;
    }

    static void InferProperties(SaveObject saveObject, Schema schema, List<Schema> allSchemas, Dictionary<string, Schema> processedTypes)
    {
        // Group properties by key to detect repeated keys and other patterns
        var propertyGroups = saveObject.Properties
            .GroupBy(p => p.Key)
            .ToDictionary(g => g.Key, g => g.ToList());

        var processedRepeatedKeys = new HashSet<string>(); // Track keys handled by special logic

        // --- Step 1: Detect and handle specific repeated key patterns --- 
        foreach (var group in propertyGroups)
        {
            if (group.Value.Count <= 1) continue; // Only interested in repeated keys here

                string key = group.Key;
                string propertyName = ToPascalCase(key);
            var values = group.Value;

            // Check for RepeatedArrayList pattern: key = {..} key = {..} where inner elements are strings
            if (values.All(v => v.Value is SaveArray)) 
            {
                bool allArrayItemsAreStrings = true;
                
                // Check each array to ensure all items are strings
                foreach (var kvp in values)
                {
                    if (kvp.Value is SaveArray array)
                    {
                        if (!array.Items.All(item => item is Scalar<string>))
                        {
                            allArrayItemsAreStrings = false;
                            break;
                        }
                    }
                }
                
                if (allArrayItemsAreStrings)
                {
                    if (schema.Properties.Any(p => p.KeyName == key)) continue; // Already processed?

                    schema.Properties.Add(new SchemaProperty
                    {
                        KeyName = key,
                        PropertyName = propertyName,
                        SaveDataType = SaveDataType.RepeatedArrayList,
                        Type = "List<List<string>>", // Updated to match test expectations
                        ElementSaveDataType = SaveDataType.Array, // Outer list contains arrays
                        IsNullable = true // The List<...> itself is nullable
                    });
                    processedRepeatedKeys.Add(key);
                    continue; // Move to next group
                }
            }
            
            // Check for FlatRepeatedObjectList pattern: key = {..} key = {..} where values are objects
             if (values.All(v => v.Value is SaveObject))
             {
                 if (schema.Properties.Any(p => p.KeyName == key)) continue;

                 // Infer the schema for the repeated object by merging all instances
                 string nestedClassName = GenerateUniqueClassName(schema.Name, propertyName + "Item", processedTypes.Keys);
                 Schema elementSchema = new Schema { Name = nestedClassName };
                 // Add to processedTypes *before* recursive call to handle cycles/merging
                  if (!processedTypes.ContainsKey(nestedClassName))
                  {
                      processedTypes.Add(nestedClassName, elementSchema);
                      allSchemas.Add(elementSchema);
                  }
                  else
                  {
                       elementSchema = processedTypes[nestedClassName]; // Use existing if somehow already created
                  }

                 foreach (var kvp in values)
                 {
                     InferSchemaRecursive((SaveObject)kvp.Value, nestedClassName, allSchemas, processedTypes);
                 }

                 schema.Properties.Add(new SchemaProperty
                 {
                     KeyName = key,
                        PropertyName = propertyName,
                     SaveDataType = SaveDataType.FlatRepeatedObjectList,
                     Type = $"List<{nestedClassName}?>", // List contains nullable objects
                     ElementSaveDataType = SaveDataType.Object,
                     NestedSchema = elementSchema,
                     IsNullable = true
                 });
                 processedRepeatedKeys.Add(key);
                 continue;
             }

             // Check for RepeatedKeyStringList / FlatRepeatedKeyList pattern: key = scalar key = scalar
              if (values.All(v => v.Value is Scalar<string>))
              {
                  if (schema.Properties.Any(p => p.KeyName == key)) continue;

                  // Determine if it fits FlatRepeatedKeyList (often identifiers) or RepeatedKeyStringList (flags/general)
                  // For now, default to FlatRepeatedKeyList as it's slightly more common?
                  // We could refine this based on key name heuristics later if needed.
                  schema.Properties.Add(new SchemaProperty
                            {
                                KeyName = key,
                                PropertyName = propertyName,
                      SaveDataType = SaveDataType.FlatRepeatedKeyList, // Or RepeatedKeyStringList
                      Type = "List<string?>",
                      ElementSaveDataType = SaveDataType.String,
                      IsNullable = true
                  });
                  processedRepeatedKeys.Add(key);
                  continue;
              }
              
            // Add checks for other complex repeated patterns if necessary...
        }

        // --- Step 2: Process remaining properties (unique keys or first instance of unhandled repeated keys) ---
        foreach (var kvp in saveObject.Properties)
        {
            string key = kvp.Key;
            SaveElement value = kvp.Value;

            // Skip keys already handled by the specific repeated pattern logic above
            if (processedRepeatedKeys.Contains(key)) 
                continue;
                
            // Also skip subsequent instances of any *other* repeated keys not handled above
            // This ensures we only call InferPropertyType once for any given key name
            if (propertyGroups[key].Count > 1 && schema.Properties.Any(p => p.KeyName == key))
                continue;

            string propertyName = ToPascalCase(key);
            var existingProperty = schema.Properties.FirstOrDefault(p => p.PropertyName == propertyName);

            if (existingProperty == null)
            {
                // This is where unique keys or the *first* instance of a non-special repeated key gets processed
                var property = InferPropertyType(key, propertyName, value, schema, allSchemas, processedTypes);

                if (property != null)
                {
                    // Nullability is complex. Let's refine initial guess.
                    // Value types are non-nullable unless mixed types observed later.
                    // Reference types (string, List<>, Dictionary<>, custom classes) are nullable.
                    property.IsNullable = !(property.SaveDataType == SaveDataType.Int ||
                                            property.SaveDataType == SaveDataType.Long ||
                                            property.SaveDataType == SaveDataType.Float ||
                                            property.SaveDataType == SaveDataType.Bool ||
                                            property.SaveDataType == SaveDataType.DateTime ||
                                            property.SaveDataType == SaveDataType.Guid);
                    // Override for known reference types
                    if (property.SaveDataType == SaveDataType.String || 
                        property.SaveDataType == SaveDataType.Object || 
                        property.SaveDataType == SaveDataType.Array ||
                        IsDictionaryType(property.SaveDataType) ||
                        IsRepeatedKeyType(property.SaveDataType))
                    {
                        property.IsNullable = true; 
                    }

                    schema.Properties.Add(property);
                }
            }
            else
            {
                // This block should technically not be hit for the *first* pass due to the checks above,
                // but is kept for the UpdateExistingProperty logic which merges subsequent observations.
                UpdateExistingProperty(existingProperty, value, schema.Name, allSchemas, processedTypes);
            }
        }
    }

    static void UpdateExistingProperty(SchemaProperty property, SaveElement newValue, string parentClassName, List<Schema> allSchemas,
        Dictionary<string, Schema> processedTypes)
    {
        // Handle type refinement based on the new value
        switch (newValue)
        {
            case Scalar<string> _:
                // Keep property as string or make it nullable if it wasn't a string before
                if (property.SaveDataType != SaveDataType.String && property.SaveDataType != SaveDataType.Unknown)
                {
                    property.IsNullable = true; // Mixed types means we need nullable
                }

                break;

            case Scalar<int> _:
                // Promote to int or higher numeric type
                property.AddObservedNumericType(SaveDataType.Int);
                break;

            case Scalar<long> _:
                // Promote to long or float
                property.AddObservedNumericType(SaveDataType.Long);
                break;

            case Scalar<float> _:
                // Always promote to float
                property.AddObservedNumericType(SaveDataType.Float);
                break;

            case SaveObject obj:
                // Merge properties from this object instance
                if (property.SaveDataType == SaveDataType.Object && property.NestedSchema != null)
                {
                    InferProperties(obj, property.NestedSchema, allSchemas, processedTypes);
                }
                else if (property.SaveDataType != SaveDataType.Object)
                {
                    // We've seen this property with a different type before
                    // Make it nullable and potentially change type
                    property.IsNullable = true;
                }

                break;

            case SaveArray arr:
                // Handle array type refinement if possible
                // This is more complex - we'd need to examine array elements
                if (property.SaveDataType == SaveDataType.Array)
                {
                    RefineArrayElementTypes(property, arr, parentClassName, allSchemas, processedTypes);
                }
                else
                {
                    // Mixed type - make nullable
                    property.IsNullable = true;
                }

                break;
        }
    }

    static void RefineArrayElementTypes(SchemaProperty property, SaveArray array, string parentClassName, List<Schema> allSchemas,
        Dictionary<string, Schema> processedTypes)
    {
        // No refinement needed for empty arrays
        if (array.Items.Count == 0) return;

        // Check if elements are primitives that can be promoted
        bool hasFloats = array.Items.Any(i => i is Scalar<float>);
        bool hasLongs = array.Items.Any(i => i is Scalar<long>);
        bool hasInts = array.Items.Any(i => i is Scalar<int>);

        if (hasFloats || hasLongs || hasInts)
        {
            // Apply numeric type promotion for array elements
            if (hasFloats)
            {
                property.ElementSaveDataType = SaveDataType.Float;
                property.Type = "List<float>";
            }
            else if (hasLongs)
            {
                property.ElementSaveDataType = SaveDataType.Long;
                property.Type = "List<long>";
            }
            else if (hasInts && property.ElementSaveDataType != SaveDataType.Long && property.ElementSaveDataType != SaveDataType.Float)
            {
                property.ElementSaveDataType = SaveDataType.Int;
                property.Type = "List<int>";
            }
        }

        // For object elements, merge schema information
        if (property.ElementSaveDataType == SaveDataType.Object && property.NestedSchema != null)
        {
            foreach (var item in array.Items)
            {
                if (item is SaveObject objItem)
                {
                    InferProperties(objItem, property.NestedSchema, allSchemas, processedTypes);
                }
            }
        }
    }

    static SchemaProperty InferPropertyType(string keyName, string propertyName, SaveElement value, Schema containingSchema, List<Schema> allSchemas,
        Dictionary<string, Schema> processedTypes)
    {
        var property = new SchemaProperty
        {
            KeyName = keyName,
            PropertyName = propertyName
        };

        switch (value)
        {
            case Scalar<string> s:
                property.Type = "string";
                property.SaveDataType = SaveDataType.String;
                break;
            case Scalar<int> i:
                property.Type = "int";
                property.SaveDataType = SaveDataType.Int;
                break;
            case Scalar<long> l:
                property.Type = "long";
                property.SaveDataType = SaveDataType.Long;
                break;
            case Scalar<float> f:
                property.Type = "float";
                property.SaveDataType = SaveDataType.Float;
                break;
            case Scalar<bool> b:
                property.Type = "bool";
                property.SaveDataType = SaveDataType.Bool;
                break;
            case Scalar<DateTime> dt:
                property.Type = "DateTime";
                property.SaveDataType = SaveDataType.DateTime;
                break;
            case Scalar<Guid> g:
                property.Type = "Guid";
                property.SaveDataType = SaveDataType.Guid;
                break;
            case SaveObject obj:
                // Infer nested object schema
                string nestedClassName = GenerateUniqueClassName(containingSchema.Name, propertyName, processedTypes.Keys);
                var nestedSchema = InferSchemaRecursive(obj, nestedClassName, allSchemas, processedTypes);

                property.Type = nestedClassName;
                    property.SaveDataType = SaveDataType.Object;
                    property.NestedSchema = nestedSchema;
                    break;
            case SaveArray arr:
                InferArrayPropertyType(property, arr, containingSchema, allSchemas, processedTypes);
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

    static void InferArrayPropertyType(SchemaProperty property, SaveArray arr, Schema containingSchema, List<Schema> allSchemas, Dictionary<string, Schema> processedTypes)
    {
        property.SaveDataType = SaveDataType.Array;
        property.IsNullable = true;

        if (arr.Items.Count == 0)
        {
            // Empty array - default to List<string?> as a common case.
            property.Type = "List<string?>";
            property.ElementSaveDataType = SaveDataType.String;
            property.IsNullable = true;
            return;
        }

        // Determine the common type of elements in the array
        SaveDataType firstElementType = GetSaveDataType(arr.Items[0]);
        bool allSameType = arr.Items.All(item => GetSaveDataType(item) == firstElementType);

        if (!allSameType)
        {
            // Mixed types - Check for numeric promotion possibilities (int/long/float)
            bool hasFloat = arr.Items.Any(i => GetSaveDataType(i) == SaveDataType.Float);
            bool hasLong = arr.Items.Any(i => GetSaveDataType(i) == SaveDataType.Long);
            bool hasInt = arr.Items.Any(i => GetSaveDataType(i) == SaveDataType.Int);

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
        // Determine element nullability based on actual values? For now, assume elements can be null for scalars.
        bool elementNullable = firstElementType != SaveDataType.Object;

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
                // Infer schema for objects within the array
                string elementClassName = GenerateUniqueClassName(containingSchema.Name, property.PropertyName + "Item", processedTypes.Keys);
                var elementSchema = new Schema
                {
                    Name = elementClassName
                };

                processedTypes[elementClassName] = elementSchema;
                allSchemas.Add(elementSchema);

                // Merge properties from all objects in the array
                foreach (var item in arr.Items)
                {
                    if (item is SaveObject objItem)
                    {
                        InferProperties(objItem, elementSchema, allSchemas, processedTypes);
                    }
                }

                property.Type = $"List<{elementClassName}>";
                property.NestedSchema = elementSchema;
                break;
            default:
                // Fallback for unknown element types within array
                property.Type = "List<object>";
                property.ElementSaveDataType = SaveDataType.Unknown;
                break;
        }
    }

    static SaveDataType GetSaveDataType(SaveElement element)
    {
        return element switch
        {
            Scalar<string> _ => SaveDataType.String,
            Scalar<int> _ => SaveDataType.Int,
            Scalar<long> _ => SaveDataType.Long,
            Scalar<float> _ => SaveDataType.Float,
            Scalar<bool> _ => SaveDataType.Bool,
            Scalar<DateTime> _ => SaveDataType.DateTime,
            Scalar<Guid> _ => SaveDataType.Guid,
            SaveObject _ => SaveDataType.Object,
            SaveArray _ => SaveDataType.Array,
            _ => SaveDataType.Unknown // Fallback for unrecognized types
        };
    }

    // --- Code Generation ---

    static StringBuilder GenerateCSharpCode(List<Schema> schemas, string rootClassName, ClassDeclarationSyntax partialClass, Compilation compilation)
    {
        // Use IndentedStringBuilder instead of StringBuilder
        var isb = new IndentedStringBuilder();
        var syntaxTree = partialClass.SyntaxTree;
        var rootNode = syntaxTree.GetRoot();
        var usingDirectives = rootNode.DescendantNodes().OfType<UsingDirectiveSyntax>();
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
        if (!usingDirectives.Any(u => u.Name!.ToString() == "System"))
            isb.AppendLine("using System;");

        if (!usingDirectives.Any(u => u.Name!.ToString() == "System.Collections.Generic"))
            isb.AppendLine("using System.Collections.Generic;");

        if (!usingDirectives.Any(u => u.Name!.ToString() == "System.Linq"))
            isb.AppendLine("using System.Linq;");

        if (!usingDirectives.Any(u => u.Name!.ToString() == "MageeSoft.PDX.CE"))
            isb.AppendLine("using MageeSoft.PDX.CE;");

        isb.AppendLine();

        // Add the namespace
        bool hasNamespace = !string.IsNullOrEmpty(namespaceString);

        if (hasNamespace)
        {
            isb.AppendLine($"namespace {namespaceString}");
            isb.OpenBrace(); // Handles brace and indent
        }

        Schema? rootSchema = schemas.FirstOrDefault(s => s.Name == rootClassName);

        if (rootSchema == null)
        {
            isb.AppendLine($"// ERROR: Root schema '{rootClassName}' not found.");
            if (hasNamespace) isb.CloseBrace(); // Close namespace if opened
            return new StringBuilder(isb.ToString()); // Return StringBuilder for compatibility
        }

        var generatedClassNames = new HashSet<string>();

        // Pass IndentedStringBuilder to recursive generation
        GenerateClassRecursive(isb, rootSchema, schemas, generatedClassNames, isRoot: true);

        // End of types
        if (hasNamespace)
        {
            isb.CloseBrace(); // Close namespace brace and unindent
        }

        // Return as StringBuilder for the existing interface
        return new StringBuilder(isb.ToString());
    }

    // Updated to accept IndentedStringBuilder
    static void GenerateClassRecursive(IndentedStringBuilder isb, Schema schema, List<Schema> allSchemas, HashSet<string> generatedClassNames, bool isRoot)
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
        isb.AppendLine($"/// <summary>Loads data from a SaveObject into a new {schema.Name} instance.</summary>");
        isb.AppendLine($"public static {schema.Name} Load(SaveObject saveObject)");
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

        // Add ToSaveObject method for serialization
        isb.AppendLine();
        isb.AppendLine("/// <summary>Converts this model back to a SaveObject structure</summary>");
        isb.AppendLine("public SaveObject ToSaveObject()");
        isb.OpenBrace(); // ToSaveObject method opening brace
        isb.AppendLine("var properties = new List<KeyValuePair<string, SaveElement>>();");
        isb.AppendLine("var result = new SaveObject(properties);");
        isb.AppendLine();

        // Generate serialization code for each property - Pass IndentedStringBuilder
        foreach (var prop in schema.Properties.OrderBy(p => p.PropertyName))
        {
            GeneratePropertySave(isb, prop, "properties");
        }

        isb.AppendLine("return result;");
        isb.CloseBrace(); // ToSaveObject method closing brace

        // Generate nested classes recursively
        var nestedSchemasToGenerate = schema.Properties
            .Where(p => p.NestedSchema != null)
            .Select(p => p.NestedSchema)
            .Distinct();

        foreach (var nestedSchema in nestedSchemasToGenerate.OrderBy(ns => ns!.Name))
        {
            isb.AppendLine();
            GenerateClassRecursive(isb, nestedSchema!, allSchemas, generatedClassNames, isRoot: false);
        }

        isb.CloseBrace(); // Class closing brace
    }

    // Method to generate code for saving properties back to a SaveObject
    // Updated signature to accept IndentedStringBuilder
    static void GeneratePropertySave(IndentedStringBuilder isb, SchemaProperty prop, string propertiesVar)
    {
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
                string typeArg = GetScalarTypeName(prop.SaveDataType);

                if (prop.SaveDataType == SaveDataType.String && prop.IsNullable && !needsNullCheck)
                {
                    isb.AppendLine($"if ({propAccess} != null)");
                    isb.OpenBrace();
                    isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, SaveElement>({keyNameLiteral}, new Scalar<string>({propAccess})));");
                    isb.CloseBrace();
                }
                else if (prop.IsNullable && !needsNullCheck && prop.SaveDataType != SaveDataType.String) // Other nullable value types not in main null check
                {
                    isb.AppendLine($"if ({propAccess}.HasValue)");
                    isb.OpenBrace();
                    isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, SaveElement>({keyNameLiteral}, new Scalar<{typeArg}>({scalarValueAccess})));");
                    isb.CloseBrace();
                }
                else
                {
                    isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, SaveElement>({keyNameLiteral}, new Scalar<{typeArg}>({scalarValueAccess})));");
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
                isb.AppendLine($"var {listVar} = new List<SaveElement>();");
                isb.AppendLine($"foreach (var item in {propAccess})");
                isb.OpenBrace(); // foreach

                // Handle potential null elements within the list
                if (elementIsNullable || elementDataType == SaveDataType.String || elementDataType == SaveDataType.Object)
                {
                    isb.AppendLine("if (item != null)");
                    isb.OpenBrace(); // if item not null
                }

                string itemAccess = (elementIsNullable && elementDataType != SaveDataType.String && elementDataType != SaveDataType.Object) ? "item.Value" : "item";

                switch (elementDataType)
                {
                    case SaveDataType.String:
                    case SaveDataType.Int:
                    case SaveDataType.Long:
                    case SaveDataType.Float:
                    case SaveDataType.Bool:
                    case SaveDataType.DateTime:
                    case SaveDataType.Guid:
                        string scalarTypeArg = GetScalarTypeName(elementDataType);
                        isb.AppendLine($"{listVar}.Add(new Scalar<{scalarTypeArg}>({itemAccess}));");
                        break;
                    case SaveDataType.Object:
                        isb.AppendLine($"{listVar}.Add({itemAccess}.ToSaveObject());");
                        break;
                    default:
                        isb.AppendLine($"// TODO: Handle unknown element type {elementDataType} in array serialization");
                        break;
                }

                if (elementIsNullable || elementDataType == SaveDataType.String || elementDataType == SaveDataType.Object)
                {
                    isb.CloseBrace(); // End if item not null
                    // else { listVar.Add(null??); } // Decide how to serialize null items if needed
                }

                isb.CloseBrace(); // End foreach
                isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, SaveElement>({keyNameLiteral}, new SaveArray({listVar})));");
                isb.CloseBrace(); // End if list not null/empty
                break;
            }

            // --- Objects ---
            case SaveDataType.Object:
            {
                // Null check is handled by needsNullCheck block above
                isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, SaveElement>({keyNameLiteral}, {propAccess}.ToSaveObject()));");
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
                isb.AppendLine($"var {dictVar}_properties = new List<KeyValuePair<string, SaveElement>>();");
                isb.AppendLine($"var {dictVar} = new SaveObject({dictVar}_properties);");
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
                    case SaveDataType.Int:
                    case SaveDataType.Long:
                    case SaveDataType.Float:
                    case SaveDataType.Bool:
                    case SaveDataType.DateTime:
                    case SaveDataType.Guid:
                        string dictScalarTypeArg = GetScalarTypeName(dictValueDataType);
                        isb.AppendLine($"{dictVar}_properties.Add(new KeyValuePair<string, SaveElement>(kvp.Key, new Scalar<{dictScalarTypeArg}>({dictValueAccess})));");
                        break;
                    case SaveDataType.Object:
                        isb.AppendLine($"{dictVar}_properties.Add(new KeyValuePair<string, SaveElement>(kvp.Key, {dictValueAccess}.ToSaveObject()));");
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
                isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, SaveElement>({keyNameLiteral}, {dictVar}));");
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
                isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, SaveElement>({keyNameLiteral}, new Scalar<string>(value)));");
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
                isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, SaveElement>({keyNameLiteral}, obj.ToSaveObject()));");
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
                isb.AppendLine("var innerSaveArrayElements = innerList.Where(s => s != null).Select(s => new Scalar<string>(s));");
                isb.AppendLine($"{propertiesVar}.Add(new KeyValuePair<string, SaveElement>({keyNameLiteral}, new SaveArray(innerSaveArrayElements)));");
                isb.CloseBrace(); // end if innerList != null
                isb.CloseBrace(); // end foreach
                isb.CloseBrace(); // end if outer list != null
                break;

            // --- Dictionary Types needing special handling ---
            case SaveDataType.DictionaryNumericKey:
            case SaveDataType.DictionaryIntKey:
            case SaveDataType.DictionaryScalarKeyObjectValue:
                isb.AppendLine($"// TODO: Implement serialization for {prop.SaveDataType} - {prop.PropertyName}");
                // DictionaryNumericKey/IntKey -> SaveObject where keys are strings
                // DictionaryScalarKeyObjectValue -> SaveArray of SaveArrays like [ [key, valueObj], ... ]
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

    // Method for loading properties
    // Updated signature to accept IndentedStringBuilder
    static void GeneratePropertyLoad(IndentedStringBuilder isb, SchemaProperty prop, string targetObject)
    {
        string targetProperty = $"{targetObject}.{prop.PropertyName}";
        string keyNameLiteral = ToCSharpStringLiteral(prop.KeyName);
        string varName = ToCamelCase(prop.PropertyName);
        string typeWithoutNullable = prop.Type.TrimEnd('?');

        switch (prop.SaveDataType)
        {
            case SaveDataType.String:
                isb.AppendLine($"if (saveObject.TryGetString({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {varName};");
                isb.CloseBrace();
                break;
            case SaveDataType.Int:
                isb.AppendLine($"if (saveObject.TryGetInt({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {varName}.Value;");
                isb.CloseBrace();
                break;
            case SaveDataType.Long:
                isb.AppendLine($"if (saveObject.TryGetLong({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {varName}.Value;");
                isb.CloseBrace();
                break;
            case SaveDataType.Float:
                isb.AppendLine($"if (saveObject.TryGetFloat({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {varName}.Value;");
                isb.CloseBrace();
                break;
            case SaveDataType.Bool:
                isb.AppendLine($"if (saveObject.TryGetBool({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {varName}.Value;");
                isb.CloseBrace();
                break;
            case SaveDataType.DateTime:
                isb.AppendLine($"if (saveObject.TryGetDateTime({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {varName}.Value;");
                isb.CloseBrace();
                break;
            case SaveDataType.Guid:
                isb.AppendLine($"if (saveObject.TryGetGuid({keyNameLiteral}, out var {varName}))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {varName}.Value;");
                isb.CloseBrace();
                break;
            case SaveDataType.Object:
                isb.AppendLine($"if (saveObject.TryGetSaveObject({keyNameLiteral}, out var {varName}Obj))");
                isb.OpenBrace();
                isb.AppendLine($"{targetProperty} = {typeWithoutNullable}.Load({varName}Obj);");
                isb.CloseBrace();
                break;
            case SaveDataType.Array:
                string listElementType = prop.Type.TrimEnd('?').Replace("List<", "").Replace(">", "");
                string listElementNullableMarker = listElementType.EndsWith("?") ? "?" : "";
                listElementType = listElementType.TrimEnd('?');

                isb.AppendLine($"if (saveObject.TryGetSaveArray({keyNameLiteral}, out var {varName}Array))");
                isb.OpenBrace(); // if TryGetSaveArray
                isb.AppendLine($"{targetProperty} = new List<{listElementType}{listElementNullableMarker}>();");
                isb.AppendLine($"foreach (var item in {varName}Array.Items)");
                isb.OpenBrace(); // foreach

                switch (prop.ElementSaveDataType)
                {
                    case SaveDataType.String:
                        isb.AppendLine("if (item is Scalar<string> scalarString)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarString.Value);"); // Indent manually inside simple if
                        break;
                    case SaveDataType.Int:
                        isb.AppendLine("if (item is Scalar<int> scalarInt)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarInt.Value);");
                        break;
                    case SaveDataType.Long:
                        isb.AppendLine("if (item is Scalar<long> scalarLong)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarLong.Value);");
                        break;
                    case SaveDataType.Float:
                        isb.AppendLine("if (item is Scalar<float> scalarFloat)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarFloat.Value);");
                        break;
                    case SaveDataType.Bool:
                        isb.AppendLine("if (item is Scalar<bool> scalarBool)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarBool.Value);");
                        break;
                    case SaveDataType.DateTime:
                        isb.AppendLine("if (item is Scalar<DateTime> scalarDateTime)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarDateTime.Value);");
                        break;
                    case SaveDataType.Guid:
                        isb.AppendLine("if (item is Scalar<Guid> scalarGuid)");
                        isb.AppendLine($"    {targetProperty}.Add(scalarGuid.Value);");
                        break;
                    case SaveDataType.Object:
                        isb.AppendLine("if (item is SaveObject objItem)");
                        isb.OpenBrace(); // if is SaveObject
                        isb.AppendLine($"var newObj = {listElementType}.Load(objItem);");
                        isb.AppendLine($"{targetProperty}.Add(newObj);");
                        isb.CloseBrace(); // end if is SaveObject
                        break;
                    default:
                        isb.AppendLine("if (item is Scalar<string> scalarString)");
                        isb.OpenBrace();
                        isb.AppendLine(
                            $"try {{ {targetProperty}.Add(({listElementType}{listElementNullableMarker})Convert.ChangeType(scalarString.Value, typeof({listElementType}))); }} catch {{ /* Handle conversion error? */ }} // Fallback conversion"
                        );

                        isb.CloseBrace();
                        isb.AppendLine("// TODO: Add handling for other scalar types if needed for fallback conversion");
                        break;
                }

                isb.CloseBrace(); // End foreach loop
                isb.CloseBrace(); // End if TryGetSaveArray
                break;

            // --- Dictionary Types ---
            case SaveDataType.DictionaryNumericKey:
            case SaveDataType.DictionaryIntKey:
            {
                string keyType = prop.SaveDataType == SaveDataType.DictionaryIntKey ? "int" : "long";
                string valueTypeNumeric = ExtractDictionaryValueType(prop.Type);
                string valueTypeNumericNullableMarker = valueTypeNumeric.EndsWith("?") ? "?" : "";
                valueTypeNumeric = valueTypeNumeric.TrimEnd('?');

                isb.AppendLine($"if (saveObject.TryGetSaveObject({keyNameLiteral}, out var {varName}Dict))");
                isb.OpenBrace(); // if TryGetSaveObject
                isb.AppendLine($"{targetProperty} = new {prop.Type.TrimEnd('?')}();");
                isb.AppendLine($"foreach (var kvp in {varName}Dict.Properties)");
                isb.OpenBrace(); // foreach kvp
                isb.AppendLine($"if ({keyType}.TryParse(kvp.Key, out var key))");
                isb.OpenBrace(); // if TryParse key

                switch (prop.ElementSaveDataType)
                {
                    case SaveDataType.String:
                        isb.AppendLine("if (kvp.Value is Scalar<string> scalarString)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarString.Value);");
                        break;
                    case SaveDataType.Int:
                        isb.AppendLine("if (kvp.Value is Scalar<int> scalarInt)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarInt.Value);");
                        break;
                    case SaveDataType.Long:
                        isb.AppendLine("if (kvp.Value is Scalar<long> scalarLong)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarLong.Value);");
                        break;
                    case SaveDataType.Float:
                        isb.AppendLine("if (kvp.Value is Scalar<float> scalarFloat)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarFloat.Value);");
                        break;
                    case SaveDataType.Bool:
                        isb.AppendLine("if (kvp.Value is Scalar<bool> scalarBool)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarBool.Value);");
                        break;
                    case SaveDataType.DateTime:
                        isb.AppendLine("if (kvp.Value is Scalar<DateTime> scalarDateTime)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarDateTime.Value);");
                        break;
                    case SaveDataType.Guid:
                        isb.AppendLine("if (kvp.Value is Scalar<Guid> scalarGuid)");
                        isb.AppendLine($"    {targetProperty}.Add(key, scalarGuid.Value);");
                        break;
                    case SaveDataType.Object:
                        isb.AppendLine("if (kvp.Value is SaveObject objItem)");
                        isb.OpenBrace();
                        isb.AppendLine($"var newObj = {valueTypeNumeric}.Load(objItem);");
                        isb.AppendLine($"{targetProperty}.Add(key, newObj);");
                        isb.CloseBrace();
                        break;
                    default:
                        isb.AppendLine("if (kvp.Value is Scalar<string> scalarString)");
                        isb.OpenBrace();
                        isb.AppendLine(
                            $"try {{ {targetProperty}.Add(key, ({valueTypeNumeric}{valueTypeNumericNullableMarker})Convert.ChangeType(scalarString.Value, typeof({valueTypeNumeric}))); }} catch {{}} // Fallback"
                        );

                        isb.CloseBrace();
                        break;
                }

                isb.CloseBrace(); // End if TryParse key
                isb.CloseBrace(); // End foreach kvp
                isb.CloseBrace(); // End if TryGetSaveObject
                break;
            }

            case SaveDataType.DictionaryScalarKeyObjectValue:
            {
                string scalarDictValueType = ExtractDictionaryValueType(prop.Type);
                scalarDictValueType = scalarDictValueType.TrimEnd('?');
                string scalarDictKeyType = prop.KeySaveDataType == SaveDataType.Long ? "long" : "int";

                isb.AppendLine($"if (saveObject.TryGetSaveArray({keyNameLiteral}, out var {varName}Pairs))");
                isb.OpenBrace(); // if TryGetSaveArray
                isb.AppendLine($"{targetProperty} = new {prop.Type.TrimEnd('?')}();");
                isb.AppendLine($"foreach (var item in {varName}Pairs.Items)");
                isb.OpenBrace(); // foreach item
                isb.AppendLine("if (item is SaveArray pairArray && pairArray.Items.Count >= 2)");
                isb.OpenBrace(); // if is pair array
                isb.AppendLine("var keyElement = pairArray.Items[0];");
                isb.AppendLine("var valueElement = pairArray.Items[1];");
                isb.AppendLine($"if (keyElement is Scalar<{scalarDictKeyType}> keyScalar && valueElement is SaveObject valueObj)");
                isb.OpenBrace(); // if key/value match expected types
                isb.AppendLine($"var newObj = {scalarDictValueType}.Load(valueObj);");
                isb.AppendLine($"{targetProperty}.Add(keyScalar.Value, newObj);");
                isb.CloseBrace(); // end if key/value match
                isb.CloseBrace(); // end if is pair array
                isb.CloseBrace(); // end foreach item
                isb.CloseBrace(); // end if TryGetSaveArray
                break;
            }

            case SaveDataType.DictionaryStringKey:
            {
                string stringDictValueType = ExtractDictionaryValueType(prop.Type);
                string stringDictValueTypeNullableMarker = stringDictValueType.EndsWith("?") ? "?" : "";
                stringDictValueType = stringDictValueType.TrimEnd('?');

                isb.AppendLine($"if (saveObject.TryGetSaveObject({keyNameLiteral}, out var {varName}StrDict))");
                isb.OpenBrace(); // if TryGetSaveObject
                isb.AppendLine($"{targetProperty} = new {prop.Type.TrimEnd('?')}();");
                isb.AppendLine($"foreach (var kvp in {varName}StrDict.Properties)");
                isb.OpenBrace(); // foreach kvp

                if (prop.ElementSaveDataType == SaveDataType.Object)
                {
                    isb.AppendLine("if (kvp.Value is SaveObject objItem)");
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
                        isb.AppendLine("if (kvp.Value is Scalar<string> scalarString)");
                        isb.OpenBrace();
                        isb.AppendLine(
                            $"try {{ {targetProperty}.Add(kvp.Key, ({stringDictValueType}{stringDictValueTypeNullableMarker})Convert.ChangeType(scalarString.Value, typeof({stringDictValueType}))); }} catch {{}} // Fallback"
                        );

                        isb.CloseBrace();
                    }
                }

                isb.CloseBrace(); // End foreach
                isb.CloseBrace(); // End if TryGetSaveObject
                break;
            }

            // --- Repeated Key Types --- 
            case SaveDataType.RepeatedKeyStringList:
            case SaveDataType.FlatRepeatedKeyList:
            {
                isb.AppendLine($"{targetProperty} = saveObject.Properties");
                isb.Indent(); // Start LINQ expression indent
                isb.AppendLine($".Where(kvp => kvp.Key == {keyNameLiteral} && kvp.Value is Scalar<string>)");
                isb.AppendLine(".Select(kvp => ((Scalar<string>)kvp.Value).Value)");
                isb.AppendLine(".ToList();");
                isb.Unindent(); // End LINQ expression indent
                break;
            }

            case SaveDataType.FlatRepeatedObjectList:
            {
                string flatObjListElementType = prop.Type.TrimEnd('?').Replace("List<", "").Replace(">", "");
                isb.AppendLine($"{targetProperty} = saveObject.Properties");
                isb.Indent();
                isb.AppendLine($".Where(kvp => kvp.Key == {keyNameLiteral} && kvp.Value is SaveObject)");
                isb.AppendLine($".Select(kvp => {flatObjListElementType}.Load((SaveObject)kvp.Value))");
                isb.AppendLine(".ToList();");
                isb.Unindent();
                break;
            }

            case SaveDataType.RepeatedArrayList:
                isb.AppendLine($"{targetProperty} = saveObject.Properties");
                isb.Indent();
                isb.AppendLine($".Where(kvp => kvp.Key == {keyNameLiteral} && kvp.Value is SaveArray)");
                isb.AppendLine(".Select(kvp => ((SaveArray)kvp.Value).Items");
                isb.Indent(); // Indent for inner LINQ
                isb.AppendLine(".OfType<Scalar<string>>()");
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
        // Concatenate parent class name + PascalCased property name.
        string baseName = parentName + ToPascalCase(basePropertyName);
        string uniqueName = baseName;
        int counter = 1;
        var currentNames = new HashSet<string>(existingNames);

        while (currentNames.Contains(uniqueName))
        {
            uniqueName = baseName + counter++;
        }

        // Final check: ensure the generated unique name isn't a keyword
        if (SyntaxFacts.IsKeywordKind(SyntaxFacts.GetKeywordKind(uniqueName)) ||
            SyntaxFacts.IsContextualKeyword(SyntaxFacts.GetContextualKeywordKind(uniqueName)))
        {
            uniqueName = "@" + uniqueName;

            // Re-check uniqueness after adding @, although collisions are highly unlikely here
            while (currentNames.Contains(uniqueName))
            {
                uniqueName = baseName + counter++; // Regenerate without @ first
                uniqueName = "@" + uniqueName; // Then add @
            }
        }

        return uniqueName;
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
        // Example: Dictionary<int, List<string>?>, Dictionary<string, MyClass>
        int firstBracket = dictionaryType.IndexOf('<');
        int lastBracket = dictionaryType.LastIndexOf('>');

        if (firstBracket == -1 || lastBracket == -1 || lastBracket <= firstBracket)
        {
            return "object"; // Fallback if parsing fails
        }

        string genericArgs = dictionaryType.Substring(firstBracket + 1, lastBracket - firstBracket - 1);

        // Need to handle nested generics correctly when finding the comma
        int commaIndex = -1;
        int bracketLevel = 0;

        for (int i = 0; i < genericArgs.Length; i++)
        {
            if (genericArgs[i] == '<') bracketLevel++;
            else if (genericArgs[i] == '>') bracketLevel--;
            else if (genericArgs[i] == ',' && bracketLevel == 0)
            {
                commaIndex = i;
                break;
            }
        }

        if (commaIndex == -1)
        {
            return "object"; // Fallback if comma not found at the top level
        }

        return genericArgs.Substring(commaIndex + 1).Trim();
    }
}