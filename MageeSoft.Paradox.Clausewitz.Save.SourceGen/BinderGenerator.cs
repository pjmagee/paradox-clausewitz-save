using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MageeSoft.Paradox.Clausewitz.Save.SourceGen;

[Generator]
public class BinderGenerator : IIncrementalGenerator
{
    // Attribute fully qualified names we care about
    const string NameSpace = "MageeSoft.Paradox.Clausewitz.Save.Models.Attributes";
    const string SaveScalarAttributeName = $"{NameSpace}.SaveScalarAttribute";
    const string SaveArrayAttributeName = $"{NameSpace}.SaveArrayAttribute";
    const string SaveObjectAttributeName = $"{NameSpace}.SaveObjectAttribute";
    const string SaveIndexedDictionaryAttributeName = $"{NameSpace}.SaveIndexedDictionaryAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register for syntax notifications 
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsPotentiallyRelevantClass(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider
            .Combine(classDeclarations.Collect());

        // Generate the source
        context.RegisterSourceOutput(compilationAndClasses, 
            static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    static bool IsPotentiallyRelevantClass(SyntaxNode node)
    {
        // We're looking for classes with properties that might have our attributes
        return node is ClassDeclarationSyntax classDecl && 
               classDecl.Members.OfType<PropertyDeclarationSyntax>().Any(p => 
                   p.AttributeLists.Count > 0);
    }

    static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // Get the class declaration
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        
        // Check if any property has our attributes
        foreach (var property in classDeclaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            foreach (var attributeList in property.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name.ToString();
                    
                    if (attributeName.Contains("SaveScalar") || 
                        attributeName.Contains("SaveArray") || 
                        attributeName.Contains("SaveObject") || 
                        attributeName.Contains("SaveIndexedDictionary"))
                    {
                        // This is a potentially relevant class
                        return classDeclaration;
                    }
                }
            }
        }

        return null;
    }

    static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        // Generate common binding utility methods
        GenerateBinderUtilities(context);

