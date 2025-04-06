using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace MageeSoft.PDX.CE.SourceGenerator;

[Generator]
public class IncrementalGenerator : IIncrementalGenerator
{
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
        var additionalTexts = context.AdditionalTextsProvider.Where(static additionalText => additionalText.Path.EndsWith(".csf", StringComparison.OrdinalIgnoreCase));
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

    private static List<Schema> InferSchema(SaveObject root, string rootClassName)
    {
        var schemas = new List<Schema>();
        var processedTypes = new Dictionary<string, Schema>();
        InferSchemaRecursive(root, rootClassName, schemas, processedTypes);
        return schemas;
    }

    private static Schema InferSchemaRecursive(SaveObject saveObject, string className, List<Schema> allSchemas, Dictionary<string, Schema> processedTypes)
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

    private static void InferProperties(SaveObject saveObject, Schema schema, List<Schema> allSchemas, Dictionary<string, Schema> processedTypes)
    {
        foreach (var kvp in saveObject.Properties)
        {
            string key = kvp.Key;
            SaveElement value = kvp.Value;
            string propertyName = ToPascalCase(key);

            var existingProperty = schema.Properties.FirstOrDefault(p => p.PropertyName == propertyName);

            if (existingProperty == null)
            {
                var property = InferPropertyType(key, propertyName, value, schema.Name, allSchemas, processedTypes);

                if (property != null)
                {
                    property.IsNullable = !(property.SaveDataType == SaveDataType.Int ||
                                            property.SaveDataType == SaveDataType.Long ||
                                            property.SaveDataType == SaveDataType.Float ||
                                            property.SaveDataType == SaveDataType.Bool ||
                                            property.SaveDataType == SaveDataType.DateTime ||
                                            property.SaveDataType == SaveDataType.Guid);

                    schema.Properties.Add(property);
                }
            }
            else
            {
                // TODO: Refine type/nullability based on multiple encounters
            }
        }
    }

    private static SchemaProperty? InferPropertyType(string key, string propertyName, SaveElement value, string parentClassName, List<Schema> allSchemas,
        Dictionary<string, Schema> processedTypes)
    {
        var property = new SchemaProperty
        {
            KeyName = key,
            PropertyName = propertyName
        };

        switch (value)
        {
            case Scalar<string>:
                property.Type = "string";
                property.SaveDataType = SaveDataType.String;
                break;
            case Scalar<int>:
                property.Type = "int";
                property.SaveDataType = SaveDataType.Int;
                break;
            case Scalar<long>:
                property.Type = "long";
                property.SaveDataType = SaveDataType.Long;
                break;
            case Scalar<float>:
                property.Type = "float";
                property.SaveDataType = SaveDataType.Float;
                break;
            case Scalar<bool>:
                property.Type = "bool";
                property.SaveDataType = SaveDataType.Bool;
                break;
            case Scalar<DateTime>:
                property.Type = "DateTime";
                property.SaveDataType = SaveDataType.DateTime;
                break;
            case Scalar<Guid>:
                property.Type = "Guid";
                property.SaveDataType = SaveDataType.Guid;
                break;
            case SaveObject obj:
            {
                // Dictionary<int, T> check
                if (obj.Properties.Count >= 0 && obj.Properties.All(p => int.TryParse(p.Key, out _))) // Allow empty dict { }
                {
                    if (obj.Properties.Count > 0 && obj.Properties.First().Value is SaveObject firstValueObj)
                    {
                        string dictValueClassName = GenerateUniqueClassName(parentClassName, propertyName + "Value", processedTypes.Keys);
                        Schema dictValueSchema = InferSchemaRecursive(firstValueObj, dictValueClassName, allSchemas, processedTypes);

                        // Merge properties from other values
                        foreach (var item in obj.Properties.Skip(1))
                        {
                            if (item.Value is SaveObject otherValueObj)
                            {
                                InferProperties(otherValueObj, dictValueSchema, allSchemas, processedTypes);
                            }
                        }

                        property.Type = $"Dictionary<int, {dictValueSchema.Name}>";
                        property.SaveDataType = SaveDataType.DictionaryIntKey;
                        property.NestedSchema = dictValueSchema;
                    }
                    else // Empty or non-object values
                    {
                        property.Type = "Dictionary<int, object>"; // Fallback for empty or non-object values
                        property.SaveDataType = SaveDataType.DictionaryIntKey;
                        property.NestedSchema = null;
                    }
                }
                else // Regular nested object
                {
                    string nestedClassName = GenerateUniqueClassName(parentClassName, propertyName, processedTypes.Keys);
                    Schema nestedSchema = InferSchemaRecursive(obj, nestedClassName, allSchemas, processedTypes);
                    property.Type = nestedSchema.Name;
                    property.SaveDataType = SaveDataType.Object;
                    property.NestedSchema = nestedSchema;
                }
            }

                break;
            case SaveArray arr:
                return InferArrayPropertyType(key, propertyName, arr, parentClassName, allSchemas, processedTypes);
            default: return null; // Skip unknown
        }

        return property;
    }

