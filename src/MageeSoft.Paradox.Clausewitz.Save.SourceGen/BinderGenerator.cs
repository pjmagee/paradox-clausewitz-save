using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MageeSoft.Paradox.Clausewitz.Save.SourceGen;

/*
 * CASCADING BINDING ARCHITECTURE
 * 
 * The binding system works by generating Bind() methods for each class annotated with [SaveModel].
 * These Bind methods form a cascading hierarchy that enables hierarchical object binding:
 * 
 * 1. Root objects like GameState and Meta have a static Bind(SaveObject) method that initializes the root object
 * 2. Sub-objects marked with [SaveObject] are bound by calling their Bind method recursively
 * 3. Collections of objects use the appropriate binding method for each element:
 *    - [SaveArray] of complex objects -> bind each element using its Bind method
 *    - [SaveIndexedDictionary] with complex values -> bind each value using its Bind method
 * 
 * This architecture enables automatic cascading binding through the entire object graph, starting
 * from a single call to the root object's Bind method.
 * 
 * ISSUES THAT NEED TO BE FIXED:
 * 
 * 1. Dictionary<TKey, TValue> support:
 *    - Currently generates code for regular Dictionary but not ImmutableDictionary
 *    - Needs proper type checking for TKey and TValue
 *    - Needs special handling for int keys (parsing int instead of using TKey.Parse)
 *    
 * 2. Complex object binding:
 *    - Need to properly detect when a type has a Bind() method available
 *    - For SaveIndexedDictionary with complex values, need to invoke the Bind() method
 *    - Need to handle arrays of complex objects correctly
 *    
 * 3. Required properties initialization:
 *    - Default initialization of required properties sometimes doesn't work
 *    - Need better default value generation for complex objects
 *    
 * 4. Dictionary<long/int, TValue> special handling:
 *    - SaveIndexedDictionary with long keys needs custom int.Parse/long.Parse logic
 */

[Generator]
public class BinderGenerator : IIncrementalGenerator
{
    private const string SaveScalarAttributeName = "MageeSoft.Paradox.Clausewitz.Save.Models.SaveScalarAttribute";
    private const string SaveArrayAttributeName = "MageeSoft.Paradox.Clausewitz.Save.Models.SaveArrayAttribute";
    private const string SaveObjectAttributeName = "MageeSoft.Paradox.Clausewitz.Save.Models.SaveObjectAttribute";
    private const string SaveIndexedDictionaryAttributeName = "MageeSoft.Paradox.Clausewitz.Save.Models.SaveIndexedDictionaryAttribute";
    private const string SaveModelAttributeName = "MageeSoft.Paradox.Clausewitz.Save.Models.SaveModelAttribute";
    
    // List of simple types that don't need complex binding
    private readonly static HashSet<string> SimpleTypes = new()
    {
        "string", "System.String",
        "int", "System.Int32",
        "long", "System.Int64",
        "float", "System.Single",
        "double", "System.Double",
        "bool", "System.Boolean",
        "DateOnly", "System.DateOnly",
        "Guid", "System.Guid"
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        var compilation = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilation,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var model = context.SemanticModel;
        var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        
        if (classSymbol == null)
            return null;
            
        // Check if the class has the SaveModel attribute
        var attributes = classSymbol.GetAttributes();
        var hasSaveModelAttribute = attributes.Any(a => 
            a.AttributeClass?.Name == "SaveModelAttribute" || 
            a.AttributeClass?.Name == "SaveModel");
            
        if (hasSaveModelAttribute)
        {
            return classDeclaration;
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        // Keep track of processed classes by full qualified name to avoid duplicates
        var processedClasses = new HashSet<string>();

        foreach (var classDeclaration in classes)
        {
            var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
            if (classSymbol != null)
            {
                var className = classSymbol.ToDisplayString();
                var classSyntaxTree = classDeclaration.SyntaxTree.FilePath;
                var classKey = $"{className}|{classSyntaxTree}";
                
                // Skip if we've already processed this class
                if (processedClasses.Contains(classKey))
                    continue;
                    
                processedClasses.Add(classKey);
                
                // Check if this class has any SaveScalar, SaveArray, etc. attributes on its properties
                var properties = GetPropertiesWithAttributes(classSymbol);
                if (properties.Any())
                {
                    GenerateBinderForClass(classSymbol, properties, context);
                }
            }
        }
    }

    private static void GenerateBinderForClass(INamedTypeSymbol classSymbol, IEnumerable<(IPropertySymbol Property, AttributeData Attribute)> properties, SourceProductionContext context)
    {
        // Clear helper methods for this new class
        HelperMethods.Clear();
        
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var className = classSymbol.Name;
        var allProperties = classSymbol.GetMembers().OfType<IPropertySymbol>().ToList();
        var requiredProperties = allProperties.Where(p => p.IsRequired).ToList();

        var sourceTextBuilder = new StringBuilder();
        sourceTextBuilder.AppendLine($$"""
// <auto-generated/>
#nullable enable
using MageeSoft.Paradox.Clausewitz.Save.Parser;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace {{namespaceName}}
{
    partial class {{className}}
    {
        /// <summary>
        /// Binds a SaveObject to a {{className}} instance.
        /// </summary>
        /// <param name="obj">The SaveObject to bind</param>
        /// <returns>A new {{className}} instance with properties set from the SaveObject</returns>
        public static {{className}} Bind(SaveObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            try
            {
                return new {{className}}
                {
{{GeneratePropertyBindings(properties, requiredProperties)}}
                };
            }
            catch (System.Exception exception)
            {
                throw new System.Exception($"Error binding {nameof({{className}})}", exception);
            }
        }
""");

        // Add any helper methods needed for array binding
        foreach (var helperMethod in HelperMethods)
        {
            sourceTextBuilder.Append(helperMethod);
        }
        
        // Close the class and namespace
        sourceTextBuilder.AppendLine("    }");
        sourceTextBuilder.AppendLine("}");

        // Parse the source text into a syntax tree for formatting
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceTextBuilder.ToString());
        
        // Get the formatted syntax tree with proper whitespace normalization
        var formattedRoot = syntaxTree.GetRoot().NormalizeWhitespace();
        var formattedSourceText = formattedRoot.GetText(Encoding.UTF8);
        
        // Add the source with proper formatting
        context.AddSource($"{className}.g.cs", formattedSourceText);
    }