        // Generate a static binder for each class
        foreach (var classDeclaration in classes.Where(c => c != null))
        {
            var semanticModel = compilation.GetSemanticModel(classDeclaration!.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(classDeclaration!) is INamedTypeSymbol classSymbol)
            {
                GenerateBinderForClass(classSymbol, context);
            }
        }
    }

    static void GenerateBinderUtilities(SourceProductionContext context)
    {
        var utilityCode = @"#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Parser;

namespace MageeSoft.Paradox.Clausewitz.Save.Models
{
    /// <summary>
    /// Contains utility methods used by the generated binders.
    /// </summary>
    public static class BinderUtilities
    {
        public static object? BindScalar(SaveElement element, Type targetType)
        {
            // For nullable types, get the underlying type
            Type nullableUnderlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (element is Scalar<string> stringScalar)
            {
                if (stringScalar.Value == ""none"")
                    return null;
                if (nullableUnderlyingType == typeof(string))
                    return stringScalar.Value;
            }
            else if (element is Scalar<int> intScalar)
            {
                if (nullableUnderlyingType == typeof(int))
                    return intScalar.Value;
                if (nullableUnderlyingType == typeof(float))
                    return (float)intScalar.Value;
                if (nullableUnderlyingType == typeof(long))
                    return (long)intScalar.Value;
            }
            else if (element is Scalar<long> longScalar)
            {
                if (nullableUnderlyingType == typeof(long))
                    return longScalar.Value;
                if (nullableUnderlyingType == typeof(int) && longScalar.Value <= int.MaxValue)
                    return (int)longScalar.Value;
                if (nullableUnderlyingType == typeof(float))
                    return (float)longScalar.Value;
            }
            else if (element is Scalar<float> floatScalar)
            {
                if (nullableUnderlyingType == typeof(float))
                    return floatScalar.Value;
                if (nullableUnderlyingType == typeof(int) && floatScalar.Value % 1 == 0)
                    return (int)floatScalar.Value;
                if (nullableUnderlyingType == typeof(long) && floatScalar.Value % 1 == 0)
                    return (long)floatScalar.Value;
            }
            else if (element is Scalar<bool> boolScalar)
            {
                if (nullableUnderlyingType == typeof(bool))
                    return boolScalar.Value;
            }
            else if (element is Scalar<DateOnly> dateScalar)
            {
                if (nullableUnderlyingType == typeof(DateOnly))
                    return dateScalar.Value;
            }
            else if (element is Scalar<Guid> guidScalar)
            {
                if (nullableUnderlyingType == typeof(Guid))
                    return guidScalar.Value;
            }

            return null;
        }

        public static T? BindInt<T>(SaveElement element) where T : struct
        {
            if (element is Scalar<int> intScalar)
            {
                if (typeof(T) == typeof(int))
                    return (T)(object)intScalar.Value;
                if (typeof(T) == typeof(float))
                    return (T)(object)(float)intScalar.Value;
                if (typeof(T) == typeof(long))
                    return (T)(object)(long)intScalar.Value;
            }
            return null;
        }

        public static T? BindLong<T>(SaveElement element) where T : struct
        {
            if (element is Scalar<long> longScalar)
            {
                if (typeof(T) == typeof(long))
                    return (T)(object)longScalar.Value;
                if (typeof(T) == typeof(int) && longScalar.Value <= int.MaxValue)
                    return (T)(object)(int)longScalar.Value;
                if (typeof(T) == typeof(float))
                    return (T)(object)(float)longScalar.Value;
            }
            return null;
        }

        public static T? BindFloat<T>(SaveElement element) where T : struct
        {
            if (element is Scalar<float> floatScalar)
            {
                if (typeof(T) == typeof(float))
                    return (T)(object)floatScalar.Value;
                if (typeof(T) == typeof(int) && floatScalar.Value % 1 == 0)
                    return (T)(object)(int)floatScalar.Value;
                if (typeof(T) == typeof(long) && floatScalar.Value % 1 == 0)
                    return (T)(object)(long)floatScalar.Value;
            }
            return null;
        }

        public static SaveObject? GetSaveObject(SaveObject saveObject, string propertyName)
        {
            var prop = saveObject.Properties.FirstOrDefault(p => p.Key == propertyName);
            
            // Handle 'none' value
            if (prop.Value != null && prop.Value.ToString() == ""none"")
                return null;
            
            return prop.Value as SaveObject;
        }

        public static bool IsNullable(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }
    }
}";

        context.AddSource("BinderUtilities.g.cs", SourceText.From(utilityCode, Encoding.UTF8));
    }

    static void GenerateBinderForClass(INamedTypeSymbol classSymbol, SourceProductionContext context)
    {
        string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        string className = classSymbol.Name;
        string binderClassName = $"{className}Binder";

        var properties = GetPropertiesWithAttributes(classSymbol);
        
        if (!properties.Any())
        {
            return; // No relevant properties to generate binding for
        }

        var source = new StringBuilder();
        source.AppendLine($@"#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Parser;
using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace {namespaceName}.Generated
{{
    /// <summary>
    /// Generated binder for {className}
    /// </summary>
    public static class {binderClassName}
    {{
        /// <summary>
        /// Binds a SaveObject to a {className} instance.
        /// </summary>
        /// <param name=""saveObject"">The SaveObject to bind from.</param>
        /// <returns>A new {className} instance with bound properties.</returns>
        public static {className} Bind(SaveObject saveObject)
        {{
            var result = new {className}();
");

        // Generate binding code for each property
        foreach (var property in properties)
        {
            var propertyName = property.Name;
            var propertyType = property.Type.ToDisplayString();
            var attributeData = GetSaveAttribute(property);
            
            if (attributeData == null)
                continue;

            var attributeName = attributeData.AttributeClass!.Name;
            var saveName = GetAttributeNameValue(attributeData);

            // Handle different attribute types
            if (attributeName == "SaveScalarAttribute")
            {
                GenerateScalarBinding(source, property, saveName);
            }
            else if (attributeName == "SaveArrayAttribute")
            {
                GenerateArrayBinding(source, property, saveName);
            }
            else if (attributeName == "SaveObjectAttribute")
            {
                GenerateObjectBinding(source, property, saveName);
            }
            else if (attributeName == "SaveIndexedDictionaryAttribute")
            {
                GenerateDictionaryBinding(source, property, saveName);
            }
        }

        source.AppendLine(@"
            return result;
        }");

        // Add any helper methods for binding specific types for this class
        GenerateHelperMethods(source, properties);

        source.AppendLine(@"    }
}");

        context.AddSource($"{binderClassName}.g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
    }

    static IEnumerable<IPropertySymbol> GetPropertiesWithAttributes(INamedTypeSymbol classSymbol)
    {
        return classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.GetAttributes().Any(a => 
                a.AttributeClass?.ToDisplayString() == SaveScalarAttributeName ||
                a.AttributeClass?.ToDisplayString() == SaveArrayAttributeName ||
                a.AttributeClass?.ToDisplayString() == SaveObjectAttributeName ||
                a.AttributeClass?.ToDisplayString() == SaveIndexedDictionaryAttributeName));
    }

    static AttributeData? GetSaveAttribute(IPropertySymbol property)
    {
        return property.GetAttributes().FirstOrDefault(a => 
            a.AttributeClass?.ToDisplayString() == SaveScalarAttributeName ||
            a.AttributeClass?.ToDisplayString() == SaveArrayAttributeName ||
            a.AttributeClass?.ToDisplayString() == SaveObjectAttributeName ||
            a.AttributeClass?.ToDisplayString() == SaveIndexedDictionaryAttributeName);
    }

    static string GetAttributeNameValue(AttributeData attributeData)
    {
        if (attributeData.ConstructorArguments.Length > 0)
        {
            return attributeData.ConstructorArguments[0].Value?.ToString() ?? "";
        }
        return "";
    }

    static void GenerateScalarBinding(StringBuilder source, IPropertySymbol property, string saveName)
    {
        var propertyName = property.Name;
        var propertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        
        source.AppendLine($@"            try
            {{
                var scalar = saveObject.Properties.FirstOrDefault(p => p.Key == ""{saveName}"").Value;
                if (scalar != null)
                {{
                    var value = BinderUtilities.BindScalar(scalar, typeof({propertyType}));
                    if (value != null)
                    {{
                        result.{propertyName} = ({propertyType})value;
                    }}
                }}
            }}
            catch (Exception ex)
            {{
                Console.WriteLine($""Error binding {propertyName}: {{ex.Message}}"");
            }}");
    }

    static void GenerateArrayBinding(StringBuilder source, IPropertySymbol property, string saveName)
    {
        var propertyName = property.Name;
        var propertyType = property.Type;
        
        if (propertyType is IArrayTypeSymbol arrayType)
        {
            var elementType = arrayType.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            GenerateArrayBindingCode(source, propertyName, saveName, elementType, "Array");
        }
        else if (propertyType is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var genericTypeName = namedType.ConstructedFrom.ToDisplayString();
            var elementType = namedType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            
            if (genericTypeName.Contains("ImmutableArray"))
            {
                GenerateArrayBindingCode(source, propertyName, saveName, elementType, "ImmutableArray");
            }
            else if (genericTypeName.Contains("ImmutableList"))
            {
                GenerateArrayBindingCode(source, propertyName, saveName, elementType, "ImmutableList");
            }
            else if (genericTypeName.Contains("List"))
            {
                GenerateArrayBindingCode(source, propertyName, saveName, elementType, "List");
            }
        }
    }

    static void GenerateArrayBindingCode(StringBuilder source, string propertyName, string saveName, 
                                             string elementType, string collectionType)
    {
        // Simplified array binding that calls into a separate method to handle the details
        source.AppendLine($@"            try
            {{
                result.{propertyName} = Bind{collectionType}OfType<{elementType}>(saveObject, ""{saveName}"");
            }}
            catch (Exception ex)
            {{
                Console.WriteLine($""Error binding {propertyName}: {{ex.Message}}"");
            }}");
    }

    static void GenerateObjectBinding(StringBuilder source, IPropertySymbol property, string saveName)
    {
        var propertyName = property.Name;
        var propertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var isNullable = property.NullableAnnotation == NullableAnnotation.Annotated;
        
        source.AppendLine($@"            try
            {{
                var obj = BinderUtilities.GetSaveObject(saveObject, ""{saveName}"");
                if (obj != null)
                {{
                    // Need to handle binding to the specific type
                    result.{propertyName} = {GetBindMethodForType(propertyType)};
                }}");
        
        if (isNullable)
        {
            source.AppendLine($@"                else
                {{
                    result.{propertyName} = null;
                }}");
        }
        
        source.AppendLine($@"            }}
            catch (Exception ex)
            {{
                Console.WriteLine($""Error binding {propertyName}: {{ex.Message}}"");
            }}");
    }

    static void GenerateDictionaryBinding(StringBuilder source, IPropertySymbol property, string saveName)
    {
        var propertyName = property.Name;
        var propertyType = property.Type as INamedTypeSymbol;
        
        if (propertyType != null && propertyType.IsGenericType && propertyType.TypeArguments.Length == 2)
        {
            var keyType = propertyType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var valueType = propertyType.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            
            source.AppendLine($@"            try
            {{
                var obj = BinderUtilities.GetSaveObject(saveObject, ""{saveName}"");
                if (obj != null)
                {{
                    result.{propertyName} = BindDictionary<{keyType}, {valueType}>(obj);
                }}
            }}
            catch (Exception ex)
            {{
                Console.WriteLine($""Error binding {propertyName}: {{ex.Message}}"");
            }}");
        }
    }

    static string GetBindMethodForType(string typeName)
    {
        // This is a simplification - ideally we'd check if the type has a generated binder
        // For now, just assume it does
        var simpleTypeName = typeName.Split('.').Last().TrimEnd('?');
        return $"{simpleTypeName}Binder.Bind(obj)";
    }

    static void GenerateHelperMethods(StringBuilder source, IEnumerable<IPropertySymbol> properties)
    {
        // Add standard helper methods for handling common types
        source.AppendLine(@"
        // Helper methods for array binding
        private static T[] BindArrayOfType<T>(SaveObject saveObject, string propertyName)
        {
            var list = new List<T>();
            bool isNullableElementType = BinderUtilities.IsNullable(typeof(T));
            
            foreach (var prop in saveObject.Properties.Where(p => p.Key == propertyName))
            {
                if (prop.Value is SaveArray array)
                {
                    foreach (var element in array.Items)
                    {
                        if (element is SaveObject obj)
                        {
                            // Need specific binding based on type T
                            var item = BindObjectElement<T>(obj);
                            if (item != null)
                                list.Add((T)item);
                            else if (isNullableElementType)
                                list.Add(default!);
                        }
                        else if (element is SaveElement scalar)
                        {
                            if (scalar.ToString() == ""none"" && isNullableElementType)
                            {
                                list.Add(default!);
                            }
                            else if (scalar.ToString() != ""none"")
                            {
                                var value = BinderUtilities.BindScalar(scalar, typeof(T));
                                if (value != null)
                                    list.Add((T)value);
                                else if (isNullableElementType)
                                    list.Add(default!);
                            }
                        }
                    }
                }
                else if (prop.Value is SaveObject obj)
                {
                    var item = BindObjectElement<T>(obj);
                    if (item != null)
                        list.Add((T)item);
                    else if (isNullableElementType)
                        list.Add(default!);
                }
                else if (prop.Value is SaveElement scalar && prop.Value.ToString() != ""none"")
                {
                    var value = BinderUtilities.BindScalar(scalar, typeof(T));
                    if (value != null)
                        list.Add((T)value);
                    else if (isNullableElementType)
                        list.Add(default!);
                }
            }
            
            return list.ToArray();
        }

        private static List<T> BindListOfType<T>(SaveObject saveObject, string propertyName)
        {
            return new List<T>(BindArrayOfType<T>(saveObject, propertyName));
        }

        private static ImmutableArray<T> BindImmutableArrayOfType<T>(SaveObject saveObject, string propertyName)
        {
            return ImmutableArray.CreateRange(BindArrayOfType<T>(saveObject, propertyName));
        }

        private static ImmutableList<T> BindImmutableListOfType<T>(SaveObject saveObject, string propertyName)
        {
            // Fix to ensure nullability is preserved correctly
            return ImmutableList.CreateRange<T>(BindArrayOfType<T>(saveObject, propertyName));
        }

        private static object? BindObjectElement<T>(SaveObject obj)
        {
            // This is a simplification - ideally, we would check if T has a generated binder
            // and use it if available, but for now we'll just return null
            return null;
        }

        private static ImmutableDictionary<TKey, TValue> BindDictionary<TKey, TValue>(SaveObject saveObject)
            where TKey : notnull
        {
            var dictionary = new Dictionary<TKey, TValue>();
            bool isNullableValueType = BinderUtilities.IsNullable(typeof(TValue));

            foreach (var prop in saveObject.Properties)
            {
                TKey? key = default;
                try
                {
                    if (typeof(TKey) == typeof(string))
                        key = (TKey)(object)prop.Key;
                    else if (typeof(TKey) == typeof(int) && int.TryParse(prop.Key, out var intKey))
                        key = (TKey)(object)intKey;
                    else
                        continue; // Skip if key cannot be converted
                }
                catch
                {
                    continue; // Skip if key cannot be converted
                }

                if (key == null)
                    continue;

                if (prop.Value is SaveObject obj)
                {
                    var value = BindObjectElement<TValue>(obj);
                    if (value != null)
                        dictionary.Add(key, (TValue)value);
                }
                else if (prop.Value is SaveElement scalar && scalar.ToString() != ""none"")
                {
                    var value = BinderUtilities.BindScalar(scalar, typeof(TValue));
                    if (value != null)
                        dictionary.Add(key, (TValue)value);
                }
            }

            return dictionary.ToImmutableDictionary();
        }");
    }
} 