    private static SchemaProperty? InferArrayPropertyType(string key, string propertyName, SaveArray arr, string parentClassName, List<Schema> allSchemas,
        Dictionary<string, Schema> processedTypes)
    {
        if (arr.Items.Count == 0)
        {
            return new SchemaProperty
            {
                KeyName = key,
                PropertyName = propertyName,
                Type = "List<string>",
                SaveDataType = SaveDataType.Array,
                IsNullable = true,
                ElementSaveDataType = SaveDataType.String
            };
        }

        // Declare elementSchema once here
        Schema? elementSchema = null;

        // Dictionary<int, T> from Array check: { { id1 {...} } { id2 {...} } ... }
        bool isIdObjectDict = arr.Items.All(item =>
            item is SaveArray innerArr && innerArr.Items.Count == 2 &&
            innerArr.Items[0] is Scalar<int> && innerArr.Items[1] is SaveObject
        );

        if (isIdObjectDict)
        {
            SaveObject firstValueObj = (SaveObject)((SaveArray)arr.Items[0]).Items[1];
            string dictValueClassName = GenerateUniqueClassName(parentClassName, propertyName + "Value", processedTypes.Keys);
            // Assign to the existing elementSchema variable
            elementSchema = InferSchemaRecursive(firstValueObj, dictValueClassName, allSchemas, processedTypes);

            // Merge properties from other values
            foreach (var item in arr.Items.Skip(1))
            {
                if (item is SaveArray innerArr && innerArr.Items.Count == 2 && innerArr.Items[1] is SaveObject otherValueObj)
                {
                    InferProperties(otherValueObj, elementSchema, allSchemas, processedTypes);
                }
            }

            return new SchemaProperty
            {
                KeyName = key,
                PropertyName = propertyName,
                Type = $"Dictionary<int, {elementSchema.Name}>",
                SaveDataType = SaveDataType.DictionaryScalarKeyObjectValue,
                IsNullable = true,
                NestedSchema = elementSchema,
            };
        }

        // --- Regular Array Inference ---
        SaveElement firstItem = arr.Items[0];
        SaveDataType elementDataType = SaveDataType.Unknown;
        string elementType = "object";

        switch (firstItem)
        {
            case Scalar<string>:
                elementType = "string";
                elementDataType = SaveDataType.String;
                break;
            case Scalar<int>:
                elementType = "int";
                elementDataType = SaveDataType.Int;
                break;
            case Scalar<long>:
                elementType = "long";
                elementDataType = SaveDataType.Long;
                break;
            case Scalar<float>:
                elementType = "float";
                elementDataType = SaveDataType.Float;
                break;
            case Scalar<bool>:
                elementType = "bool";
                elementDataType = SaveDataType.Bool;
                break;
            case Scalar<DateTime>:
                elementType = "DateTime";
                elementDataType = SaveDataType.DateTime;
                break;
            case Scalar<Guid>:
                elementType = "Guid";
                elementDataType = SaveDataType.Guid;
                break;
            case SaveObject obj:
            {
                string elementClassName = GenerateUniqueClassName(parentClassName, propertyName + "Item", processedTypes.Keys);
                elementSchema = InferSchemaRecursive(obj, elementClassName, allSchemas, processedTypes);

                // Merge properties from other objects in array
                foreach (var item in arr.Items.Skip(1))
                {
                    if (item is SaveObject otherObj)
                    {
                        InferProperties(otherObj, elementSchema, allSchemas, processedTypes);
                    } // else: mixed types? Promote element type to object?
                }

                elementType = elementSchema.Name;
                elementDataType = SaveDataType.Object;
            }

                break;
            case SaveArray:
                elementType = "object";
                elementDataType = SaveDataType.Array;
                break; // Nested array -> List<object>
            default:
                elementType = "object";
                elementDataType = SaveDataType.Unknown;
                break;
        }

        // Refine scalar types if mixed
        bool containsFloat = arr.Items.Any(i => i is Scalar<float>);
        bool containsLong = arr.Items.Any(i => i is Scalar<long>);
        bool containsInt = arr.Items.Any(i => i is Scalar<int>);

        if (elementDataType == SaveDataType.Int || elementDataType == SaveDataType.Long || elementDataType == SaveDataType.Float)
        {
            if (containsFloat)
            {
                elementType = "float";
                elementDataType = SaveDataType.Float;
            }
            else if (containsLong)
            {
                elementType = "long";
                elementDataType = SaveDataType.Long;
            }
            else if (containsInt)
            {
                elementType = "int";
                elementDataType = SaveDataType.Int;
            }
        }

        return new SchemaProperty
        {
            KeyName = key,
            PropertyName = propertyName,
            Type = $"List<{elementType}>",
            SaveDataType = SaveDataType.Array,
            IsNullable = true,
            ElementSaveDataType = elementDataType,
            NestedSchema = elementSchema
        };
    }

