using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;

namespace MageeSoft.PDX.CE.SourceGenerator;

[Generator]
public class IncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => 
            ctx.AddSource("InitializationDiagnostic.g.cs", 
                """
                // Code generator initialized
                // This file is just a marker for debugging
                """
            )
        );

        // Get MSBuild Properties to conditionally disable code generation
        IncrementalValueProvider<bool> generateModels = context.AnalyzerConfigOptionsProvider
            .Select((provider, _) => 
            {
                // Check if generation is disabled via MSBuild property
                if (provider.GlobalOptions.TryGetValue("build_property.PDXGenerateModels", out var value) &&
                    value.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine("SOURCE GENERATOR: Skipping code generation due to PDXGenerateModels=false");
                    return false;
                }
                return true;
            });

        // Output a diagnostic indicating whether code generation is enabled
        context.RegisterSourceOutput(generateModels, (ctx, enabled) => 
        {
            if (!enabled)
            {
                ctx.AddSource("GenerationDisabled.g.cs", 
                    """
                    // Code generation is disabled via PDXGenerateModels=false
                    // This file is just a marker
                    """
                );
                
                ctx.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("PDXSG300", "Code Generation Disabled", 
                        "PDX model code generation is disabled via PDXGenerateModels=false property", 
                        "StellarisCodeGenerator", DiagnosticSeverity.Info, true),
                    null
                ));
            }
        });

        // Find classes with GameStateDocumentAttribute
        IncrementalValuesProvider<AttributedClass> attributedClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax cls && cls.AttributeLists.Count > 0,
                transform: static (ctx, ct) => GetAttributedClass(ctx, ct)
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // Register additional files for schema discovery
        var additionalFiles = context.AdditionalTextsProvider;
        
        // Add logging to show file paths
        var filePathsPipeline = additionalFiles.Select((file, _) => file.Path);

        context.RegisterSourceOutput(filePathsPipeline, (ctx, path) => 
        {
            Debug.WriteLine($"SOURCE GENERATOR: Found additional file: {path}");
            ctx.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("PDXSG200", "Additional File", $"Found additional file: {path}", "StellarisCodeGenerator", DiagnosticSeverity.Info, true),
                null
            ));
        });

        // Combine the attributed classes with additional files and generation flag
        var combinedInputs = attributedClasses
            .Combine(additionalFiles.Collect())
            .Combine(generateModels)
            .Where(pair => pair.Right) // Only process if generation is enabled
            .Select((pair, _) => (AttributedClass: pair.Left.Left, AdditionalFiles: pair.Left.Right));

        // Generate code for each attributed class
        context.RegisterSourceOutput(combinedInputs, (ctx, data) => 
        {
            var (attributedClass, afs) = data;
            
            // Find the matching schema file
            var matchingFile = FindMatchingSchemaFile(attributedClass, afs);
            if (matchingFile == null)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("PDXSG201", "Schema File Not Found", 
                        $"Schema file '{attributedClass.SchemaFileName}' not found for class {attributedClass.Namespace}.{attributedClass.ClassName}", 
                        "StellarisCodeGenerator", DiagnosticSeverity.Warning, true),
                    null
                ));
                return;
            }

            // Generate models for the class based on the schema file
            GenerateModelsForAttributedClass(ctx, attributedClass, matchingFile);
        });
    }

    private static void GenerateModelsForAttributedClass(SourceProductionContext ctx, AttributedClass attributedClass, AdditionalText schemaFile)
    {
        try
        {
            Debug.WriteLine($"SOURCE GENERATOR: Generating code for {attributedClass.ClassName} using schema file {schemaFile.Path}");
            ctx.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("PDXSG202", "Generating Code", 
                    $"Generating code for {attributedClass.ClassName} using schema file {schemaFile.Path}", 
                    "StellarisCodeGenerator", DiagnosticSeverity.Info, true),
                null
            ));
            
            ctx.CancellationToken.ThrowIfCancellationRequested();
            
            var analyses = AnalyzeFile(schemaFile, ctx.CancellationToken);
            
            // Convert to list to work with multiple times
            var analysesList = analyses.ToList();
            
            if (analysesList.Any())
            {
                // Log what analyses were found before filtering
                ctx.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("PDXSG205", "Analyses Before Filtering", 
                        $"Found {analysesList.Count} analyses in schema file {Path.GetFileName(schemaFile.Path)} before filtering: " +
                        string.Join(", ", analysesList.Select(a => a.RootName)), 
                        "StellarisCodeGenerator", DiagnosticSeverity.Info, true),
                    null
                ));
                
                // Filter the analyses to only include the ones relevant to this class type
                var analysesForClass = FilterAnalysesForAttributedClass(analysesList, attributedClass).ToList();
                
                // Log the filtered analyses
                ctx.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("PDXSG206", "Analyses After Filtering", 
                        $"After filtering for {attributedClass.ClassName}: Found {analysesForClass.Count} analyses in schema file {Path.GetFileName(schemaFile.Path)}: " +
                        string.Join(", ", analysesForClass.Select(a => a.RootName)), 
                        "StellarisCodeGenerator", DiagnosticSeverity.Info, true),
                    null
                ));
                
                if (analysesForClass.Any())
                {
                    // Generate code for this class as a partial class
                    ModelsGenerator.GeneratePartialClass(ctx, attributedClass, schemaFile, analysesForClass);
                }
                else
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("PDXSG207", "No Matching Analyses", 
                            $"No matching analyses found for class {attributedClass.ClassName} in schema file {schemaFile.Path} after filtering", 
                            "StellarisCodeGenerator", DiagnosticSeverity.Warning, true),
                        null
                    ));
                    
                    // If no analyses matched after filtering, try using all of them as a fallback
                    // This is a workaround that might help in some cases
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("PDXSG208", "Fallback to All Analyses", 
                            $"Attempting fallback to use all analyses for class {attributedClass.ClassName}", 
                            "StellarisCodeGenerator", DiagnosticSeverity.Info, true),
                        null
                    ));
                    
                    ModelsGenerator.GeneratePartialClass(ctx, attributedClass, schemaFile, analysesList);
                }
            }
            else
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("PDXSG203", "No Analyses Found", 
                        $"No analyses found in schema file {schemaFile.Path} for class {attributedClass.ClassName}", 
                        "StellarisCodeGenerator", DiagnosticSeverity.Warning, true),
                    null
                ));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SOURCE GENERATOR: ERROR in GenerateModelsForAttributedClass: {ex.Message}\n{ex.StackTrace}");
            ctx.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("PDXSG204", "Generation Error", 
                    $"Error generating code for {attributedClass.ClassName}: {ex.Message}", 
                    "StellarisCodeGenerator", DiagnosticSeverity.Error, true),
                null
            ));
        }
    }

    /// <summary>
    /// DEPRECATED: Use FilterAnalysesForAttributedClass instead.
    /// This method relies on hardcoded class names rather than using attribute information.
    /// Maintained for backward compatibility.
    /// </summary>
    private static IEnumerable<SaveObjectAnalysis> FilterAnalysesForClass(IEnumerable<SaveObjectAnalysis> analyses, string className)
    {
        // Use file name from GameStateDocumentAttribute to determine which analyses to include
        if (className.Equals("Meta", StringComparison.OrdinalIgnoreCase))
        {
            // Meta class: filter to analyses that are from meta files
            return analyses.Where(a => a.RootName.EndsWith("meta", StringComparison.OrdinalIgnoreCase));
        }
        else if (className.Equals("Gamestate", StringComparison.OrdinalIgnoreCase))
        {
            // Gamestate class: filter to analyses that are from gamestate files
            return analyses.Where(a => !a.RootName.EndsWith("meta", StringComparison.OrdinalIgnoreCase));
        }
        
        // Default - return all analyses if class name doesn't match known patterns
        return analyses;
    }

    // Add a new method that uses the AttributedClass directly for filtering
    private static IEnumerable<SaveObjectAnalysis> FilterAnalysesForAttributedClass(IEnumerable<SaveObjectAnalysis> analyses, AttributedClass attributedClass)
    {
        string schemaFileName = attributedClass.SchemaFileName;
        
        // Extract the base name without extension for matching with analysis root names
        string schemaBaseName = Path.GetFileNameWithoutExtension(schemaFileName).ToLowerInvariant();
        
        // Meta class handling - used for meta files
        if (attributedClass.ClassName.Equals("Meta", StringComparison.OrdinalIgnoreCase) ||
            schemaBaseName.EndsWith("meta"))
        {
            Debug.WriteLine($"SOURCE GENERATOR: Filtering analyses for Meta class using schema {schemaFileName}");
            return analyses.Where(a => a.RootName.EndsWith("meta", StringComparison.OrdinalIgnoreCase));
        }
        
        // Gamestate class handling - used for gamestate files
        if (attributedClass.ClassName.Equals("Gamestate", StringComparison.OrdinalIgnoreCase) ||
            schemaBaseName.Equals("gamestate", StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine($"SOURCE GENERATOR: Filtering analyses for Gamestate class using schema {schemaFileName}");
            return analyses.Where(a => !a.RootName.EndsWith("meta", StringComparison.OrdinalIgnoreCase));
        }
        
        // For other classes, try to match directly by root name
        var result = analyses.Where(a => a.RootName.Equals(schemaBaseName, StringComparison.OrdinalIgnoreCase));
        
        // If we didn't find any matches, log this and return all analyses
        if (!result.Any())
        {
            Debug.WriteLine($"SOURCE GENERATOR: No analyses matched schema {schemaFileName}, returning all analyses");
            return analyses;
        }
        
        return result;
    }

    private static AdditionalText? FindMatchingSchemaFile(AttributedClass attributedClass, ImmutableArray<AdditionalText> additionalFiles)
    {
        Debug.WriteLine($"SOURCE GENERATOR: Looking for schema file {attributedClass.SchemaFileName} for class {attributedClass.ClassName}");
        Debug.WriteLine($"SOURCE GENERATOR: Available additional files: {additionalFiles.Length}");
        
        // Log all available additional files
        foreach (var file in additionalFiles)
        {
            Debug.WriteLine($"SOURCE GENERATOR: Additional file: {file.Path}");
        }
        
        // Try to find an exact match first
        var exactMatch = additionalFiles.FirstOrDefault(f => 
            Path.GetFileName(f.Path).Equals(attributedClass.SchemaFileName, StringComparison.OrdinalIgnoreCase));
        
        if (exactMatch != null)
        {
            Debug.WriteLine($"SOURCE GENERATOR: Found exact match for schema file: {exactMatch.Path}");
            return exactMatch;
        }
        
        Debug.WriteLine($"SOURCE GENERATOR: No exact match found for {attributedClass.SchemaFileName}");
        
        // Try to match by base name (without extension) if exact match not found
        string baseSchemaName = Path.GetFileNameWithoutExtension(attributedClass.SchemaFileName);
        Debug.WriteLine($"SOURCE GENERATOR: Looking for files with base name: {baseSchemaName}");
        
        var baseNameMatch = additionalFiles.FirstOrDefault(f => 
            Path.GetFileNameWithoutExtension(f.Path).Equals(baseSchemaName, StringComparison.OrdinalIgnoreCase));
            
        if (baseNameMatch != null)
        {
            Debug.WriteLine($"SOURCE GENERATOR: Found base name match: {baseNameMatch.Path}");
            return baseNameMatch;
        }
        
        Debug.WriteLine($"SOURCE GENERATOR: No schema file found for {attributedClass.SchemaFileName}");
        return null;
    }

    private static AttributedClass? GetAttributedClass(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        
        // Log the class being processed
        var className = classDeclaration.Identifier.Text;
        Debug.WriteLine($"SOURCE GENERATOR: Checking class {className} for attributes");
        
        // Check if the class is partial
        bool isPartial = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        if (!isPartial)
        {
            Debug.WriteLine($"SOURCE GENERATOR: Class {className} is not partial, skipping");
            return null; // Skip non-partial classes
        }
        
        // Get the semantic model
        var semanticModel = context.SemanticModel;
        
        // Get the symbol for the class
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken);
        if (classSymbol == null)
        {
            Debug.WriteLine($"SOURCE GENERATOR: Could not get symbol for class {className}, skipping");
            return null;
        }
        
        // Log the namespace
        Debug.WriteLine($"SOURCE GENERATOR: Class {className} is in namespace {classSymbol.ContainingNamespace}");
        
        // Check for GameStateDocumentAttribute
        var attributes = classSymbol.GetAttributes();
        Debug.WriteLine($"SOURCE GENERATOR: Class {className} has {attributes.Length} attributes");
        
        // Log all attributes
        foreach (var attr in attributes)
        {
            Debug.WriteLine($"SOURCE GENERATOR: Class {className} has attribute {attr.AttributeClass?.ToDisplayString() ?? "unknown"}");
        }
        
        var gameStateDocAttr = attributes.FirstOrDefault(attr => 
            attr.AttributeClass?.ToDisplayString() == "MageeSoft.PDX.CE.GameStateDocumentAttribute");
        
        if (gameStateDocAttr == null)
        {
            Debug.WriteLine($"SOURCE GENERATOR: Class {className} does not have GameStateDocumentAttribute, skipping");
            return null; // No attribute found
        }
        
        Debug.WriteLine($"SOURCE GENERATOR: Class {className} has GameStateDocumentAttribute");
        
        // Get the schema file name from the attribute
        string? schemaFileName = null;
        if (gameStateDocAttr.ConstructorArguments.Length > 0 && 
            gameStateDocAttr.ConstructorArguments[0].Value is string fileName)
        {
            schemaFileName = fileName;
            Debug.WriteLine($"SOURCE GENERATOR: Class {className} has schema file name: {schemaFileName}");
        }
        
        if (string.IsNullOrEmpty(schemaFileName))
        {
            Debug.WriteLine($"SOURCE GENERATOR: Class {className} has empty schema file name, skipping");
            return null; // No schema file specified
        }
        
        // Create the AttributedClass object
        var result = new AttributedClass
        {
            ClassName = classSymbol.Name,
            Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
            SchemaFileName = schemaFileName
        };
        
        Debug.WriteLine($"SOURCE GENERATOR: Created AttributedClass for {result.ClassName} in {result.Namespace} with schema {result.SchemaFileName}");
        return result;
    }
    
    private static IEnumerable<SaveObjectAnalysis> AnalyzeFile(AdditionalText text, CancellationToken cancellationToken)
    {
        Debug.WriteLine($"SOURCE GENERATOR: AnalyzeFile called for {text.Path}");
        
        // Check the file content
        try
        {
            // Try to get the file content length
            var content = text.GetText(cancellationToken)?.ToString() ?? "";
            Debug.WriteLine($"SOURCE GENERATOR: File content length: {content.Length} characters for {text.Path}");
            
            if (string.IsNullOrWhiteSpace(content))
            {
                Debug.WriteLine($"SOURCE GENERATOR: WARNING - File appears to be empty: {text.Path}");
            }
            else 
            {
                Debug.WriteLine($"SOURCE GENERATOR: File content starts with: {content.Substring(0, Math.Min(50, content.Length))}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SOURCE GENERATOR: ERROR checking file: {ex.Message} for {text.Path}");
        }
        
        var enhancedAnalyzer = new SaveObjectAnalyzer();
        
        Debug.WriteLine($"SOURCE GENERATOR: Calling analyzer for file: {text.Path}");
        var result = enhancedAnalyzer.AnalyzeAdditionalFile(text, cancellationToken).ToList();
        
        Debug.WriteLine($"SOURCE GENERATOR: AnalyzeFile returned {result.Count} analyses for {text.Path}");
        if (result.Count == 0)
        {
            Debug.WriteLine($"SOURCE GENERATOR: WARNING - No analyses found for {text.Path}!");
        }
        else
        {
            foreach (var analysis in result)
            {
                Debug.WriteLine($"SOURCE GENERATOR: Analysis found: {analysis.RootName} with {analysis.ClassDefinitions.Count} class definitions");
                
                // Log specific class names
                var classNames = analysis.ClassDefinitions.Keys.ToList();
                Debug.WriteLine($"SOURCE GENERATOR: Classes found in {analysis.RootName}: {string.Join(", ", classNames)}");
                
                if (analysis.Error != null)
                {
                    Debug.WriteLine($"SOURCE GENERATOR: ERROR in analysis: {analysis.Error.Message}");
                }
                
                // Log any diagnostics
                if (analysis.Diagnostics.Any())
                {
                    foreach (var diagnostic in analysis.Diagnostics)
                    {
                        Debug.WriteLine($"SOURCE GENERATOR: Diagnostic in {analysis.RootName}: [{diagnostic.Id}] {diagnostic.Title} - {diagnostic.Message}");
                    }
                }
            }
        }
        
        return result;
    }
}