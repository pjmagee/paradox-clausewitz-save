using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests;

[TestClass]
public class ComplexStructureTests
{
    readonly static AnalyzerConfigOptions ConfigOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>(
            new List<KeyValuePair<string, string>>([
                    new KeyValuePair<string, string>("build_property.PDXGenerateModels", "true")
                ]
            )
        )
    );
    
    const string SchemaFileName = "complex.csf";
    const string ModelTestClass = 
        """
        using MageeSoft.PDX.CE;
        namespace MageeSoft.PDX.CE.SourceGenerator.Tests
        {
            [GameStateDocument("complex.csf")] 
            public partial class ComplexModel 
            {
            }
        }
        """;

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [Description("Test for deeply nested object structures to ensure they're properly captured without too_many or too_deep placeholders")]
    public void DeepNestedObjectStructures()
    {
        // Arrange - create a nested structure with many properties
        var generator = new IncrementalGenerator();
        
        // Build a CSF with nested objects several levels deep
        var csfBuilder = new StringBuilder();
        csfBuilder.AppendLine("galaxy={");
        
        // Add several nested sectors to reach depth
        for (int i = 1; i <= 10; i++)
        {
            csfBuilder.AppendLine($"  sector_{i}={{");
            
            // Add systems to each sector
            for (int j = 1; j <= 5; j++)
            {
                csfBuilder.AppendLine($"    system_{j}={{");
                
                // Add planets to each system
                for (int k = 1; k <= 3; k++)
                {
                    csfBuilder.AppendLine($"      planet_{k}={{");
                    
                    // Add a variety of properties to each planet
                    csfBuilder.AppendLine($"        name=\"Planet {i}-{j}-{k}\"");
                    csfBuilder.AppendLine($"        size={40 + k}");
                    csfBuilder.AppendLine($"        type=\"habitable_{k}\"");
                    csfBuilder.AppendLine($"        population={1000000 * k}");
                    csfBuilder.AppendLine("        resources={");
                    
                    // Add various resources
                    for (int r = 1; r <= 5; r++)
                    {
                        csfBuilder.AppendLine($"          resource_{r}={{");
                        csfBuilder.AppendLine($"            name=\"Resource {r}\"");
                        csfBuilder.AppendLine($"            amount={r * 10}");
                        csfBuilder.AppendLine($"            quality={r * 0.5}");
                        csfBuilder.AppendLine("          }");
                    }
                    
                    csfBuilder.AppendLine("        }");
                    csfBuilder.AppendLine("      }");
                }
                
                csfBuilder.AppendLine("    }");
            }
            
            csfBuilder.AppendLine("  }");
        }
        
        csfBuilder.AppendLine("}");
        
        string csfText = csfBuilder.ToString();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(Path.GetFullPath(SchemaFileName), csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(ConfigOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(ComplexStructureTests),
            syntaxTrees: [CSharpSyntaxTree.ParseText(ModelTestClass)],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Act
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        // Print diagnostics for debugging
        foreach (var diagnostic in diagnostics)
        {
            TestContext.WriteLine($"Diagnostic: {diagnostic}");
        }

        // Assert
        var modelTree = newCompilation.SyntaxTrees.FirstOrDefault(tree => tree.FilePath.Contains("ComplexModel.g.cs"));
        Assert.IsNotNull(modelTree, "Generated ComplexModel class not found - No ComplexModel.g.cs file was generated");
        
        TestContext.WriteLine($"Found generated model at: {modelTree.FilePath}");
        
        var modelClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ComplexModel");

        Assert.IsNotNull(modelClass, "ComplexModel class declaration not found");

        // Check for a ComplexModelGalaxy class
        var galaxyClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ComplexModelGalaxy");

        Assert.IsNotNull(galaxyClass, "ComplexModelGalaxy class not found");

        // Check specifically that we don't find "too_many" or "too_deep" properties
        var allProperties = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .ToList();
        
        // Ensure we have many properties
        Assert.IsTrue(allProperties.Count > 50, "Should have generated many properties");
        
        // Check for the absence of placeholder properties
        var tooManyProperty = allProperties.FirstOrDefault(p => p.Identifier.ValueText == "TooMany");
        var tooDeepProperty = allProperties.FirstOrDefault(p => p.Identifier.ValueText == "TooDeep");
        
        Assert.IsNull(tooManyProperty, "Should not find a TooMany placeholder property");
        Assert.IsNull(tooDeepProperty, "Should not find a TooDeep placeholder property");
        
        // Check for deep structure by looking for a specific planet
        var sector1Class = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText.Contains("Sector1"));
            
        Assert.IsNotNull(sector1Class, "Sector1 class not found");
        
        // Verify the deepest level was modeled properly by looking for resource properties
        var resourceClasses = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(c => c.Identifier.ValueText.Contains("Resource"))
            .ToList();
            
        Assert.IsTrue(resourceClasses.Count > 0, "Resource classes not found at the deepest level");
        
        // Verify resource properties exist and have the right types
        var resourceClass = resourceClasses.First();
        
        var nameProperty = resourceClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "Name");
            
        Assert.IsNotNull(nameProperty, "Name property not found in resource class");
        Assert.AreEqual("string?", nameProperty.Type.ToString(), "Name property should be of type string?");
        
        var amountProperty = resourceClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "Amount");
            
        Assert.IsNotNull(amountProperty, "Amount property not found in resource class");
        Assert.AreEqual("int?", amountProperty.Type.ToString(), "Amount property should be of type int?");
        
        var qualityProperty = resourceClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "Quality");
            
        Assert.IsNotNull(qualityProperty, "Quality property not found in resource class");
        Assert.AreEqual("float?", qualityProperty.Type.ToString(), "Quality property should be of type float?");
    }
    
    [TestMethod]
    [Description("Test for structures with large numbers of properties at the same level")]
    public void LargePropertyCount()
    {
        // Arrange - create a structure with many properties at the same level
        var generator = new IncrementalGenerator();
        
        // Build a CSF with hundreds of properties at the same level
        var csfBuilder = new StringBuilder();
        csfBuilder.AppendLine("star_database={");
        
        // Create 300+ properties in a single object
        for (int i = 1; i <= 300; i++)
        {
            csfBuilder.AppendLine($"  star_{i}={{");
            csfBuilder.AppendLine($"    name=\"Star {i}\"");
            csfBuilder.AppendLine($"    luminosity={i * 0.5}");
            csfBuilder.AppendLine($"    mass={i * 0.8}");
            csfBuilder.AppendLine($"    temperature={1000 + (i * 100)}");
            csfBuilder.AppendLine($"    radius={i * 0.3}");
            csfBuilder.AppendLine("  }");
        }
        
        csfBuilder.AppendLine("}");
        
        string csfText = csfBuilder.ToString();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(Path.GetFullPath(SchemaFileName), csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(ConfigOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(ComplexStructureTests) + "_LargeCount",
            syntaxTrees: [CSharpSyntaxTree.ParseText(ModelTestClass)],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Act
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        // Assert
        var modelTree = newCompilation.SyntaxTrees.FirstOrDefault(tree => tree.FilePath.Contains("ComplexModel.g.cs"));
        Assert.IsNotNull(modelTree, "Generated ComplexModel class not found");
        
        var databaseClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ComplexModelStarDatabase");

        Assert.IsNotNull(databaseClass, "ComplexModelStarDatabase class not found");
        
        // Count the properties to ensure we didn't hit the property limit
        var allStarProperties = databaseClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .ToList();
        
        // We should have approximately 300 star properties
        Assert.IsTrue(allStarProperties.Count >= 200, 
            $"Expected at least 200 star properties, got {allStarProperties.Count}");
        
        // Check that there are no placeholder properties
        var tooManyProperty = databaseClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "TooMany");
            
        Assert.IsNull(tooManyProperty, "Should not find a TooMany placeholder property");
    }
} 