    // --- Code Generation ---

    private static StringBuilder GenerateCSharpCode(List<Schema> schemas, string rootClassName, ClassDeclarationSyntax partialClass, Compilation compilation)
    {
        var sb = new StringBuilder();
        var syntaxTree = partialClass.SyntaxTree;
        var rootNode = syntaxTree.GetRoot();
        var usingDirectives = rootNode.DescendantNodes().OfType<UsingDirectiveSyntax>();
        var namespaceDeclaration = partialClass.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();
        string? namespaceName = namespaceDeclaration?.Name.ToString() ?? "GeneratedModels";

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        // Standard usings required by the generated code
        var standardUsings = new HashSet<string> {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "MageeSoft.PDX.CE"
        };

        foreach (var ns in standardUsings) {
            sb.AppendLine($"using {ns};");
        }

        // Add using directives from the original file, avoiding duplicates
        foreach (var usingDirective in usingDirectives)
        {
            // Get the full namespace string (handles aliases etc.)
            string usingNamespace = usingDirective.Name.ToString();
            if (standardUsings.Add(usingNamespace)) // .Add returns false if item already exists
            {
                sb.AppendLine(usingDirective.ToString());
            }
        }
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine("{");

        Schema? rootSchema = schemas.FirstOrDefault(s => s.Name == rootClassName);

        if (rootSchema == null)
        {
            sb.AppendLine($"    // ERROR: Root schema '{rootClassName}' not found.");
            sb.AppendLine("}");
            return sb;
        }

        var generatedClassNames = new HashSet<string>();
        
        GenerateClassRecursive(sb, rootSchema, schemas, generatedClassNames, isRoot: true, indentationLevel: 1);

        sb.AppendLine("}"); // end namespace
        return sb;
    }

