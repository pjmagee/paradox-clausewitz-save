using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace MageeSoft.PDX.CE.SourceGenerator;

[Generator]
public class StellarisCodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add diagnostic for initialization
        context.RegisterPostInitializationOutput(ctx => 
            ctx.AddSource("InitializationDiagnostic.g.cs", 
                """
                // Code generator initialized
                // This file is just a marker for debugging
                """
            )
        );

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
        
        var pdxAnalysis = additionalFiles.Select((file, cancellationToken) => 
        {
            Debug.WriteLine($"SOURCE GENERATOR: Analyzing file: {file.Path}");
            var result = (file, AnalyzeFile(file, cancellationToken));
            var analysisCount = result.Item2?.Count() ?? 0;
            Debug.WriteLine($"SOURCE GENERATOR: Analysis complete for {file.Path}, found {analysisCount} analyses");
            return result;
        });
        
        context.RegisterSourceOutput(pdxAnalysis, (ctx, data) => 
        {
            var file = data.Item1;
            var analyses = data.Item2;
            
            Debug.WriteLine($"SOURCE GENERATOR: Generating code for {file.Path}");
            ctx.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("PDXSG201", "Generating Code", $"Generating code for {file.Path} with {analyses?.Count() ?? 0} analyses", "StellarisCodeGenerator", DiagnosticSeverity.Info, true),
                null
            ));
            
            StellarisModelsGenerator.GenerateModels(ctx, file, analyses);
            
            Debug.WriteLine($"SOURCE GENERATOR: Code generation complete for {file.Path}");
        });
    }
    
    private static IEnumerable<SaveObjectAnalysis> AnalyzeFile(AdditionalText text, CancellationToken cancellationToken)
    {
        Debug.WriteLine($"SOURCE GENERATOR: AnalyzeFile called for {text.Path}");
        
        // Check if this is meta.pdx or gamestate.pdx and log more explicitly
        string fileName = Path.GetFileName(text.Path).ToLowerInvariant();

        if (fileName.Equals("meta"))
        {
            Debug.WriteLine($"SOURCE GENERATOR: Processing META file: {text.Path}");
        }
        else if (fileName.Equals("gamestate"))
        {
            Debug.WriteLine($"SOURCE GENERATOR: Processing GAMESTATE file: {text.Path}");
        }
        else
        {
            Debug.WriteLine($"SOURCE GENERATOR: Processing UNKNOWN file type: {text.Path}");
        }
        
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
        
        var enhancedAnalyzer = new EnhancedSaveObjectAnalyzer();
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
                if (analysis.Error != null)
                {
                    Debug.WriteLine($"SOURCE GENERATOR: ERROR in analysis: {analysis.Error.Message}");
                }
            }
        }
        
        return result;
    }
}
