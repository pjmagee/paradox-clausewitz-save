using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Collections.Generic;

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
 * 4. Dictionary<long, TValue> special handling:
 *    - SaveIndexedDictionary with long keys needs custom int.Parse/long.Parse logic
 */

[Generator]
public class BinderGenerator : IIncrementalGenerator
{
    private const string SaveScalarAttributeName = "MageeSoft.Paradox.Clausewitz.Save.Models.SaveScalarAttribute";
    private const string SaveArrayAttributeName = "MageeSoft.Paradox.Clausewitz.Save.Models.SaveArrayAttribute";
    private const string SaveObjectAttributeName = "MageeSoft.Paradox.Clausewitz.Save.Models.SaveObjectAttribute";
    private const string SaveIndexedDictionaryAttributeName = "MageeSoft.Paradox.Clausewitz.Save.Models.SaveIndexedDictionaryAttribute";

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
                GenerateBinderForClass(classSymbol, context);
                }
            }
        }
    }

    private static void GenerateBinderForClass(INamedTypeSymbol classSymbol, SourceProductionContext context)
    {
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var className = classSymbol.Name;
        var properties = GetPropertiesWithAttributes(classSymbol);
        var allProperties = classSymbol.GetMembers().OfType<IPropertySymbol>().ToList();
        var requiredProperties = allProperties.Where(p => p.IsRequired).ToList();

        var sourceText = $$"""
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
        public static {{className}} Bind(SaveObject obj)
        {
            try
            {
                return new {{className}}
                {
{{GenerateRequiredPropertyInitializers(requiredProperties)}}{{GeneratePropertyBindings(properties)}}
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error binding {nameof({{className}})}", ex);
            }
        }
    }
}
""";

        // Parse the source text into a syntax tree for formatting
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText);
        
        // Get the formatted syntax tree with proper whitespace normalization
        var formattedRoot = syntaxTree.GetRoot().NormalizeWhitespace();
        var formattedSourceText = formattedRoot.GetText(Encoding.UTF8);
        
        // Add the source with proper formatting
        context.AddSource($"{className}.g.cs", formattedSourceText);
    }

    private static string GenerateRequiredPropertyInitializers(List<IPropertySymbol> requiredProperties)
    {
        if (requiredProperties.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var property in requiredProperties)
        {
            sb.AppendLine($"                    {property.Name} = {GetDefaultValueForType(property.Type)},");
        }
        return sb.ToString();
    }

    private static string GetDefaultValueForType(ITypeSymbol type)
    {
        if (type.SpecialType == SpecialType.System_String)
            return "string.Empty";
        if (type.SpecialType == SpecialType.System_Boolean)
            return "false";
        if (type.SpecialType == SpecialType.System_Int32 ||
            type.SpecialType == SpecialType.System_Int64 ||
            type.SpecialType == SpecialType.System_Single ||
            type.SpecialType == SpecialType.System_Double)
            return "0";
        
        if (type.TypeKind == TypeKind.Enum)
            return "0";
        
        // For DateOnly
        if (type.Name == "DateOnly")
            return "new DateOnly(1, 1, 1)";
        
        // For Guid
        if (type.Name == "Guid")
            return "Guid.Empty";

        // For collections
        if (type.Name.Contains("List") || type.Name.Contains("Array") || type.Name.Contains("Dictionary"))
        {
            // Check if it's an array
            if (type is IArrayTypeSymbol)
                return "Array.Empty<" + ((IArrayTypeSymbol)type).ElementType + ">()";
            
            // Check if it's a generic collection
            if (type is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
            {
                if (type.Name.Contains("ImmutableList"))
                    return "ImmutableList<" + namedType.TypeArguments[0] + ">.Empty";
                if (type.Name.Contains("ImmutableArray"))
                    return "ImmutableArray<" + namedType.TypeArguments[0] + ">.Empty";
                if (type.Name.Contains("ImmutableDictionary"))
                    return "ImmutableDictionary<" + namedType.TypeArguments[0] + ", " + namedType.TypeArguments[1] + ">.Empty";
                if (type.Name.Contains("Dictionary"))
                    return "new Dictionary<" + namedType.TypeArguments[0] + ", " + namedType.TypeArguments[1] + ">()";
                if (type.Name.Contains("List"))
                    return "new List<" + namedType.TypeArguments[0] + ">()";
            }
        }

        // For all other reference types
        return "default!";
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

    private static string GeneratePropertyBindings(IEnumerable<(IPropertySymbol Property, AttributeData Attribute)> propertyAttributes)
    {
        var sb = new StringBuilder();
        // Track properties by containing type + property name to avoid duplicates
        var processedProperties = new HashSet<string>(); 
        
        foreach (var (property, attribute) in propertyAttributes)
        {
            var propertyName = property.Name;
            var containingType = property.ContainingType.ToDisplayString();
            var fullPropertyId = $"{containingType}.{propertyName}";
            
            // Skip if this property has already been processed
            if (processedProperties.Contains(fullPropertyId))
                continue;
                
            processedProperties.Add(fullPropertyId);
            
            // Get the property key name from the attribute's Name property or use the property name if null
            string? propertyKey = null;
            if (attribute.NamedArguments.Any(na => na.Key == "Name"))
            {
                propertyKey = attribute.NamedArguments.First(na => na.Key == "Name").Value.Value?.ToString();
            }
            else if (attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0].Value != null)
            {
                propertyKey = attribute.ConstructorArguments[0].Value?.ToString();
            }

            // Fall back to property name if no custom name is specified
            propertyKey ??= property.Name;

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

            // Special case for Achievements.Values
            if (propertyName == "Values" && propertyType.Contains("ImmutableList<int>"))
            {
                sb.AppendLine($"                    {propertyName} = obj.TryGetSaveArray(\"{propertyKey}\", out var {varName}Array) ? " +
                              $"System.Collections.Immutable.ImmutableList<int>.Empty.AddRange({varName}Array.Elements().Select(x => ((Scalar<int>)x).Value)) : " +
                              $"System.Collections.Immutable.ImmutableList<int>.Empty,");
                continue;
            }

            switch (attributeType)
            {
                case SaveScalarAttributeName:
                    var underlyingType = GetUnderlyingType(propertyType);
                    
                    if (underlyingType == "bool" || underlyingType == "System.Boolean")
                    {
                        sb.AppendLine($"                    {propertyName} = obj.TryGetBool(\"{propertyKey}\", out var {varName}) ? {varName} : default,");
                    }
                    else if (underlyingType == "int" || underlyingType == "System.Int32")
                    {
                        sb.AppendLine($"                    {propertyName} = obj.TryGetInt(\"{propertyKey}\", out var {varName}) ? {varName} : default,");
                    }
                    else if (underlyingType == "long" || underlyingType == "System.Int64")
                    {
                        sb.AppendLine($"                    {propertyName} = obj.TryGetLong(\"{propertyKey}\", out var {varName}) ? {varName} : default,");
                    }
                    else if (underlyingType == "float" || underlyingType == "System.Single")
                    {
                        sb.AppendLine($"                    {propertyName} = obj.TryGetFloat(\"{propertyKey}\", out var {varName}) ? {varName} : default,");
                    }
                    else if (underlyingType == "string" || underlyingType == "System.String")
                    {
                        sb.AppendLine($"                    {propertyName} = obj.TryGetString(\"{propertyKey}\", out var {varName}) ? {varName} : default,");
                    }
                    else if (underlyingType == "Guid" || underlyingType == "System.Guid")
                    {
                        sb.AppendLine($"                    {propertyName} = obj.TryGetGuid(\"{propertyKey}\", out var {varName}) ? {varName} : default,");
                    }
                    else if (underlyingType == "DateOnly" || underlyingType == "System.DateOnly")
                    {
                        sb.AppendLine($"                    {propertyName} = obj.TryGetDateOnly(\"{propertyKey}\", out var {varName}) ? {varName} : default,");
                    }
                    else
                    {
                        // For other types, use a generic approach
                        sb.AppendLine($"                    {propertyName} = obj.Properties.FirstOrDefault(p => p.Key == \"{propertyKey}\").Value is Scalar<{underlyingType}> {varName} ? {varName}.Value : default,");
                    }
                    break;

                case SaveArrayAttributeName:
                    var elementType = GetElementType(propertyType);
                    
                    // Check if the element type is a complex type that needs binding
                    bool isElementComplexType = !(elementType.StartsWith("System.") || elementType == "string" || elementType == "int" || 
                                               elementType == "float" || elementType == "double" || elementType == "bool" || elementType == "long");
                    
                    // For arrays of complex objects or scalar values
                    string elementSelector;
                    if (isElementComplexType)
                    {
                        elementSelector = $"(x is SaveObject saveObj ? {elementType}.Bind(saveObj) : new {elementType}())";
                    }
                    else
                    {
                        elementSelector = $"((Scalar<{elementType}>)x).Value";
                    }
                    
                    // Handle different collection types
                    if (propertyType.Contains("ImmutableList"))
                    {
                        sb.AppendLine($"                    {propertyName} = obj.TryGetSaveArray(\"{propertyKey}\", out var {varName}Array) ? " +
                                     $"System.Collections.Immutable.ImmutableList<{elementType}>.Empty.AddRange({varName}Array.Elements().Select(x => {elementSelector})) : " +
                                     $"System.Collections.Immutable.ImmutableList<{elementType}>.Empty,");
                    }
                    else if (propertyType.Contains("ImmutableArray"))
                    {
                        sb.AppendLine($"                    {propertyName} = obj.TryGetSaveArray(\"{propertyKey}\", out var {varName}Array) ? " +
                                     $"System.Collections.Immutable.ImmutableArray.CreateRange({varName}Array.Elements().Select(x => {elementSelector})) : " +
                                     $"System.Collections.Immutable.ImmutableArray<{elementType}>.Empty,");
                    }
                    else if (propertyType.Contains("List<"))
                    {
                        sb.AppendLine($"                    {propertyName} = obj.TryGetSaveArray(\"{propertyKey}\", out var {varName}Array) ? " +
                                     $"new List<{elementType}>({varName}Array.Elements().Select(x => {elementSelector})) : " +
                                     $"new List<{elementType}>(),");
                    }
                    else // Default to array
                    {
                        sb.AppendLine($"                    {propertyName} = obj.TryGetSaveArray(\"{propertyKey}\", out var {varName}Array) ? " +
                                     $"{varName}Array.Elements().Select(x => {elementSelector}).ToArray() : " +
                                     $"Array.Empty<{elementType}>(),");
                    }
                    break;

                case SaveObjectAttributeName:
                    var objectType = GetUnderlyingType(propertyType);
                    
                    // Only try to use Bind if the property is a type that might have a Bind method
                    // For now, let's check if it's a simple system type - if not, assume a custom type that might need binding
                    if (objectType.StartsWith("System.") || objectType == "string" || objectType == "int" || objectType == "float" || 
                        objectType == "double" || objectType == "bool" || objectType == "long")
                    {
                        sb.AppendLine($"                    {propertyName} = obj.TryGetSaveObject(\"{propertyKey}\", out var {varName}Obj) ? ({objectType})Convert.ChangeType({varName}Obj.ToString(), typeof({objectType})) : default,");
                    }
                    else 
                    {
                        // For custom types, use the generated Bind method
                        sb.AppendLine($"                    {propertyName} = obj.TryGetSaveObject(\"{propertyKey}\", out var {varName}Obj) ? " +
                                     $"{objectType}.Bind({varName}Obj) : new {objectType}(),");
                    }
                    break;

                case SaveIndexedDictionaryAttributeName:
                    var keyValueTypes = GetDictionaryTypes(propertyType);
                    
                    // Check if the value type is a complex type that needs binding
                    var valueType = keyValueTypes.Value;
                    bool isValueComplexType = !(valueType.StartsWith("System.") || valueType == "string" || valueType == "int" || 
                                              valueType == "float" || valueType == "double" || valueType == "bool" || valueType == "long");
                    
                    if (isValueComplexType)
                    {
                        // For dictionaries with complex object values that need cascading binding
                        sb.AppendLine($"                    {propertyName} = obj.TryGetSaveObject(\"{propertyKey}\", out var {varName}Dict) ? " +
                                    $"{varName}Dict.Properties.ToDictionary(" +
                                    $"kvp => {keyValueTypes.Key}.Parse(kvp.Key), " +
                                    $"kvp => kvp.Value is SaveObject saveObj ? {valueType}.Bind(saveObj) : new {valueType}()) : " +
                                    $"new Dictionary<{keyValueTypes.Key}, {valueType}>(),");
                    }
                    else
                    {
                        // For dictionaries with simple scalar values
                        sb.AppendLine($"                    {propertyName} = obj.TryGetSaveObject(\"{propertyKey}\", out var {varName}Dict) ? " +
                                    $"{varName}Dict.Properties.ToDictionary(" +
                                    $"kvp => {keyValueTypes.Key}.Parse(kvp.Key), " +
                                    $"kvp => kvp.Value is Scalar<{valueType}> scalar ? scalar.Value : default!) : " +
                                    $"new Dictionary<{keyValueTypes.Key}, {valueType}>(),");
                    }
                    break;
            }
        }
        return sb.ToString();
    }

    private static string GetUnderlyingType(string propertyType)
    {
        if (propertyType.StartsWith("System.Nullable<"))
        {
            return propertyType.Substring(15, propertyType.Length - 16);
        }
        return propertyType;
    }

    private static string GetElementType(string propertyType)
    {
        if (propertyType.StartsWith("System.Collections.Generic.IEnumerable<"))
        {
            return propertyType.Substring(40, propertyType.Length - 41);
        }
        if (propertyType.StartsWith("System.Collections.Generic.List<"))
        {
            return propertyType.Substring(33, propertyType.Length - 34);
        }
        if (propertyType.EndsWith("[]"))
        {
            return propertyType.Substring(0, propertyType.Length - 2);
        }
        return propertyType;
    }

    private static (string Key, string Value) GetDictionaryTypes(string propertyType)
    {
        if (propertyType.StartsWith("System.Collections.Generic.Dictionary<"))
        {
            var types = propertyType.Substring(37, propertyType.Length - 38).Split(',');
            return (types[0].Trim(), types[1].Trim());
        }
        if (propertyType.StartsWith("System.Collections.Immutable.ImmutableDictionary<"))
        {
            var types = propertyType.Substring(48, propertyType.Length - 49).Split(',');
            return (types[0].Trim(), types[1].Trim());
        }
        throw new ArgumentException($"Unsupported dictionary type: {propertyType}");
    }
} 