    private static void GenerateClassRecursive(StringBuilder sb, Schema schema, List<Schema> allSchemas, HashSet<string> generatedClassNames, bool isRoot, int indentationLevel)
    {
        if (generatedClassNames.Contains(schema.Name)) return;
        
        generatedClassNames.Add(schema.Name);

        string indent = new string(' ', indentationLevel * 4);
        string classModifiers = isRoot ? "public partial class" : "public class";

        sb.AppendLine($"{indent}{classModifiers} {schema.Name}");
        sb.AppendLine($"{indent}{{");
        string innerIndent = indent + "    ";
        string methodIndent = innerIndent + "    ";

        // Properties
        foreach (var prop in schema.Properties.OrderBy(p => p.PropertyName))
        {
            string nullableMarker = prop.IsNullable ? "?" : "";
            string initializer = (prop.SaveDataType == SaveDataType.Array ||
                                  prop.SaveDataType == SaveDataType.DictionaryIntKey ||
                                  prop.SaveDataType == SaveDataType.DictionaryStringKey ||
                                  prop.SaveDataType == SaveDataType.DictionaryScalarKeyObjectValue)
                ? " = new();"
                : "";

            sb.AppendLine($"{innerIndent}/// <summary>Original key: {prop.KeyName}</summary>");
            sb.AppendLine($"{innerIndent}public {prop.Type}{nullableMarker} {prop.PropertyName} {{ get; set; }}{initializer}");
            sb.AppendLine();
        }

        // Static Load method
        sb.AppendLine($"{innerIndent}/// <summary>Loads data from a SaveObject into a new {schema.Name} instance.</summary>");
        sb.AppendLine($"{innerIndent}public static {schema.Name} Load(SaveObject saveObject)");
        sb.AppendLine($"{innerIndent}{{");
        sb.AppendLine($"{methodIndent}var model = new {schema.Name}();");

        // Binding logic
        foreach (var prop in schema.Properties.OrderBy(p => p.PropertyName))
        {
            string keyNameLiteral = ToCSharpStringLiteral(prop.KeyName);
            string targetProperty = $"model.{prop.PropertyName}";
            string varName = ToCamelCase(prop.PropertyName);
            string typeWithoutNullable = prop.Type.TrimEnd('?');

            switch (prop.SaveDataType)
            {
                case SaveDataType.String:
                case SaveDataType.Int:
                case SaveDataType.Long:
                case SaveDataType.Float:
                case SaveDataType.Bool:
                case SaveDataType.DateTime:
                    string tryGetMethod = GetTryGetMethodName(prop.SaveDataType);
                    // Call specific TryGetX method (e.g., TryGetInt)
                    // The out parameter will be nullable (e.g., out int? value)
                    // Use simple string concatenation for the if statement line itself
                    sb.AppendLine(methodIndent + "if (saveObject." + tryGetMethod + "(" + keyNameLiteral + ", out " + typeWithoutNullable + "? " + varName + "))");
                    // Keep the rest of the block using interpolation or original structure
                    sb.AppendLine($"{methodIndent}{{");
                    if (prop.IsNullable)
                    {
                        sb.AppendLine($"{methodIndent}    {targetProperty} = {varName};");
                    }
                    else
                    {
                        sb.AppendLine($"{methodIndent}    {targetProperty} = {varName}!.Value;");
                    }
                    sb.AppendLine($"{methodIndent}}}");
                    break;

                case SaveDataType.Object:
                    // Use original interpolation/structure for non-scalar types
                    sb.AppendLine($"{methodIndent}if (saveObject.TryGet<SaveObject>({keyNameLiteral}, out var {varName}Obj) && {varName}Obj != null)");
                    sb.AppendLine($"{methodIndent}{{");
                    if (prop.NestedSchema != null)
                    {
                        sb.AppendLine($"{methodIndent}    {targetProperty} = {prop.NestedSchema.Name}.Load({varName}Obj);");
                    }
                    else
                    {
                        sb.AppendLine($"{methodIndent}    // Warning: Nested schema for '{prop.PropertyName}' is null.");
                    }
                    sb.AppendLine($"{methodIndent}}}");
                    break;

                case SaveDataType.Array:
                    sb.AppendLine($"{methodIndent}if (saveObject.TryGet<SaveArray>({keyNameLiteral}, out var {varName}Arr) && {varName}Arr != null)");
                    sb.AppendLine($"{methodIndent}{{");
                    if (prop.ElementSaveDataType == SaveDataType.Object && prop.NestedSchema != null)
                    {
                        sb.AppendLine($"{methodIndent}    {targetProperty} = {varName}Arr.Items.OfType<SaveObject>().Select({prop.NestedSchema.Name}.Load).ToList();");
                    }
                    else if (prop.ElementSaveDataType != SaveDataType.Unknown && prop.ElementSaveDataType != SaveDataType.Object)
                    {
                        string scalarType = GetScalarTypeName(prop.ElementSaveDataType);
                        sb.AppendLine($"{methodIndent}    {targetProperty} = {varName}Arr.Items.OfType<Scalar<{scalarType}>>().Select(s => s.Value).ToList();");
                    }
                    else
                    {
                        sb.AppendLine($"{methodIndent}    // Warning: Cannot determine array element type for '{prop.PropertyName}'. Assigning empty list.");
                        sb.AppendLine($"{methodIndent}    {targetProperty} ??= new();"); // Ensure initialized
                    }
                    sb.AppendLine($"{methodIndent}}}");
                    break;

                case SaveDataType.DictionaryIntKey:
                    sb.AppendLine($"{methodIndent}if (saveObject.TryGet<SaveObject>({keyNameLiteral}, out var {varName}DictObj) && {varName}DictObj != null)");
                    sb.AppendLine($"{methodIndent}{{");
                    if (prop.NestedSchema != null)
                    {
                        sb.AppendLine($"{methodIndent}    {targetProperty} = {varName}DictObj.Properties");
                        sb.AppendLine($"{methodIndent}        .Where(kv => int.TryParse(kv.Key, out _) && kv.Value is SaveObject)");
                        sb.AppendLine($"{methodIndent}        .ToDictionary(");
                        sb.AppendLine($"{methodIndent}            kv => int.Parse(kv.Key),");
                        sb.AppendLine($"{methodIndent}            kv => {prop.NestedSchema.Name}.Load((SaveObject)kv.Value));");
                    }
                    else
                    {
                        sb.AppendLine($"{methodIndent}    // Warning: Nested schema for DictionaryIntKey '{prop.PropertyName}' is null.");
                        sb.AppendLine($"{methodIndent}    {targetProperty} ??= new();");
                    }
                    sb.AppendLine($"{methodIndent}}}");
                    break;

                case SaveDataType.DictionaryScalarKeyObjectValue: // From Array { {key, obj}, ... }
                    sb.AppendLine($"{methodIndent}if (saveObject.TryGet<SaveArray>({keyNameLiteral}, out var {varName}IdArr) && {varName}IdArr != null)");
                    sb.AppendLine($"{methodIndent}{{");
                    if (prop.NestedSchema != null)
                    {
                        string keyScalarType = "int"; // Assuming int keys
                        sb.AppendLine($"{methodIndent}    try {{");
                        sb.AppendLine($"{methodIndent}        {targetProperty} = {varName}IdArr.Items.OfType<SaveArray>()");
                        sb.AppendLine($"{methodIndent}            .Where(innerArr => innerArr.Items.Count == 2 && innerArr.Items[0] is Scalar<{keyScalarType}> && innerArr.Items[1] is SaveObject)");
                        sb.AppendLine($"{methodIndent}            .ToDictionary(");
                        sb.AppendLine($"{methodIndent}                keySelector: sa => ((Scalar<{keyScalarType}>)sa.Items[0]).Value,");
                        sb.AppendLine($"{methodIndent}                elementSelector: sa => {prop.NestedSchema.Name}.Load((SaveObject)sa.Items[1]));");
                        sb.AppendLine($"{methodIndent}    }} catch (Exception ex) {{");
                        sb.AppendLine($"{methodIndent}        Console.WriteLine($\"Error parsing dictionary '{prop.KeyName}': {{ex.Message}}\");");
                        sb.AppendLine($"{methodIndent}        {targetProperty} ??= new();"); // Ensure initialized on error
                        sb.AppendLine($"{methodIndent}    }}");
                    }
                    else
                    {
                        sb.AppendLine($"{methodIndent}    // Warning: Nested schema for DictionaryScalarKeyObjectValue '{prop.PropertyName}' is null.");
                        sb.AppendLine($"{methodIndent}    {targetProperty} ??= new();");
                    }
                    sb.AppendLine($"{methodIndent}}}");
                    break;

                default:
                    sb.AppendLine($"{methodIndent}// TODO: Load logic for {prop.SaveDataType} property '{prop.PropertyName}'");
                    break;
            }

            sb.AppendLine(); // Blank line between property loads
        }

        sb.AppendLine($"{methodIndent}return model;");
        sb.AppendLine($"{innerIndent}}}"); // end Load method

        // Generate nested classes
        var nestedSchemasToGenerate = schema.Properties
            .Where(p => p.NestedSchema != null)
            .Select(p => p.NestedSchema)
            .Distinct();

        foreach (var nestedSchema in nestedSchemasToGenerate.OrderBy(ns => ns!.Name))
        {
            sb.AppendLine();
            GenerateClassRecursive(sb, nestedSchema!, allSchemas, generatedClassNames, isRoot: false, indentationLevel + 1);
        }

        sb.AppendLine($"{indent}}}"); // end class
    }