    private static string GeneratePropertyBindings(IEnumerable<(IPropertySymbol Property, AttributeData Attribute)> properties, IEnumerable<IPropertySymbol> requiredProperties)
    {
        var sb = new StringBuilder();
        // Use a hash set to track properties we've already processed
        var processedProperties = new HashSet<string>();
        
        // First handle initialization of required properties that aren't marked with Save* attributes
        foreach (var requiredProperty in requiredProperties)
        {
            var propertyKey = $"{requiredProperty.ContainingType.ToDisplayString()}.{requiredProperty.Name}";
            
            // Skip if this property will be handled later by Save* attributes
            if (properties.Any(p => p.Property.Name == requiredProperty.Name))
                continue;
                
            // Skip if we've already processed this property
            if (processedProperties.Contains(propertyKey))
                continue;
                
            processedProperties.Add(propertyKey);
            
            sb.AppendLine($"                    {requiredProperty.Name} = {GetDefaultValueForType(requiredProperty.Type)},");
        }
        
        // Now handle properties with Save* attributes
        foreach (var (property, attribute) in properties)
        {
            var propertyName = property.Name;
            var propertyKey = $"{property.ContainingType.ToDisplayString()}.{propertyName}";
            
            // Skip if we've already processed this property
            if (processedProperties.Contains(propertyKey))
                continue;
                
            processedProperties.Add(propertyKey);
            
            // Get the property key name from the attribute
            string? attributePropertyKey = null;
            if (attribute.NamedArguments.Any(na => na.Key == "Name"))
            {
                attributePropertyKey = attribute.NamedArguments.First(na => na.Key == "Name").Value.Value?.ToString();
            }
            else if (attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0].Value != null)
            {
                attributePropertyKey = attribute.ConstructorArguments[0].Value?.ToString();
            }

            // Fall back to property name if no custom name is specified
            attributePropertyKey ??= propertyName;

            var attributeType = attribute.AttributeClass?.ToDisplayString();
            var propertyType = property.Type.ToDisplayString();
            var varName = propertyName.ToLower();

            // Check if property is read-only or has an init setter
            bool isReadOnly = property.IsReadOnly;
            bool hasInitSetter = property.SetMethod?.IsInitOnly ?? false;

            if (isReadOnly && !hasInitSetter)
            {
                // Skip properties that are read-only and don't have an init setter
                continue;
            }

            switch (attributeType)
            {
                case SaveScalarAttributeName:
                    GenerateScalarBinding(sb, propertyName, attributePropertyKey, varName, property.Type);
                    break;

                case SaveArrayAttributeName:
                    GenerateArrayBinding(sb, propertyName, attributePropertyKey, varName, property.Type);
                    break;

                case SaveObjectAttributeName:
                    GenerateObjectBinding(sb, propertyName, attributePropertyKey, varName, property.Type);
                    break;

                case SaveIndexedDictionaryAttributeName:
                    GenerateDictionaryBinding(sb, propertyName, attributePropertyKey, varName, property.Type);
                    break;
            }
        }
        
