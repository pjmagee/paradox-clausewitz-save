using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
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
                
                // This is a duplicate definition of GameStateDocumentAttribute to ensure proper resolution
                namespace MageeSoft.PDX.CE
                {
                    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
                    internal class GameStateDocumentAttribute : System.Attribute
                    {
                        public string SchemaFileName { get; }

                        public GameStateDocumentAttribute(string schemaFileName)
                        {
                            SchemaFileName = schemaFileName;
                        }
                    }
                }
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
                predicate: static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax cls && cls.AttributeLists.Count > 0,
                transform: static (ctx, ct) => GetAttributedClass(ctx, ct)
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // Register additional files for schema discovery 
        var additionalFiles = context.AdditionalTextsProvider;
        
        // Log additional files
        var filePathsPipeline = additionalFiles.Select((file, _) => file.Path);
        context.RegisterSourceOutput(filePathsPipeline, (ctx, path) => 
        {
            Debug.WriteLine($"SOURCE GENERATOR: Found additional file: {path}");
            ctx.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("PDXSG200", "Additional File", $"Found additional file: {path}", "StellarisCodeGenerator", DiagnosticSeverity.Info, true),
                null
            ));
        });

        // Create a provider that analyzes each additional file only when its content changes
        // This is the key improvement: we now track file content and only reanalyze when it changes
        var analyzedFiles = additionalFiles
            .Select((file, ct) => {
                // Get text to track content changes
                var text = file.GetText(ct);
                if (text == null) 
                    return (file.Path, Analyses: Array.Empty<SaveObjectAnalysis>());
                
                // Only analyze if file has content
                if (text.Length > 0) 
                {
                    Debug.WriteLine($"SOURCE GENERATOR: Analyzing file {file.Path} with length {text.Length}");
                    try 
                    {
                        var analyzer = new SaveObjectAnalyzer();
                        var analyses = analyzer.AnalyzeAdditionalFile(file, ct).ToArray();
                        Debug.WriteLine($"SOURCE GENERATOR: Completed analysis for {file.Path}, found {analyses.Length} results");
                        return (file.Path, Analyses: analyses);
                    }
                    catch (Exception ex) 
                    {
                        Debug.WriteLine($"SOURCE GENERATOR: Error analyzing {file.Path}: {ex.Message}");
                        return (file.Path, Analyses: Array.Empty<SaveObjectAnalysis>());
                    }
                }
                else 
                {
                    Debug.WriteLine($"SOURCE GENERATOR: File {file.Path} has no content, skipping analysis");
                    return (file.Path, Analyses: Array.Empty<SaveObjectAnalysis>());
                }
            })
            .WithTrackingName("Schema Analysis"); // Give it a name for debug tracking
            
        // Collect all analyses into a dictionary for lookup
        var analysisLookup = analyzedFiles
            .Collect()
            .Select((items, ct) => {
                var result = new Dictionary<string, SaveObjectAnalysis[]>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in items) 
                {
                    Debug.WriteLine($"SOURCE GENERATOR: Adding {item.Analyses.Length} analyses to lookup for {item.Path}");
                    result[item.Path] = item.Analyses;
                }
                return result;
            })
            .WithTrackingName("Analysis Dictionary");

        // Combine attributed classes with analyses and generation flag
        var generationInputs = attributedClasses
            .Combine(analysisLookup)
            .Combine(generateModels)
            .Where(pair => pair.Right) // Only process if generation is enabled
            .Select((pair, ct) => (AttributedClass: pair.Left.Left, AnalysisLookup: pair.Left.Right));

        // Generate code for each attributed class using cached analyses
        context.RegisterSourceOutput(generationInputs, (ctx, data) => 
        {
            var (attributedClass, analysisLookup) = data;
            
            Debug.WriteLine($"SOURCE GENERATOR: Processing attribution for {attributedClass.ClassName} with schema {attributedClass.SchemaFileName}");
            
            // Try to find matching schema file analyses
            var matchingFilePath = FindMatchingSchemaFilePath(attributedClass.SchemaFileName, analysisLookup.Keys.ToArray());
            
            if (string.IsNullOrEmpty(matchingFilePath) || !analysisLookup.TryGetValue(matchingFilePath, out var analyses) || analyses.Length == 0)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("PDXSG201", "Schema Analyses Not Found", 
                        $"No analyses found for schema file '{attributedClass.SchemaFileName}' for class {attributedClass.Namespace}.{attributedClass.ClassName}", 
                        "StellarisCodeGenerator", DiagnosticSeverity.Warning, true),
                    null
                ));
                return;
            }
            
            Debug.WriteLine($"SOURCE GENERATOR: Found {analyses.Length} analyses for {attributedClass.ClassName}");
            
            // Filter analyses for this class
            var filteredAnalyses = FilterAnalysesForAttributedClass(analyses, attributedClass).ToList();
            
            if (filteredAnalyses.Any())
            {
                Debug.WriteLine($"SOURCE GENERATOR: Generating code for {attributedClass.ClassName} with {filteredAnalyses.Count} analyses");
                // Create a dummy AdditionalText to satisfy the ModelsGenerator.GeneratePartialClass interface
                AdditionalText dummyFile = new CustomAdditionalText(matchingFilePath);
                ModelsGenerator.GeneratePartialClass(ctx, attributedClass, dummyFile, filteredAnalyses);
            }
            else
            {
                Debug.WriteLine($"SOURCE GENERATOR: No matching analyses after filtering for {attributedClass.ClassName}, trying all");
                // Fallback to all analyses
                AdditionalText dummyFile = new CustomAdditionalText(matchingFilePath);
                ModelsGenerator.GeneratePartialClass(ctx, attributedClass, dummyFile, analyses);
            }
        });
    }
    
    // Helper class to create a dummy AdditionalText
    private class CustomAdditionalText : AdditionalText
    {
        private readonly string _path;
        
        public CustomAdditionalText(string path)
        {
            _path = path;
        }
        
        public override string Path => _path;
        
        public override SourceText? GetText(CancellationToken cancellationToken = default)
        {
            return null; // We don't need the actual text since analysis is already done
        }
    }
    
    // Helper method to find matching schema file from a list of paths
    private static string FindMatchingSchemaFilePath(string schemaFileName, string[] availablePaths)
    {
        Debug.WriteLine($"SOURCE GENERATOR: Looking for schema file {schemaFileName} in {availablePaths.Length} paths");
        
        // Try exact match first
        var exactMatch = availablePaths.FirstOrDefault(p => 
            Path.GetFileName(p).Equals(schemaFileName, StringComparison.OrdinalIgnoreCase));
            
        if (exactMatch != null)
        {
            Debug.WriteLine($"SOURCE GENERATOR: Found exact match for {schemaFileName}: {exactMatch}");
            return exactMatch;
        }
        
        // Try base name match as fallback
        string baseSchemaName = Path.GetFileNameWithoutExtension(schemaFileName);
        var baseNameMatch = availablePaths.FirstOrDefault(p => 
            Path.GetFileNameWithoutExtension(p).Equals(baseSchemaName, StringComparison.OrdinalIgnoreCase));
            
        if (baseNameMatch != null)
        {
            Debug.WriteLine($"SOURCE GENERATOR: Found base name match for {schemaFileName}: {baseNameMatch}");
            return baseNameMatch;
        }
        
        Debug.WriteLine($"SOURCE GENERATOR: No match found for {schemaFileName}");
        return string.Empty;
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
            SchemaFileName = schemaFileName!
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