    // --- Helpers ---
    private static string GetTryGetMethodName(SaveDataType dataType) => dataType switch
    {
        SaveDataType.String => "TryGetString",
        SaveDataType.Int => "TryGetInt",
        SaveDataType.Long => "TryGetLong",
        SaveDataType.Float => "TryGetFloat",
        SaveDataType.Bool => "TryGetBool",
        SaveDataType.DateTime => "TryGetDateTime",
        SaveDataType.Guid => "TryGetGuid",
        // Add cases for SaveObject/SaveArray if needed, though currently handled differently
        _ => throw new ArgumentOutOfRangeException(nameof(dataType), $"No corresponding TryGet method for {dataType}")
    };

    private static string GetScalarTypeName(SaveDataType dataType) => dataType switch
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

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return "_";
        // Replace invalid chars, normalize separators, handle potential leading digits
        string sanitized = Regex.Replace(input, @"[^\w\s_-]", ""); // Remove chars not word, whitespace, underscore, hyphen
        sanitized = Regex.Replace(sanitized, @"[ \s_-]+", " ").Trim(); // Normalize separators to space
        if (string.IsNullOrEmpty(sanitized)) return "_";
        if (char.IsDigit(sanitized[0])) sanitized = "_" + sanitized; // Prepend underscore if starts with digit