        return sb.ToString();
    }

    private static void GenerateScalarBinding(StringBuilder sb, string propertyName, string propertyKey, string varName, ITypeSymbol propertyType)
    {
        string typeName = propertyType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var nullableUnderlyingType = GetNullableUnderlyingType(propertyType);
        bool isNullable = nullableUnderlyingType != null || propertyType.NullableAnnotation == NullableAnnotation.Annotated || propertyType.IsReferenceType;
        
        string underlyingTypeName = isNullable && nullableUnderlyingType != null
            ? nullableUnderlyingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
            : typeName;
            
        // Check for each specific scalar type
        if (underlyingTypeName == "bool" || underlyingTypeName == "Boolean")
        {
            sb.AppendLine($"                    {propertyName} = obj.TryGetBool(\"{propertyKey}\", out var {varName}) ? {varName} : {GetDefaultValueForType(propertyType)},");
        }
        else if (underlyingTypeName == "int" || underlyingTypeName == "Int32")
        {
            sb.AppendLine($"                    {propertyName} = obj.TryGetInt(\"{propertyKey}\", out var {varName}) ? {varName} : {GetDefaultValueForType(propertyType)},");
        }
        else if (underlyingTypeName == "long" || underlyingTypeName == "Int64")
        {
            sb.AppendLine($"                    {propertyName} = obj.TryGetLong(\"{propertyKey}\", out var {varName}) ? {varName} : {GetDefaultValueForType(propertyType)},");
        }
        else if (underlyingTypeName == "float" || underlyingTypeName == "Single")
        {
            sb.AppendLine($"                    {propertyName} = obj.TryGetFloat(\"{propertyKey}\", out var {varName}) ? {varName} : {GetDefaultValueForType(propertyType)},");
        }
        else if (underlyingTypeName == "string" || underlyingTypeName == "String")
        {
            sb.AppendLine($"                    {propertyName} = obj.TryGetString(\"{propertyKey}\", out var {varName}) ? ({varName} == \"none\" ? null : {varName}) : {GetDefaultValueForType(propertyType)},");
        }
        else if (underlyingTypeName == "Guid")
        {
            sb.AppendLine($"                    {propertyName} = obj.TryGetGuid(\"{propertyKey}\", out var {varName}) ? {varName} : {GetDefaultValueForType(propertyType)},");
        }
        else if (underlyingTypeName == "DateOnly")
        {
            sb.AppendLine($"                    {propertyName} = obj.TryGetDateOnly(\"{propertyKey}\", out var {varName}) ? {varName} : {GetDefaultValueForType(propertyType)},");
        }
        else if (underlyingTypeName.Contains("Enum"))
        {
            // Handle enum types
            sb.AppendLine($"                    {propertyName} = obj.TryGetInt(\"{propertyKey}\", out var {varName}Int) ? ({typeName}){varName}Int : {GetDefaultValueForType(propertyType)},");
        }
        else
        {
            // For other types, use a generic approach with explicit type checks
            sb.AppendLine($"                    {propertyName} = obj.Properties.FirstOrDefault(p => p.Key == \"{propertyKey}\").Value is Scalar<{underlyingTypeName}> {varName} ? {varName}.Value : {GetDefaultValueForType(propertyType)},");
        }
    }

    private static void GenerateArrayBinding(StringBuilder sb, string propertyName, string propertyKey, string varName, ITypeSymbol propertyType)
    {
        string elementTypeName;
        bool isList = false;
        bool isArray = false;
        INamedTypeSymbol? elementNamedType = null;
        ITypeSymbol? elementTypeSymbol = null;
        bool isElementValueType = false;
        
        // Determine the type of collection and the element type
        if (propertyType is IArrayTypeSymbol arrayTypeSymbol)
        {
            elementTypeName = arrayTypeSymbol.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            elementTypeSymbol = arrayTypeSymbol.ElementType;
            isArray = true;
            isElementValueType = elementTypeSymbol.IsValueType;
        }
        else if (propertyType is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.TypeArguments.Length > 0)
        {
            elementTypeName = namedType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            elementTypeSymbol = namedType.TypeArguments[0];
            elementNamedType = namedType.TypeArguments[0] as INamedTypeSymbol;
            isElementValueType = elementTypeSymbol.IsValueType;
            
            if (namedType.ToDisplayString().Contains("List"))
            {
                isList = true;
            }
        }
        else
        {
            sb.AppendLine($"                    {propertyName} = {GetDefaultValueForType(propertyType)}, // Unknown collection type");
            return;
        }
        
        // Always treat element types as potentially nullable unless they are value types
        bool isNullableElementType = !isElementValueType;
        
        bool hasBindMethod = elementTypeSymbol != null && HasBindMethod(elementTypeSymbol); 
        
        // Use a helper method to bind array properties
        sb.AppendLine($"                    {propertyName} = obj.Properties.Any(p => p.Key == \"{propertyKey}\") ? Bind{propertyName}FromSaveObject(obj) : {GetDefaultValueForType(propertyType)},");
        
        // Create a helper method for binding this array property
        StringBuilder methodSb = new StringBuilder();
        methodSb.AppendLine();
        methodSb.AppendLine($"        private static {propertyType.ToDisplayString()} Bind{propertyName}FromSaveObject(SaveObject obj)");
        methodSb.AppendLine($"        {{");
        methodSb.AppendLine($"            if (obj == null)");
        methodSb.AppendLine($"                return {GetDefaultValueForType(propertyType)};");
        methodSb.AppendLine($"                ");
        methodSb.AppendLine($"            var elementsList = new List<{elementTypeName}>();");
        
        // First try using the direct array accessor
        methodSb.AppendLine($"            // First try direct array access");
        methodSb.AppendLine($"            if (obj.TryGetSaveArray(\"{propertyKey}\", out var arrayValue))");
        methodSb.AppendLine($"            {{");
        methodSb.AppendLine($"                if (arrayValue == null)");
        methodSb.AppendLine($"                    return {GetDefaultValueForType(propertyType)};");
        methodSb.AppendLine($"                    ");
        methodSb.AppendLine($"                foreach (var item in arrayValue.Items)");
        methodSb.AppendLine($"                {{");
        
        if (hasBindMethod)
        {
            methodSb.AppendLine($"                    if (item is SaveObject itemObj)");
            methodSb.AppendLine($"                    {{");
            methodSb.AppendLine($"                        elementsList.Add({elementTypeName}.Bind(itemObj));");
            methodSb.AppendLine($"                    }}");
        }
        else if (IsSimpleType(elementTypeName))
        {
            // Determine what scalar type to look for based on element type
            string scalarTypeName = GetScalarTypeForElementType(elementTypeName);
            
            methodSb.AppendLine($"                    if (item is Scalar<{scalarTypeName}> scalar)");
            methodSb.AppendLine($"                    {{");
            methodSb.AppendLine($"                        elementsList.Add(scalar.Value);");
            methodSb.AppendLine($"                    }}");
            
            if (isNullableElementType)
            {
                methodSb.AppendLine($"                    else if (item.ToString() == \"none\")");
                methodSb.AppendLine($"                    {{");
                methodSb.AppendLine($"                        elementsList.Add(default({elementTypeName}));");
                methodSb.AppendLine($"                    }}");
            }
        }
        
        methodSb.AppendLine($"                }}");
        methodSb.AppendLine($"            }}");
        methodSb.AppendLine($"            else");
        methodSb.AppendLine($"            {{");
        methodSb.AppendLine($"                // Fall back to property iteration for repeating keys");
        methodSb.AppendLine($"                foreach (var prop in obj.Properties.Where(p => p.Key == \"{propertyKey}\"))");
        methodSb.AppendLine($"                {{");
        
        // Handle nested object case for individual properties
        methodSb.AppendLine($"                    if (prop.Value is SaveObject objElement)");
        methodSb.AppendLine($"                    {{");
        
        if (hasBindMethod)
        {
            methodSb.AppendLine($"                        elementsList.Add({elementTypeName}.Bind(objElement));");
        }
        else if (isNullableElementType)
        {
            methodSb.AppendLine($"                        // Handling a SaveObject for a non-object element type with nullable reference");
            methodSb.AppendLine($"                        elementsList.Add(default({elementTypeName}));");
        }
        else
        {
            methodSb.AppendLine($"                        // Skip - expected scalar but got object");
        }
        
        methodSb.AppendLine($"                    }}");
        methodSb.AppendLine($"                    else if (prop.Value is SaveArray arrayElement)");
        methodSb.AppendLine($"                    {{");
        methodSb.AppendLine($"                        foreach (var item in arrayElement.Items)");
        methodSb.AppendLine($"                        {{");
        
        if (hasBindMethod)
        {
            methodSb.AppendLine($"                            if (item is SaveObject itemObj)");
            methodSb.AppendLine($"                            {{");
            methodSb.AppendLine($"                                elementsList.Add({elementTypeName}.Bind(itemObj));");
            methodSb.AppendLine($"                            }}");
        }
        else if (IsSimpleType(elementTypeName))
        {
            // Determine what scalar type to look for based on element type
            string scalarTypeName = GetScalarTypeForElementType(elementTypeName);
            
            methodSb.AppendLine($"                            if (item is Scalar<{scalarTypeName}> scalar)");
            methodSb.AppendLine($"                            {{");
            methodSb.AppendLine($"                                elementsList.Add(scalar.Value);");
            methodSb.AppendLine($"                            }}");
            
            if (isNullableElementType)
            {
                methodSb.AppendLine($"                            else if (item.ToString() == \"none\")");
                methodSb.AppendLine($"                            {{");
                methodSb.AppendLine($"                                elementsList.Add(default({elementTypeName}));");
                methodSb.AppendLine($"                            }}");
            }
        }
        
        methodSb.AppendLine($"                        }}");
        methodSb.AppendLine($"                    }}");
        methodSb.AppendLine($"                    else if (prop.Value is SaveElement element)");
        methodSb.AppendLine($"                    {{");
        
        if (IsSimpleType(elementTypeName))
        {
            // Handle simple scalar types with proper prefixes to avoid variable conflicts
            string propPrefix = propertyName.ToLower();
            
            if (elementTypeName.EndsWith("string", StringComparison.OrdinalIgnoreCase))
            {
                methodSb.AppendLine($"                        if (element is Scalar<string> {propPrefix}_str)");
                methodSb.AppendLine($"                        {{");
                methodSb.AppendLine($"                            elementsList.Add({propPrefix}_str.Value);");
                methodSb.AppendLine($"                        }}");
            }
            else if (elementTypeName.EndsWith("int", StringComparison.OrdinalIgnoreCase))
            {
                methodSb.AppendLine($"                        if (element is Scalar<int> {propPrefix}_int)");
                methodSb.AppendLine($"                        {{");
                methodSb.AppendLine($"                            elementsList.Add({propPrefix}_int.Value);");
                methodSb.AppendLine($"                        }}");
            }
            else if (elementTypeName.EndsWith("long", StringComparison.OrdinalIgnoreCase))
            {
                methodSb.AppendLine($"                        if (element is Scalar<long> {propPrefix}_long)");
                methodSb.AppendLine($"                        {{");
                methodSb.AppendLine($"                            elementsList.Add({propPrefix}_long.Value);");
                methodSb.AppendLine($"                        }}");
            }
            else if (elementTypeName.EndsWith("float", StringComparison.OrdinalIgnoreCase))
            {
                methodSb.AppendLine($"                        if (element is Scalar<float> {propPrefix}_float)");
                methodSb.AppendLine($"                        {{");
                methodSb.AppendLine($"                            elementsList.Add({propPrefix}_float.Value);");
                methodSb.AppendLine($"                        }}");
            }
            else if (elementTypeName.EndsWith("bool", StringComparison.OrdinalIgnoreCase))
            {
                methodSb.AppendLine($"                        if (element is Scalar<bool> {propPrefix}_bool)");
                methodSb.AppendLine($"                        {{");
                methodSb.AppendLine($"                            elementsList.Add({propPrefix}_bool.Value);");
                methodSb.AppendLine($"                        }}");
            }
            else if (elementTypeName.EndsWith("Guid", StringComparison.OrdinalIgnoreCase))
            {
                methodSb.AppendLine($"                        if (element is Scalar<Guid> {propPrefix}_guid)");
                methodSb.AppendLine($"                        {{");
                methodSb.AppendLine($"                            elementsList.Add({propPrefix}_guid.Value);");
                methodSb.AppendLine($"                        }}");
            }
            else if (elementTypeName.EndsWith("DateOnly", StringComparison.OrdinalIgnoreCase))
            {
                methodSb.AppendLine($"                        if (element is Scalar<DateOnly> {propPrefix}_date)");
                methodSb.AppendLine($"                        {{");
                methodSb.AppendLine($"                            elementsList.Add({propPrefix}_date.Value);");
                methodSb.AppendLine($"                        }}");
            }
            
            if (isNullableElementType)
            {
                methodSb.AppendLine($"                        else if (element.ToString() == \"none\")");
                methodSb.AppendLine($"                        {{");
                methodSb.AppendLine($"                            elementsList.Add(default({elementTypeName}));");
                methodSb.AppendLine($"                        }}");
            }
        }
        
        methodSb.AppendLine($"                    }}");
        methodSb.AppendLine($"                }}");
        methodSb.AppendLine($"            }}");
        
        // Return the appropriate collection type
        if (isArray)
        {
            methodSb.AppendLine($"            return elementsList.ToArray();");
        }
        else // List or other collection types
        {
            methodSb.AppendLine($"            return elementsList;");
        }
        
        methodSb.AppendLine($"        }}");
        
        // Add the helper method to the list to be included in the class
        HelperMethods.Add(methodSb.ToString());
    }

    // A list to store helper methods that will be added to the generated class
    private static readonly List<string> HelperMethods = new();

    private static void GenerateObjectBinding(StringBuilder sb, string propertyName, string propertyKey, string varName, ITypeSymbol propertyType)
    {
        if (propertyType is not INamedTypeSymbol namedType)
            return;

        string typeName = namedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        bool hasBindMethod = HasBindMethod(propertyType);

        if (hasBindMethod)
        {
            sb.AppendLine($"                    {propertyName} = obj.TryGetSaveObject(\"{propertyKey}\", out var {varName}Obj) ? {typeName}.Bind({varName}Obj) : null,");
        }
        else
        {
            sb.AppendLine($"                    {propertyName} = obj.TryGetSaveObject(\"{propertyKey}\", out var {varName}Obj) ? new {typeName}");
            sb.AppendLine($"                    {{");

            foreach (var member in namedType.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.IsReadOnly || member.IsStatic)
                    continue;

                // Generate property bindings depending on property type
                if (IsSimpleType(member.Type.ToDisplayString()))
                {
                    string memberKey = ConvertToSnakeCase(member.Name);
                    string memberDefaultValue = GetDefaultValueLiteral(member.Type);
                    sb.AppendLine($"                        {member.Name} = {varName}Obj.GetValueOrDefault<{member.Type}>(\"{memberKey}\", {memberDefaultValue}),");
                }
                // Additional handling for arrays, dictionaries, and nested objects can be added here
            }

            sb.AppendLine($"                    }} : null,");
        }
    }

    private static void GenerateDictionaryBinding(StringBuilder sb, string propertyName, string propertyKey, string varName, ITypeSymbol propertyType)
    {
        // Check if this is a valid dictionary type
        if (propertyType is not INamedTypeSymbol namedType || !namedType.IsGenericType || namedType.TypeArguments.Length != 2)
        {
            sb.AppendLine($"                    {propertyName} = {GetDefaultValueForType(propertyType)}, // Not a valid dictionary type");
            return;
        }
        
        var keyType = namedType.TypeArguments[0];
        var valueType = namedType.TypeArguments[1];
        
        string keyTypeName = keyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string valueTypeName = valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        bool isValueComplexType = !IsSimpleType(valueTypeName);
        bool hasValueSaveModelAttribute = HasBindMethod(valueType);
        bool isValueNullable = valueType.NullableAnnotation == NullableAnnotation.Annotated || !valueType.IsValueType;

        // Generate key conversion logic
        string keyConverter;
        if (keyTypeName == "System.String" || keyTypeName == "string")
        {
            keyConverter = "kvp.Key";
        }
        else if (keyType.TypeKind == TypeKind.Enum)
        {
            keyConverter = $"({keyTypeName})System.Enum.Parse(typeof({keyTypeName}), kvp.Key)";
        }
        else 
        {
            keyConverter = $"System.Convert.ChangeType(kvp.Key, typeof({keyTypeName}))";
        }
        
        // Generate value binding code based on type
        string valueConverter;
        if (isValueComplexType && hasValueSaveModelAttribute)
        {
            valueConverter = $"kvp.Value is SaveObject valueObj ? {valueTypeName}.Bind(valueObj) : {GetDefaultValueForType(valueType)}";
        }
        else if (valueType.TypeKind == TypeKind.Enum)
        {
            valueConverter = $"kvp.Value is Scalar<int> i ? ({valueTypeName})i.Value : {GetDefaultValueForType(valueType)}";
        }
        else
        {
            // Handle primitive and simple types dynamically
            string scalarType = GetScalarTypeForElementType(valueTypeName);
            string defaultValue = GetDefaultValueForType(valueType);
            
            if (valueType.IsValueType && !isValueNullable)
            {
                valueConverter = $"kvp.Value is Scalar<{scalarType}> s ? s.Value : {defaultValue}";
            }
            else
            {
                valueConverter = $"kvp.Value switch {{ Scalar<{scalarType}> s => s.Value, SaveElement e when e.ToString() == \"none\" => {defaultValue}, _ => {defaultValue} }}";
            }
        }
        
        // Start the dictionary creation code
        sb.AppendLine($"                    {propertyName} = obj.TryGetSaveObject(\"{propertyKey}\", out var {varName}Dict) && {varName}Dict != null ?");
        sb.AppendLine($"                        {varName}Dict.Properties");
        sb.AppendLine($"                            .ToDictionary(");
        sb.AppendLine($"                                kvp => ({keyTypeName}){keyConverter},");
        sb.AppendLine($"                                kvp => {valueConverter}) :");
        sb.AppendLine($"                        new Dictionary<{keyTypeName}, {valueTypeName}>(),");
    }

    private static string GetDefaultValueForType(ITypeSymbol type)
    {
        // Always treat reference types as nullable
        if (type.IsReferenceType)
        {
            // For string, return null instead of empty string
            if (type.SpecialType == SpecialType.System_String)
                return "null";
                
            // Check if it's a collection type
            string typeName = type.ToDisplayString();
            if (typeName.Contains("[]"))
            {
                // For arrays, create an empty array
                var elementType = ((IArrayTypeSymbol)type).ElementType.ToDisplayString();
                return $"Array.Empty<{elementType}>()";
            }
            else if (typeName.Contains("List<"))
            {
                // For lists, create a new list
                if (type is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
                {
                    var elementType = namedType.TypeArguments[0].ToDisplayString();
                    return $"new List<{elementType}>()";
                }
            }
            else if (typeName.Contains("Dictionary<"))
            {
                // For dictionaries, create a new dictionary
                if (type is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 1)
                {
                    var keyType = namedType.TypeArguments[0].ToDisplayString();
                    var valueType = namedType.TypeArguments[1].ToDisplayString();
                    return $"new Dictionary<{keyType}, {valueType}>()";
                }
            }
            
            // For complex types, return null instead of new instance
            return "null";
        }
        else if (type.TypeKind == TypeKind.Enum)
        {
            // For enums, use 0 or the first value
            return $"({type.ToDisplayString()})0";
        }
        else
        {
            // Handle primitive types
            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return "false";
                case SpecialType.System_Char:
                    return "'\\0'";
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Decimal:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                    return "0";
                default:
                    // For nullable types, use null
                    if (type.NullableAnnotation == NullableAnnotation.Annotated || 
                        (type.OriginalDefinition != null && type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T))
                    {
                        return "null";
                    }
                    
                    // Special handling for common types
                    string typeName = type.ToDisplayString();
                    if (typeName.Contains("DateOnly"))
                    {
                        return "default(DateOnly)";
                    }
                    else if (typeName.Contains("Guid"))
                    {
                        return "Guid.Empty";
                    }
                    
                    return "default";
            }
        }
    }

    private static string GetDefaultValueLiteral(ITypeSymbol type)
    {
        // Always treat reference types as nullable
        if (type.IsReferenceType)
        {
            // For string, return null
            if (type.SpecialType == SpecialType.System_String)
                return "null";
                
            // Check if it's a collection type
            string typeName = type.ToDisplayString();
            if (typeName.Contains("[]"))
            {
                // For arrays, create an empty array
                var elementType = ((IArrayTypeSymbol)type).ElementType.ToDisplayString();
                return $"Array.Empty<{elementType}>()";
            }
            else if (typeName.Contains("List<"))
            {
                // For lists, create a new list
                if (type is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
                {
                    var elementType = namedType.TypeArguments[0].ToDisplayString();
                    return $"new List<{elementType}>()";
                }
            }
            else if (typeName.Contains("Dictionary<"))
            {
                // For dictionaries, create a new dictionary
                if (type is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 1)
                {
                    var keyType = namedType.TypeArguments[0].ToDisplayString();
                    var valueType = namedType.TypeArguments[1].ToDisplayString();
                    return $"new Dictionary<{keyType}, {valueType}>()";
                }
            }
            
            // For complex types, return null instead of new instance
            return "null";
        }
        else if (type.TypeKind == TypeKind.Enum)
        {
            // For enums, use 0 or the first value
            return $"({type.ToDisplayString()})0";
        }
        else
        {
            // Handle primitive types
            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return "false";
                case SpecialType.System_Char:
                    return "'\\0'";
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Decimal:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                    return "0";
                default:
                    // For nullable types, use null
                    if (type.NullableAnnotation == NullableAnnotation.Annotated || 
                        (type.OriginalDefinition != null && type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T))
                    {
                        return "null";
                    }
                    
                    // Special handling for common types
                    string typeName = type.ToDisplayString();
                    if (typeName.Contains("DateOnly"))
                    {
                        return "default(DateOnly)";
                    }
                    else if (typeName.Contains("Guid"))
                    {
                        return "Guid.Empty";
                    }
                    
                    return "default";
            }
        }
    }

    private static IEnumerable<(IPropertySymbol Property, AttributeData Attribute)> GetPropertiesWithAttributes(INamedTypeSymbol classSymbol)
    {
        // Track which properties we've processed to avoid duplicates
        var processedPropertyKeys = new HashSet<string>();
        
        foreach (var member in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            var attributes = member.GetAttributes();
            
            // Filter to just the Save* attributes
            var saveAttributes = attributes.Where(a => 
                a.AttributeClass?.ToDisplayString() == SaveScalarAttributeName ||
                a.AttributeClass?.ToDisplayString() == SaveArrayAttributeName ||
                a.AttributeClass?.ToDisplayString() == SaveObjectAttributeName ||
                a.AttributeClass?.ToDisplayString() == SaveIndexedDictionaryAttributeName).ToList();
                
            // Check if we have multiple Save* attributes on the same property
            if (saveAttributes.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Property {member.Name} on class {classSymbol.Name} has multiple Save* attributes. " +
                    $"Only one Save* attribute is allowed per property.");
            }
            
            // Check for duplicate properties
            var propertyKey = $"{classSymbol.ToDisplayString()}.{member.Name}";
            if (!processedPropertyKeys.Contains(propertyKey) && saveAttributes.Count > 0)
            {
                processedPropertyKeys.Add(propertyKey);
                yield return (member, saveAttributes[0]);
            }
        }
    }

    private static bool IsSimpleType(string typeName)
    {
        // Check if it's a simple type that doesn't need complex binding
        return SimpleTypes.Contains(typeName);
    }
    
    private static ITypeSymbol? GetNullableUnderlyingType(ITypeSymbol type)
    {
        // Check if this is a nullable value type (Nullable<T>)
        if (type is INamedTypeSymbol namedType && 
            namedType.OriginalDefinition?.SpecialType == SpecialType.System_Nullable_T)
        {
            return namedType.TypeArguments[0];
        }
        
        // Check if this is a nullable reference type (T?)
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return type.WithNullableAnnotation(NullableAnnotation.None);
        }
        
        return null;
    }
    
    private static string GetScalarTypeForElementType(string elementTypeName)
    {
        if (elementTypeName.EndsWith("System.String") || elementTypeName.EndsWith("string"))
            return "string";
        else if (elementTypeName.EndsWith("System.Int32") || elementTypeName.EndsWith("int"))
            return "int";
        else if (elementTypeName.EndsWith("System.Int64") || elementTypeName.EndsWith("long"))
            return "long";
        else if (elementTypeName.EndsWith("System.Single") || elementTypeName.EndsWith("float"))
            return "float";
        else if (elementTypeName.EndsWith("System.Boolean") || elementTypeName.EndsWith("bool"))
            return "bool";
        else if (elementTypeName.EndsWith("System.Guid") || elementTypeName.EndsWith("Guid"))
            return "Guid";
        else if (elementTypeName.EndsWith("System.DateOnly") || elementTypeName.EndsWith("DateOnly"))
            return "DateOnly";
        else
            return "object";
    }

    private static bool HasBindMethod(ITypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes().Any(attr =>
            attr.AttributeClass?.ToDisplayString() == SaveModelAttributeName ||
            attr.AttributeClass?.Name == "SaveModelAttribute" ||
            attr.AttributeClass?.Name == "SaveModel");
    }

    private static string ConvertToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder();
        bool wasUpper = false;
        bool wasDigit = false;

        foreach (char c in input)
        {
            if (char.IsUpper(c))
            {
                if (!wasUpper && !wasDigit && result.Length > 0)
                {
                    result.Append('_');
                }
                wasUpper = true;
            }
            else if (char.IsDigit(c))
            {
                wasDigit = true;
            }
            else
            {
                wasUpper = false;
                wasDigit = false;
            }
            result.Append(char.ToLower(c));
        }

        return result.ToString();
    }

    private static ITypeSymbol? GetElementType(ITypeSymbol propertyType)
    {
        if (propertyType is IArrayTypeSymbol arrayTypeSymbol)
        {
            return arrayTypeSymbol.ElementType;
        }
        else if (propertyType is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            return namedType.TypeArguments.FirstOrDefault();
        }
        return null;
    }
} 