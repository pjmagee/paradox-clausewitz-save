using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Text.RegularExpressions;

namespace MageeSoft.PDX.CE.SourceGenerator;

/// <summary>
/// Helper class for generating models from PDX save files
/// </summary>
public static class StellarisModelsGenerator
{
    private const string Namespace = "MageeSoft.PDX.CE.Models";

     public static void GenerateModels(SourceProductionContext context, AdditionalText file, IEnumerable<SaveObjectAnalysis> analyses)
    {
        try
        {
            // Add explicit diagnostic to log which file is being processed
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("PDXSG100", "Processing File", $"Processing file: {file.Path}", "StellarisModelsGenerator", DiagnosticSeverity.Info, true),
                Location.None));

            var analysisList = analyses?.ToList() ?? new List<SaveObjectAnalysis>(); // Handle potential null

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
                 if (analysis.Error != null && !analysis.Diagnostics.Any(d => d.Id == "PDXSA003")) // Avoid duplicate reporting if already logged as diagnostic
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

            foreach (var analysis in analysisList.Where(a => a.Error == null)) // Only process successful analyses
            {
                 // Determine a specific namespace segment for this analysis root
                 // Use the RootName which should be unique per file (e.g., "GameState", "Meta")
                 string rootNamespaceSegment = analysis.RootName; // Assuming RootName is PascalCased and valid for namespace
                 // No longer nesting under Generated, just use the root namespace directly
                 string fullNamespace = Namespace;

                 // Enhanced check for no definitions
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


                 // Iterate through ONLY the TOP-LEVEL classes defined for this analysis root
                foreach (var topLevelClassDef in analysis.ClassDefinitions.Values)
                {
                     // Add check for null/empty class name before proceeding
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
                     // Optional: Add using for the base generated namespace if needed?
                     // fileContentBuilder.AppendLine($"using {Namespace};");
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
                     // Use only the class name for the hint name, not prefixed with rootNamespaceSegment
                     context.AddSource($"{topLevelClassDef.Name}.g.cs", SourceText.From(fileContentBuilder.ToString(), Encoding.UTF8));

                    context.ReportDiagnostic(Diagnostic.Create(
                       new DiagnosticDescriptor("PDXSG008", "Generated Top-Level Class", $"Generated model file {topLevelClassDef.Name}.g.cs for top-level class {topLevelClassDef.Name}.", "StellarisModelsGenerator", DiagnosticSeverity.Info, true),
                       Location.None));
                }
            }
        }
        catch (Exception ex) // Catch unexpected exceptions during the generation phase itself
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
    /// <param name="classDef">The class definition to generate.</param>
    /// <param name="sb">The StringBuilder to append the source code to.</param>
    /// <param name="indentLevel">The current indentation level (number of 4-space indents).</param>
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
            // Get the type syntax suitable for declaration (uses simple name for nested types)
            // REVERTED: Use the PropertyType (FullName) directly for declaration now
            // var declarationTypeSyntax = DeterminePropertyTypeSyntaxForDeclaration(property, classDef);
            var nullableDeclarationType = AddNullableAnnotation(property.PropertyType); // Use FullName from PropertyType

            // REMOVED Attribute Generation
            /*
            string attribute;
            if (property.IsDictionary)
            {
                 attribute = $[{property.AttributeType}("{property.OriginalName}")];
            }
            sb.AppendLine($"{propertyIndent}{attribute}");
            */

            // Use the property.Name (simple) and the FullName-based type for the property definition
            sb.AppendLine($"{propertyIndent}public {nullableDeclarationType} {propertyName} {{ get; set; }}");
            sb.AppendLine(); // Blank line after property
        }

         // Generate Binder Method
         // Indent the binder method relative to the class
         string binderIndent = propertyIndent; // Same indent as properties
         sb.AppendLine(); // Blank line before binder
         sb.AppendLine($"{binderIndent}/// <summary>");
         sb.AppendLine($"{binderIndent}/// Binds a SaveObject to a new {className} instance.");
         sb.AppendLine($"{binderIndent}/// </summary>");
         sb.AppendLine($"{binderIndent}/// <param name=\"obj\">The SaveObject to bind. Can be null.</param>");
         sb.AppendLine($"{binderIndent}/// <returns>A new {className} instance or null if input is null.</returns>");
         sb.AppendLine($"{binderIndent}public static {className}? Bind(SaveObject? obj)"); // Nullable return
         sb.AppendLine($"{binderIndent}{{");
         sb.AppendLine($"{binderIndent}    if (obj == null) return null;"); // Handle null input
         sb.AppendLine();
         sb.AppendLine($"{binderIndent}    var model = new {className}();");
         sb.AppendLine();

         // Generate binding logic for each property (use the helper)
         foreach (var property in classDef.Properties)
         {
              var propertyName = property.Name.TrimStart('@');
              // Use the FULL type name (PropertyType from definition) for binding logic
              var fullPropertyType = property.PropertyType;
              // Pass the full type name (after trimming '?') to binding logic
              GeneratePropertyBindingLogic(property, propertyName, ConvertPropertyTypeSyntax(fullPropertyType), sb, indentLevel + 1); // Pass StringBuilder and indent + 1 for binder logic
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

    // Helper to add nullable annotation '?' where appropriate (updated)
    private static string AddNullableAnnotation(string? typeName)
    {
        if (typeName == null) return "object?"; // Handle null case

        typeName = typeName.Trim();
        if (typeName.EndsWith("?")) // Already nullable
            return typeName;

        // Reference types (string, object, classes, List<>, Dictionary<>) always get '?' in nullable context
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
         var valueTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
             "int", "long", "float", "double", "bool", "DateTime", "Guid",
             "decimal", "byte", "sbyte", "short", "ushort", "uint", "ulong", "char",
             "System.Int32", "System.Int64", "System.Single", "System.Double", "System.Boolean",
             "System.DateTime", "System.Guid", "System.Decimal" /* etc */ };
         // Simple struct check: Starts with Upper, not generic, not string/object
         return valueTypes.Contains(typeName) ||
               (!typeName.Contains("<") && typeName != "string" && typeName != "object" && typeName.Length > 0 && char.IsUpper(typeName[0]));
     }


    // Helper to convert property type strings for use in Bind methods (PascalCases nested types)
    // Needs to handle potentially nullable types coming from analyzer now.
    // SIMPLIFIED: Analyzer now provides the correct FullName directly in PropertyType.
    private static string ConvertPropertyTypeSyntax(string originalType)
    {
        // Trim nullable annotation '?' if present
        return originalType.TrimEnd('?');
        
        /* REMOVED OLD LOGIC
        if (IsSimpleType(originalType)) return originalType; // Simple types don't need PascalCase

        if (originalType.StartsWith("List<"))
        {
            // Extract item type, remove '?', PascalCase it, put back into List<>
            var itemType = originalType.Substring(5, originalType.Length - 6).TrimEnd(';');
            var pascalItemType = ConvertPropertyTypeSyntax(itemType); // Recursive call to handle nested complex types
            return $"List<{pascalItemType}>";
        }
        else if (originalType.StartsWith("Dictionary<"))
        {
            // Extract key/value types, remove '?', PascalCase value type, put back into Dictionary<>
             var parts = originalType.Substring(11, originalType.Length - 12).Split(new[] { ',' }, 2);
             if (parts.Length == 2) {
                  var keyType = parts[0].Trim(); // Keys are usually simple (int/string)
                  var valueType = parts[1].Trim().TrimEnd(';');
                  var pascalValueType = ConvertPropertyTypeSyntax(valueType); // Recursive call
                  return $"Dictionary<{keyType}, {pascalValueType}>";
             } else {
                  // Fallback: Try PascalCase on the whole original string if format is unexpected
                  return ToPascalCaseGenerator(originalType.TrimEnd(';')); // Use a basic PascalCase if analyzer failed
             }
        }
         // If not List/Dictionary or simple, assume it's a class name.
         // Use the name directly as Analyzer should have PascalCased it.
         // If we *must* convert here, use a simple local converter.
         return ToPascalCaseGenerator(originalType); // Use a basic converter just in case
        */
    }

     // Basic PascalCase needed locally if ConvertPropertyTypeSyntax needs a fallback
     private static string ToPascalCaseGenerator(string input)
     {
         if (string.IsNullOrEmpty(input) || (!input.Contains('_') && !input.Contains('-') && char.IsUpper(input[0])))
             return input; // Assume already PascalCase or simple type

         input = input.Replace("-", "_");
         var parts = input.Split('_');
         var builder = new StringBuilder();
         foreach (var part in parts.Where(p => p.Length > 0))
         {
             builder.Append(char.ToUpperInvariant(part[0]));
             if (part.Length > 1) // Avoid error on single char parts
                builder.Append(part.Substring(1).ToLowerInvariant());
         }
         var result = builder.ToString();
         return string.IsNullOrEmpty(result) ? "_" : result;
     }


    // GeneratePropertyBindingLogic (Syntax corrections applied)
    private static void GeneratePropertyBindingLogic(PropertyDefinition property, string propertyName, string propertyType, StringBuilder builder, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 4);
        string originalKey = property.OriginalName;
        string varName = propertyName.ToLowerInvariant() + "Value";

        builder.AppendLine($"{indent}            // Bind {propertyName} ({AddNullableAnnotation(propertyType)}) from key \"{originalKey}\"");

        if (property.IsDictionary) // Handle Dictionary<int, T>
        {
            // Handle Dictionary<int, T>
            var intDictMatch = Regex.Match(propertyType, @"Dictionary<int,\s*(.+)>");
            if (intDictMatch.Success) 
            {
                var convertedValueType = intDictMatch.Groups[1].Value;
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
                    // Handle nested collections specially - we can't call Bind() on them
                    builder.AppendLine($"{indent}                        // For nested collections, we need to handle specially");
                    builder.AppendLine($"{indent}                        // TODO: Implement proper nested collection binding");
                    builder.AppendLine($"{indent}                        // Currently skipping this since we can't call Bind() on collection types");
                }
                else if (isComplexValueType) {
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
                    var convertedValueType = stringDictMatch.Groups[1].Value;
                    bool isComplexValueType = !IsSimpleType(convertedValueType);
                    bool isNestedCollection = convertedValueType.StartsWith("List<") || convertedValueType.StartsWith("Dictionary<");

                    builder.AppendLine($"{indent}            if (obj.TryGetSaveObject(\"{originalKey}\", out var {varName}Dict) && {varName}Dict != null)");
                    builder.AppendLine($"{indent}            {{");
                    builder.AppendLine($"{indent}                var dict = new {propertyType}();");
                    builder.AppendLine($"{indent}                foreach (var kvp in {varName}Dict.Properties)");
                    builder.AppendLine($"{indent}                {{");
                    // No need to parse the key - use it directly as string
                    
                    if (isNestedCollection) {
                        builder.AppendLine($"{indent}                    // For nested collections, we need to handle specially");
                        builder.AppendLine($"{indent}                    // TODO: Implement proper nested collection binding");
                        builder.AppendLine($"{indent}                    // Currently skipping this since we can't call Bind() on collection types");
                    }
                    else if (isComplexValueType) {
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
        else if (!property.IsDictionary && propertyType.StartsWith("Dictionary<"))
        {
            // Handle the case where we've detected a SaveObject that is actually a dictionary
            // But didn't mark it with IsDictionary=true
            
            var stringDictMatch = Regex.Match(propertyType, @"Dictionary<string,\s*(.+)>");
            if (stringDictMatch.Success)
            {
                var convertedValueType = stringDictMatch.Groups[1].Value;
                bool isComplexValueType = !IsSimpleType(convertedValueType);
                bool isNestedCollection = convertedValueType.StartsWith("List<") || convertedValueType.StartsWith("Dictionary<");

                builder.AppendLine($"{indent}            // Binding a SaveObject as a Dictionary<string, {convertedValueType}>");
                builder.AppendLine($"{indent}            if (obj.TryGetSaveObject(\"{originalKey}\", out var {varName}Obj) && {varName}Obj != null)");
                builder.AppendLine($"{indent}            {{");
                builder.AppendLine($"{indent}                var dict = new {propertyType}();");
                builder.AppendLine($"{indent}                foreach (var kvp in {varName}Obj.Properties)");
                builder.AppendLine($"{indent}                {{");
                
                if (isNestedCollection) {
                    builder.AppendLine($"{indent}                    // For nested collections, we need to handle specially");
                    builder.AppendLine($"{indent}                    // TODO: Implement proper nested collection binding");
                }
                else if (isComplexValueType) {
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
                // Handle other cases - fall through to existing code
                if (property.IsCollection) // Handle List<T>
                {
                    var match = Regex.Match(propertyType, @"List<(.+)>");
                    if (match.Success) {
                        var convertedItemType = match.Groups[1].Value;
                        bool isComplexItemType = !IsSimpleType(convertedItemType);
                        bool isNestedCollection = convertedItemType.StartsWith("List<") || convertedItemType.StartsWith("Dictionary<");

                        builder.AppendLine($"{indent}            if (obj.TryGetSaveArray(\"{originalKey}\", out var {varName}Array) && {varName}Array != null)");
                        builder.AppendLine($"{indent}            {{");
                        builder.AppendLine($"{indent}                var list = new {propertyType}();");
                        
                        if (isNestedCollection) {
                            // Special handling for nested collections like List<List<int>>
                            builder.AppendLine($"{indent}                // TODO: This is a nested collection ({propertyType})");
                            builder.AppendLine($"{indent}                // We can't call Bind() on collection types like List<T> or Dictionary<K,V>");
                            builder.AppendLine($"{indent}                // For now, skipping this nested collection binding");
                        }
                        else {
                            builder.AppendLine($"{indent}                foreach (var item in {varName}Array.Items)");
                            builder.AppendLine($"{indent}                {{");
                            
                            if (isComplexItemType) {
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
                else if (!IsSimpleType(propertyType)) // Complex nested object (T)
                {
                    builder.AppendLine($"{indent}            if (obj.TryGetSaveObject(\"{originalKey}\", out var {varName}Obj))");
                    // Ensure the Bind call uses the correct, potentially nested, type name
                     string bindTargetType = propertyType; // Already PascalCased by ConvertPropertyTypeSyntax called earlier
                    builder.AppendLine($"{indent}                model.{propertyName} = {bindTargetType}.Bind({varName}Obj);");
                }
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
            }
        }
        else if (property.IsCollection) // Handle List<T>
        {
            var match = Regex.Match(propertyType, @"List<(.+)>");
            if (match.Success) {
                var convertedItemType = match.Groups[1].Value;
                bool isComplexItemType = !IsSimpleType(convertedItemType);
                bool isNestedCollection = convertedItemType.StartsWith("List<") || convertedItemType.StartsWith("Dictionary<");

                builder.AppendLine($"{indent}            if (obj.TryGetSaveArray(\"{originalKey}\", out var {varName}Array) && {varName}Array != null)");
                builder.AppendLine($"{indent}            {{");
                builder.AppendLine($"{indent}                var list = new {propertyType}();");
                
                if (isNestedCollection) {
                    // Special handling for nested collections like List<List<int>>
                    builder.AppendLine($"{indent}                // TODO: This is a nested collection ({propertyType})");
                    builder.AppendLine($"{indent}                // We can't call Bind() on collection types like List<T> or Dictionary<K,V>");
                    builder.AppendLine($"{indent}                // For now, skipping this nested collection binding");
                }
                else {
                    builder.AppendLine($"{indent}                foreach (var item in {varName}Array.Items)");
                    builder.AppendLine($"{indent}                {{");
                    
                    if (isComplexItemType) {
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
        else if (!IsSimpleType(propertyType)) // Complex nested object (T)
        {
            builder.AppendLine($"{indent}            if (obj.TryGetSaveObject(\"{originalKey}\", out var {varName}Obj))");
            // Ensure the Bind call uses the correct, potentially nested, type name
             string bindTargetType = propertyType; // Already PascalCased by ConvertPropertyTypeSyntax called earlier
            builder.AppendLine($"{indent}                model.{propertyName} = {bindTargetType}.Bind({varName}Obj);");
        }
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
        }; // Corrected semicolon for switch expression
    } // Corrected closing brace for method

    // Keep the SimpleTypes definition - use OrdinalIgnoreCase for comparisons
    private static readonly HashSet<string> SimpleTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "string", "int", "long", "float", "double", "bool", "DateTime", "Guid",
        "System.String", "System.Int32", "System.Int64", "System.Single",
        "System.Double", "System.Boolean", "System.DateTime", "System.Guid"
    };
    // Updated IsSimpleType to handle potential null and nullable annotation '?'
    private static bool IsSimpleType(string? typeName) => 
        typeName != null && SimpleTypes.Contains(typeName.TrimEnd('?'));

    // Add this method to check for reserved type names
    private static bool IsReservedTypeName(string name)
    {
        // Check if it's a simple built-in type
        if (SimpleTypes.Contains(name))
            return true;

        // Additional check for other C# keywords and common types
        var additionalReservedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // C# keywords
            "abstract", "as", "base", "break", "case", "catch", "checked",
            "class", "const", "continue", "default", "delegate", "do", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "for",
            "foreach", "goto", "if", "implicit", "in", "interface", "internal", "is", "lock",
            "namespace", "new", "null", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sealed",
            "sizeof", "stackalloc", "static", "struct", "switch", "this", "throw",
            "true", "try", "typeof", "unchecked", "unsafe", "using",
            "virtual", "void", "volatile", "while",
            
            // Common .NET types
            "TimeSpan", "Uri", "Version", "Type", "Exception",
            "List", "Dictionary", "HashSet", "Queue", "Stack", "Tuple", "Task"
        };

        return additionalReservedNames.Contains(name);
    }
} // End class