        string[] parts = sanitized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        StringBuilder pascalCase = new StringBuilder();
        foreach (string part in parts) {
            if (part.Length > 0) {
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

    private static string ToCamelCase(string input)
    {
        string pascal = ToPascalCase(input);
        if (string.IsNullOrEmpty(pascal) || pascal == "_") return "_var";

        string result;
        if (pascal.StartsWith("@")) {
            // Input was already a keyword like @Class
            // Make it @class
            if (pascal.Length > 1) {
                result = "@" + char.ToLowerInvariant(pascal[1]) + pascal.Substring(2);
            }
            else {
                result = pascal; // Should be just "@", unlikely
            }
        }
        // Standard camelCase conversion (e.g., "MyProperty" -> "myProperty")
        else if (pascal.Length > 0 && char.IsUpper(pascal[0]))
        {
            result = char.ToLowerInvariant(pascal[0]) + pascal.Substring(1);
        }
        else {
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

    private static string GenerateUniqueClassName(string parentName, string basePropertyName, IEnumerable<string> existingNames)
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

    private static string ToCSharpStringLiteral(string value)
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
        public Schema? NestedSchema { get; set; } // For objects or elements/values that are objects
    }

    public enum SaveDataType
    {
        Unknown,
        String,
        Int,
        Long,
        Float,
        Bool,
        DateTime,
        Guid,
        Object, // SaveObject -> class
        Array, // SaveArray -> List<T>
        DictionaryIntKey, // SaveObject { 0={}, 1={}, ... } -> Dictionary<int, T>
        DictionaryStringKey, // SaveObject { "a"={}, "b"={}, ... } -> Dictionary<string, T> (if needed)
        DictionaryScalarKeyObjectValue // SaveArray { { key1, {...} }, { key2, {...} } } -> Dictionary<TKey, TValue>
    }


    public class ClassSchemaPair
    {
        public ClassDeclarationSyntax? ClassDeclaration { get; set; }
        public string? SchemaFileName { get; set; }
    }
}