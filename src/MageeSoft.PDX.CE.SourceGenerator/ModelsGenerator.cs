using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Text.RegularExpressions;

namespace MageeSoft.PDX.CE.SourceGenerator;

/// <summary>
/// Helper class for generating models from PDX save files
/// </summary>
public static class ModelsGenerator
{
    private const string Namespace = "MageeSoft.PDX.CE.Models";

    // Simple types frequently referenced - we use OrdinalIgnoreCase for comparisons
    private readonly static HashSet<string> SimpleTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "string", "int", "long", "float", "double", "bool", "DateTime", "Guid",
        "System.String", "System.Int32", "System.Int64", "System.Single",
        "System.Double", "System.Boolean", "System.DateTime", "System.Guid"
    };

    public static void GenerateModels(SourceProductionContext context, AdditionalText file, IEnumerable<SaveObjectAnalysis> analyses)
    {
        try
        {
            // Add explicit diagnostic to log which file is being processed
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("PDXSG100", "Processing File", $"Processing file: {file.Path}", "StellarisModelsGenerator", DiagnosticSeverity.Info, true),
                Location.None));

            var analysisList = analyses?.ToList() ?? new List<SaveObjectAnalysis>();

            // Report number of analyses found
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("PDXSG101", "Analysis Count", $"Found {analysisList.Count} analyses for file {file.Path}", "StellarisModelsGenerator", DiagnosticSeverity.Info, true),
                Location.None));

            // Report analyzer diagnostics first
            foreach (var analysis in analysisList)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("PDXSG102", "Analysis Root Name", $"Analysis root name: {analysis.RootName}", "StellarisModelsGenerator", DiagnosticSeverity.Info, true),
                    Location.None));

                foreach (var diagInfo in analysis.Diagnostics)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(diagInfo.Id, diagInfo.Title, diagInfo.Message, "StellarisAnalyzer", diagInfo.Severity, true),
                        Location.None));
                }

                // Report explicit errors caught by the analyzer
                 if (analysis.Error != null && !analysis.Diagnostics.Any(d => d.Id == "PDXSA003"))
                 {
                      context.ReportDiagnostic(Diagnostic.Create(
                         new DiagnosticDescriptor("PDXSG005", "Analyzer Phase Error", $"Analyzer failed for {analysis.RootName}: {analysis.Error.GetType().Name} - {analysis.Error.Message}", "StellarisModelsGenerator", DiagnosticSeverity.Error, true),
                         Location.None));
                 }
            }

            if (!analysisList.Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("PDXSG004", "Analyzer Input Empty", $"No analysis results received for {Path.GetFileName(file.Path)}. Skipping generation.", "StellarisModelsGenerator", DiagnosticSeverity.Warning, true),
                    Location.None));
                return;
            }

            foreach (var analysis in analysisList.Where(a => a.Error == null))
            {
                 // Determine namespace for this analysis root
                 string rootNamespaceSegment = analysis.RootName;
                 string fullNamespace = Namespace;

                 // Check for no definitions
                if (analysis.ClassDefinitions == null || analysis.ClassDefinitions.Count == 0)
                {
                     context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("PDXSG006", "No Class Definitions", $"Analysis for {analysis.RootName} completed successfully but found 0 class definitions. No models generated for this file.", "StellarisModelsGenerator", DiagnosticSeverity.Warning, true),
                        Location.None));
                     continue; // Skip if no classes were defined
                }

                 context.ReportDiagnostic(Diagnostic.Create(
                     new DiagnosticDescriptor("PDXSG007", "Generating Root Info", $"Analysis for {analysis.RootName} found {analysis.ClassDefinitions.Count} top-level class definitions. Starting generation in namespace {fullNamespace}...", "StellarisModelsGenerator", DiagnosticSeverity.Info, true),
                     Location.None));

                 // Iterate through the TOP-LEVEL classes defined for this analysis root
                foreach (var topLevelClassDef in analysis.ClassDefinitions.Values)
                {
                     // Check for null/empty class name before proceeding
                     if (string.IsNullOrEmpty(topLevelClassDef?.Name))
                     {
                         context.ReportDiagnostic(Diagnostic.Create(
                            new DiagnosticDescriptor("PDXSG009", "Invalid Class Definition", $"Skipping generation for a class definition with null or empty name from analysis of {analysis.RootName}.", "StellarisModelsGenerator", DiagnosticSeverity.Warning, true),
                            Location.None));
                         continue;
                     }

                     // Skip primitive types and C# reserved names
                     if (IsReservedTypeName(topLevelClassDef.Name))
                     {
                         context.ReportDiagnostic(Diagnostic.Create(
                             new DiagnosticDescriptor("PDXSG010", "Skipping Reserved Type", $"Skipping generation for reserved type name {topLevelClassDef.Name}.", "StellarisModelsGenerator", DiagnosticSeverity.Warning, true),
                             Location.None));
                         continue;
                     }

                     // Prepare StringBuilder for the entire file content
                     var fileContentBuilder = new StringBuilder();

                     // Add standard usings
                     fileContentBuilder.AppendLine("// <auto-generated/>");
                     fileContentBuilder.AppendLine("#nullable enable");
                     fileContentBuilder.AppendLine("using System;");
                     fileContentBuilder.AppendLine("using System.Collections.Generic;");
                     fileContentBuilder.AppendLine("using System.Linq;");
                     fileContentBuilder.AppendLine("using System.Globalization;");
                     fileContentBuilder.AppendLine("using MageeSoft.PDX.CE;");
                     fileContentBuilder.AppendLine();

                     // Add the specific namespace block
                     fileContentBuilder.AppendLine($"namespace {fullNamespace}");
                     fileContentBuilder.AppendLine("{");

                     // Start recursive generation for the top-level class and its nested classes
                     GenerateClassSourceRecursive(topLevelClassDef, fileContentBuilder, 1); // Start with indent level 1

                     // Close namespace
                     fileContentBuilder.AppendLine("}");
                     fileContentBuilder.AppendLine(); // Trailing newline

                     // Add the source file for this top-level class (containing all its nested classes)
                     context.AddSource($"{topLevelClassDef.Name}.g.cs", SourceText.From(fileContentBuilder.ToString(), Encoding.UTF8));

                    context.ReportDiagnostic(Diagnostic.Create(
                       new DiagnosticDescriptor("PDXSG008", "Generated Top-Level Class", $"Generated model file {topLevelClassDef.Name}.g.cs for top-level class {topLevelClassDef.Name}.", "StellarisModelsGenerator", DiagnosticSeverity.Info, true),
                       Location.None));
                }
            }
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("PDXSG001", "Generator Exception", $"Generator failed unexpectedly: {ex.GetType().Name} - {ex.Message} {ex.StackTrace}", "StellarisModelsGenerator", DiagnosticSeverity.Error, true),
                Location.None));
        }
    }

    /// <summary>
    /// Recursively generates the C# source code for a class definition, including its properties,
    /// binder method, and any nested classes. Appends to the provided StringBuilder.
    /// </summary>
    private static void GenerateClassSourceRecursive(ClassDefinition classDef, StringBuilder sb, int indentLevel)
    {
        if (string.IsNullOrEmpty(classDef?.Name)) return;

        string indent = new string(' ', indentLevel * 4);
        // Use FullName for the class declaration
        string className = classDef.FullName;

        // Class Summary and Declaration
        sb.AppendLine($"{indent}/// <summary>");
        sb.AppendLine($"{indent}/// Auto-generated model class. Represents the structure found at this level.");
        sb.AppendLine($"{indent}/// </summary>");
        sb.AppendLine($"{indent}public partial class {className}");
        sb.AppendLine($"{indent}{{");

        // Generate Properties
        string propertyIndent = indent + "    ";
        foreach (var property in classDef.Properties)
        {
            var propertyName = property.Name.TrimStart('@');
            
            // Get the type syntax suitable for declaration
            var nullableDeclarationType = AddNullableAnnotation(property.PropertyType);

            // Generate property
            sb.AppendLine($"{propertyIndent}public {nullableDeclarationType} {propertyName} {{ get; set; }}");
            sb.AppendLine(); // Blank line after property
        }

         // Generate Binder Method
         string binderIndent = propertyIndent; // Same indent as properties
         sb.AppendLine(); // Blank line before binder
         sb.AppendLine($"{binderIndent}/// <summary>");
         
         if (classDef.Properties.Count == 0)
         {
             sb.AppendLine($"{binderIndent}/// Binds a SaveObject to a new {className} instance.");
             sb.AppendLine($"{binderIndent}/// This object has no properties to bind.");
             sb.AppendLine($"{binderIndent}/// It represents an empty object in the save file: {{ }}");
         }
         else
         {
             sb.AppendLine($"{binderIndent}/// Binds a SaveObject to a new {className} instance.");
         }
         
         sb.AppendLine($"{binderIndent}/// </summary>");
         sb.AppendLine($"{binderIndent}/// <param name=\"obj\">The SaveObject to bind. Can be null.</param>");
         sb.AppendLine($"{binderIndent}/// <returns>A new {className} instance or null if input is null.</returns>");
         sb.AppendLine($"{binderIndent}public static {className}? Bind(SaveObject? obj)"); // Nullable return
         sb.AppendLine($"{binderIndent}{{");
         sb.AppendLine($"{binderIndent}    if (obj == null) return null;"); // Handle null input
         sb.AppendLine();
         sb.AppendLine($"{binderIndent}    var model = new {className}();");
         sb.AppendLine();

         // Generate binding logic for each property
         foreach (var property in classDef.Properties)
         {
              var propertyName = property.Name.TrimStart('@');
              // Use the FULL type name for binding logic
              var fullPropertyType = property.PropertyType;
              // Pass the full type name (after trimming '?') to binding logic
              GeneratePropertyBindingLogic(property, propertyName, ConvertPropertyTypeSyntax(fullPropertyType), sb, indentLevel);
         }

         sb.AppendLine();
         sb.AppendLine($"{binderIndent}    return model;");
         sb.AppendLine($"{binderIndent}}}"); // End of binder method

        // Recursive call for nested classes
        foreach (var nestedClass in classDef.NestedClasses ?? Enumerable.Empty<ClassDefinition>()) // Safety check
        {
             sb.AppendLine(); // Add a line break before nested class
             GenerateClassSourceRecursive(nestedClass, sb, indentLevel + 1);
        }

        sb.AppendLine($"{indent}}} // End of class {className}");
    }

    // Helper to add nullable annotation '?' where appropriate
    private static string AddNullableAnnotation(string? typeName)
    {
        if (typeName == null) return "object?"; // Handle null case

        typeName = typeName.Trim();
        if (typeName.EndsWith("?")) // Already nullable
            return typeName;

        // Reference types always get '?' in nullable context
         if (typeName == "string" || typeName == "object" || typeName.Contains("<") || (!IsValueType(typeName) && typeName != "dynamic"))
             return typeName + "?";

        // Value types need '?' unless already Nullable<T>
        if (!typeName.StartsWith("System.Nullable<"))
            return typeName + "?";

        return typeName; // Already Nullable<T> or not applicable
    }

    // Helper used by AddNullableAnnotation
     private static bool IsValueType(string? typeName)
     {
         if (string.IsNullOrEmpty(typeName)) return false;

         // Basic check for known value types + struct heuristic (starts with uppercase)
         var valueTypes = SimpleTypes;
         // Simple struct check: Starts with Upper, not generic, not string/object
         return valueTypes.Contains(typeName) ||
               (!typeName.Contains("<") && typeName != "string" && typeName != "object" && typeName.Length > 0 && char.IsUpper(typeName[0]));
     }

    // Helper to convert property type strings for use in Bind methods
    private static string ConvertPropertyTypeSyntax(string originalType)
    {
        // Trim nullable annotation '?' if present
        return originalType.TrimEnd('?');
    }

    // Helper to check if a type name is a reserved name 
    private static bool IsReservedTypeName(string name) => SimpleTypes.Contains(name);

    // Generate property binding logic based on property type
    private static void GeneratePropertyBindingLogic(PropertyDefinition property, string propertyName, string propertyType, StringBuilder builder, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 4);
        string originalKey = property.OriginalName;
        string varName = propertyName.ToLowerInvariant() + "Value";

        builder.AppendLine($"{indent}            // Bind {propertyName} ({AddNullableAnnotation(propertyType)}) from key \"{originalKey}\"");

        // --- Check for PDX Dictionary Pattern ({ { key {val} } ... }) ---
        if (property.IsPdxDictionaryPattern)
        {
            // Expected format: Dictionary<KeyType, ValueType>
            var pdxDictMatch = Regex.Match(propertyType, @"Dictionary<(?<keyType>\w+),\s*(?<valueType>.+)>");
            if (pdxDictMatch.Success)
            {
                string keyType = pdxDictMatch.Groups["keyType"].Value;
                string valueType = pdxDictMatch.Groups["valueType"].Value; // This includes namespace/nested path
                bool isComplexValueType = !IsSimpleType(valueType);
                
                // Add this line to declare parsedKeyVar for use later
                string parsedKeyVar = "parsedKey"; // Default variable name for parsed key

                builder.AppendLine($"{indent}            if (obj.TryGetSaveArray(\"{originalKey}\", out var {varName}Array) && {varName}Array != null)");
                builder.AppendLine($"{indent}            {{");
                builder.AppendLine($"{indent}                var dict = new {propertyType}();");
                builder.AppendLine($"{indent}                // Each item is a SaveArray with [ScalarID, ObjectValue] pair");
                builder.AppendLine($"{indent}                foreach (var item in {varName}Array.Items)");
                builder.AppendLine($"{indent}                {{");
                builder.AppendLine($"{indent}                    // Each item should be a SaveArray with a Scalar key and SaveObject value");
                builder.AppendLine($"{indent}                    if (item is SaveArray innerArray && innerArray.Items.Count == 2 &&");
                builder.AppendLine($"{indent}                        innerArray.Items[1] is SaveObject valueObj)");
                builder.AppendLine($"{indent}                    {{");

                // Key Parsing Logic - handle both the ORIGINAL pattern and the NESTED ARRAY pattern
                string keyExtractionCode;
                
                if (keyType == "int")
                {
                    keyExtractionCode = $@"{indent}                        // Get the key (first item in the inner array)
{indent}                        var keyItem = innerArray.Items[0];
{indent}                        if (keyItem is Scalar<int> intKeyScalar)
{indent}                        {{
{indent}                            var {parsedKeyVar} = intKeyScalar.Value;"; // Open brace will be closed after value binding
                }
                else if (keyType == "long") 
                {
                    keyExtractionCode = $@"{indent}                        // Get the key (first item in the inner array)
{indent}                        var keyItem = innerArray.Items[0];
{indent}                        if (keyItem is Scalar<long> longKeyScalar)
{indent}                        {{
{indent}                            var {parsedKeyVar} = longKeyScalar.Value;"; // Open brace will be closed after value binding
                }
                else // string or other types
                {
                    keyExtractionCode = $@"{indent}                        // Unsupported key type ""{keyType}"" for this pattern
{indent}                        if (false) {{ // This condition prevents the inner block from executing"; // Open brace will be closed after value binding
                    builder.AppendLine($"{indent}                        // WARN: Unsupported key type \"{keyType}\" for PDX Dictionary pattern where keys are expected to be int/long.");
                }

                builder.AppendLine(keyExtractionCode);
                
                // Value Binding Logic - get the second item (index 1) from the inner array
                string valueBindingIndent = indent + "                            ";
                if (isComplexValueType)
                {
                     // For complex type, we bind the SaveObject at index 1 directly
                     builder.AppendLine($"{valueBindingIndent}// Bind complex value type: {valueType}");
                     // Value object is already extracted in the if condition above as 'valueObj'
                     builder.AppendLine($"{valueBindingIndent}var boundValue = {valueType}.Bind(valueObj);");
                     builder.AppendLine($"{valueBindingIndent}if (boundValue != null)");
                     builder.AppendLine($"{valueBindingIndent}    dict.Add({parsedKeyVar}, boundValue);");
                }
                else // Simple value type
                {
                     string scalarType = GetScalarTypeForElementType(valueType); // Get underlying scalar type (int, string, etc.)
                     
                     // For numeric properties (int, long, float), always try all numeric types with appropriate conversion
                     if (scalarType == "int" || scalarType == "long" || scalarType == "float")
                     {
                         // Use a generic approach for all numeric properties that can handle mixed types
                         builder.AppendLine($"{valueBindingIndent}// Dynamic numeric binding: tries float first, then int with conversion");
                         
                         // For any numeric dictionary value type, try to handle mixed numeric types gracefully
                         if (scalarType == "float")
                         {
                             // When targeting float, prefer float but convert from int if needed
                             builder.AppendLine($"{valueBindingIndent}if (valueObj.TryGetFloat(\"{originalKey}\", out var floatVal))");
                             builder.AppendLine($"{valueBindingIndent}    dict.Add({parsedKeyVar}, floatVal);");
                             builder.AppendLine($"{valueBindingIndent}else if (valueObj.TryGetInt(\"{originalKey}\", out var intVal))");
                             builder.AppendLine($"{valueBindingIndent}    dict.Add({parsedKeyVar}, (float)intVal);");
                         }
                         else if (scalarType == "int")
                         {
                             // When targeting int, try int first but also try float with conversion (possible data loss)
                             builder.AppendLine($"{valueBindingIndent}if (valueObj.TryGetInt(\"{originalKey}\", out var intVal))");
                             builder.AppendLine($"{valueBindingIndent}    dict.Add({parsedKeyVar}, intVal);");
                             builder.AppendLine($"{valueBindingIndent}else if (valueObj.TryGetFloat(\"{originalKey}\", out var floatVal))");
                             builder.AppendLine($"{valueBindingIndent}    dict.Add({parsedKeyVar}, (int)floatVal); // Note: potential data loss from float truncation");
                         }
                         else if (scalarType == "long")
                         {
                             // When targeting long, try all numeric conversions
                             builder.AppendLine($"{valueBindingIndent}if (valueObj.TryGetLong(\"{originalKey}\", out var longVal))");
                             builder.AppendLine($"{valueBindingIndent}    dict.Add({parsedKeyVar}, longVal);");
                             builder.AppendLine($"{valueBindingIndent}else if (valueObj.TryGetInt(\"{originalKey}\", out var intVal))");
                             builder.AppendLine($"{valueBindingIndent}    dict.Add({parsedKeyVar}, (long)intVal);");
                             builder.AppendLine($"{valueBindingIndent}else if (valueObj.TryGetFloat(\"{originalKey}\", out var floatVal))");
                             builder.AppendLine($"{valueBindingIndent}    dict.Add({parsedKeyVar}, (long)floatVal); // Note: potential data loss from float truncation");
                         }
                     }
                     else
                     {
                         // Standard scalar binding for non-numeric properties
                         builder.AppendLine($"{valueBindingIndent}// Standard non-numeric binding for type: {valueType}");
                         builder.AppendLine($"{valueBindingIndent}if (valueObj.TryGetValue(\"{originalKey}\", out var simpleValue) && simpleValue is Scalar<{scalarType}> scalarValue)");
                         builder.AppendLine($"{valueBindingIndent}    dict.Add({parsedKeyVar}, scalarValue.Value);");
                     }
                }

                // Close the key extraction if-block
                builder.AppendLine($"{indent}                        }}"); // End if for key extraction
                
                builder.AppendLine($"{indent}                    }}"); // End check for SaveArray pattern
                builder.AppendLine($"{indent}                }}"); // End foreach item in array
                builder.AppendLine($"{indent}                model.{propertyName} = dict;");
                builder.AppendLine($"{indent}            }}"); // End TryGetSaveArray
            } else {
                 builder.AppendLine($"{indent}            // WARN: Could not parse PDX Dictionary key/value types from '{propertyType}' for key \"{originalKey}\"");
            }
        }
        // --- Check for standard Dictionary<int/string, T> mapped from SaveObject ---
        else if (property.IsDictionary)
        {
            // Handle Dictionary<int, T>
            var intDictMatch = Regex.Match(propertyType, @"Dictionary<int,\s*(.+)>");
            if (intDictMatch.Success)
            {
                var convertedValueType = intDictMatch.Groups[1].Value; // Full value type
                bool isComplexValueType = !IsSimpleType(convertedValueType);
                bool isNestedCollection = convertedValueType.StartsWith("List<") || convertedValueType.StartsWith("Dictionary<");

                builder.AppendLine($"{indent}            if (obj.TryGetSaveObject(\"{originalKey}\", out var {varName}Dict) && {varName}Dict != null)");
                builder.AppendLine($"{indent}            {{");
                builder.AppendLine($"{indent}                var dict = new {propertyType}();");
                builder.AppendLine($"{indent}                foreach (var kvp in {varName}Dict.Properties)");
                builder.AppendLine($"{indent}                {{");
                builder.AppendLine($"{indent}                    if (int.TryParse(kvp.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intKey))");
                builder.AppendLine($"{indent}                    {{");

                if (isNestedCollection) {
                    builder.AppendLine($"{indent}                        // Skip nested collection binding");
                }
                else if (isComplexValueType) {
                     // Use full value type for Bind
                    builder.AppendLine($"{indent}                        var boundValue = {convertedValueType}.Bind(kvp.Value as SaveObject);");
                    builder.AppendLine($"{indent}                        if (boundValue != null)");
                    builder.AppendLine($"{indent}                            dict.Add(intKey, boundValue);");
                } else {
                    string scalarType = GetScalarTypeForElementType(convertedValueType);
                    builder.AppendLine($"{indent}                        if (kvp.Value is Scalar<{scalarType}> scalar)");
                    builder.AppendLine($"{indent}                            dict.Add(intKey, scalar.Value);");
                }

                builder.AppendLine($"{indent}                    }}"); // end if TryParse
                builder.AppendLine($"{indent}                }}"); // end foreach
                builder.AppendLine($"{indent}                model.{propertyName} = dict;");
                builder.AppendLine($"{indent}            }}");
            }
            // Handle Dictionary<string, T>
            else
            {
                var stringDictMatch = Regex.Match(propertyType, @"Dictionary<string,\s*(.+)>");
                if (stringDictMatch.Success)
                {
                    var convertedValueType = stringDictMatch.Groups[1].Value; // Full value type
                    bool isComplexValueType = !IsSimpleType(convertedValueType);
                    bool isNestedCollection = convertedValueType.StartsWith("List<") || convertedValueType.StartsWith("Dictionary<");

                    builder.AppendLine($"{indent}            // Binding a SaveObject as a Dictionary<string, {convertedValueType}>");
                    builder.AppendLine($"{indent}            if (obj.TryGetSaveObject(\"{originalKey}\", out var {varName}Obj) && {varName}Obj != null)");
                    builder.AppendLine($"{indent}            {{");
                    builder.AppendLine($"{indent}                var dict = new {propertyType}();");
                    builder.AppendLine($"{indent}                foreach (var kvp in {varName}Obj.Properties)");
                    builder.AppendLine($"{indent}                {{");

                    if (isNestedCollection) {
                        builder.AppendLine($"{indent}                    // Skip nested collection binding");
                    }
                    else if (isComplexValueType) {
                        // Use full value type for Bind
                        builder.AppendLine($"{indent}                    var boundValue = {convertedValueType}.Bind(kvp.Value as SaveObject);");
                        builder.AppendLine($"{indent}                    if (boundValue != null)");
                        builder.AppendLine($"{indent}                        dict.Add(kvp.Key, boundValue);");
                    } else {
                        string scalarType = GetScalarTypeForElementType(convertedValueType);
                        builder.AppendLine($"{indent}                    if (kvp.Value is Scalar<{scalarType}> scalar)");
                        builder.AppendLine($"{indent}                        dict.Add(kvp.Key, scalar.Value);");
                    }

                    builder.AppendLine($"{indent}                }}"); // end foreach
                    builder.AppendLine($"{indent}                model.{propertyName} = dict;");
                    builder.AppendLine($"{indent}            }}");
                }
                else {
                    builder.AppendLine($"{indent}            // WARN: Could not parse dictionary value type from '{propertyType}' for key \"{originalKey}\"");
                }
            }
        }
        // --- Check for Standard List<T> ---
        else if (property.IsCollection) // Handle List<T>
        {
            var match = Regex.Match(propertyType, @"List<(.+)>");
            if (match.Success) {
                var convertedItemType = match.Groups[1].Value; // Full item type
                bool isComplexItemType = !IsSimpleType(convertedItemType);
                bool isNestedCollection = convertedItemType.StartsWith("List<") || convertedItemType.StartsWith("Dictionary<");

                builder.AppendLine($"{indent}            if (obj.TryGetSaveArray(\"{originalKey}\", out var {varName}Array) && {varName}Array != null)");
                builder.AppendLine($"{indent}            {{");
                builder.AppendLine($"{indent}                var list = new {propertyType}();");

                if (isNestedCollection) {
                    builder.AppendLine($"{indent}                // Skip nested collection binding");
                }
                else {
                    builder.AppendLine($"{indent}                foreach (var item in {varName}Array.Items)");
                    builder.AppendLine($"{indent}                {{");

                    if (isComplexItemType) {
                         // Use full item type for Bind
                        builder.AppendLine($"{indent}                    var boundItem = {convertedItemType}.Bind(item as SaveObject);");
                        builder.AppendLine($"{indent}                    if (boundItem != null)");
                        builder.AppendLine($"{indent}                        list.Add(boundItem);");
                    } else {
                        string scalarType = GetScalarTypeForElementType(convertedItemType);
                        builder.AppendLine($"{indent}                    if (item is Scalar<{scalarType}> scalar)");
                        builder.AppendLine($"{indent}                        list.Add(scalar.Value);");
                    }

                    builder.AppendLine($"{indent}                }}"); // end foreach
                }

                builder.AppendLine($"{indent}                model.{propertyName} = list;");
                builder.AppendLine($"{indent}            }}");
            } else {
                builder.AppendLine($"{indent}            // WARN: Could not parse list item type from '{propertyType}' for key \"{originalKey}\"");
            }
        }
        // --- Check for Complex Nested Object ---
        else if (!IsSimpleType(propertyType)) // Complex nested object (T)
        {
            builder.AppendLine($"{indent}            if (obj.TryGetSaveObject(\"{originalKey}\", out var {varName}Obj))");
            builder.AppendLine($"{indent}                model.{propertyName} = {propertyType}.Bind({varName}Obj);");
        }
        // --- Handle Simple Scalar Properties ---
        else // Simple scalar property (T?)
        {
            string nullableCast = $"({AddNullableAnnotation(propertyType)})";
            switch (propertyType)
            {
                 case "string":
                     builder.AppendLine($"{indent}            model.{propertyName} = obj.TryGetString(\"{originalKey}\", out var {varName}) && {varName} != \"none\" ? {varName} : null;");
                     break;
                 case "int":
                     builder.AppendLine($"{indent}            model.{propertyName} = obj.TryGetInt(\"{originalKey}\", out var {varName}) ? {varName} : {nullableCast}null;");
                     break;
                 case "long":
                     builder.AppendLine($"{indent}            model.{propertyName} = obj.TryGetLong(\"{originalKey}\", out var {varName}) ? {varName} : {nullableCast}null;");
                     break;
                 case "float":
                     builder.AppendLine($"{indent}            model.{propertyName} = obj.TryGetFloat(\"{originalKey}\", out var {varName}) ? {varName} : {nullableCast}null;");
                     break;
                 case "bool":
                      builder.AppendLine($"{indent}            model.{propertyName} = obj.TryGetBool(\"{originalKey}\", out var {varName}) ? {varName} : {nullableCast}null;");
                     break;
                 case "DateTime":
                      builder.AppendLine($"{indent}            model.{propertyName} = obj.TryGetDateTime(\"{originalKey}\", out var {varName}) ? {varName} : {nullableCast}null;");
                     break;
                 case "Guid":
                      builder.AppendLine($"{indent}            model.{propertyName} = obj.TryGetGuid(\"{originalKey}\", out var {varName}) ? {varName} : {nullableCast}null;");
                     break;
                 default:
                      builder.AppendLine($"{indent}            // WARN: Cannot bind unknown simple type '{propertyType}' for key \"{originalKey}\"");
                      break;
            }
        }
        builder.AppendLine();
    } // End GeneratePropertyBindingLogic

    // Helper to get the underlying C# type for Scalar<T>
    private static string GetScalarTypeForElementType(string elementType)
    {
        // elementType should already be the C# type name (e.g., "int", "float", "DateTime")
        return elementType switch
        {
            "int" => "int", // System.Int32
            "long" => "long", // System.Int64
            "float" => "float", // System.Single
            "double" => "double", // System.Double
            "bool" => "bool", // System.Boolean
            "string" => "string", // System.String
            "DateTime" => "System.DateTime", // Need namespace if not globally using
            "Guid" => "System.Guid", // Need namespace if not globally using
            _ => elementType // Assume it's a type name that works directly (like a custom struct)
        };
    }

    // Helper to check if a type is a simple type
    private static bool IsSimpleType(string? typeName) => 
        typeName != null && SimpleTypes.Contains(typeName.TrimEnd('?'));
}


