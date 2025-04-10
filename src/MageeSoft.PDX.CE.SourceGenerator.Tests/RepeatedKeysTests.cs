using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests;

[TestClass]
public class RepeatedKeysTests
{
    readonly static AnalyzerConfigOptions ConfigOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>(
            new List<KeyValuePair<string, string>>([
                    new KeyValuePair<string, string>("build_property.PDXGenerateModels", "true")
                ]
            )
        )
    );
    
    const string SchemaFileName = "model.csf";
    const string ModelTestClass = 
        """
        using MageeSoft.PDX.CE;
        namespace MageeSoft.PDX.CE.SourceGenerator.Tests
        {
            [GameStateDocument("model.csf")] 
            public partial class Model 
            {
            }
        }
        """;

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [Description("Test for repeated keys with array values like asteroid_postfix={ \"413\" \"3254\" }")]
    public void RepeatedKeysWithArrayValues()
    {
        // Arrange
        var generator = new IncrementalGenerator();
        var csfText = """
                      test_record={
                          asteroid_postfix={ "413" "3254" }
                          asteroid_postfix={ "1287" "7291" }
                          asteroid_postfix={ "Alpha" "Beta" }
                          asteroid_postfix={ "Gamma" "Delta" }
                          name="Test System"
                          id=123
                      }
                      """;

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(Path.GetFullPath(SchemaFileName), csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(ConfigOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(RepeatedKeysTests),
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

        // List all syntax trees to see what's available
        TestContext.WriteLine("Available syntax trees:");
        foreach (var tree in newCompilation.SyntaxTrees)
        {
            TestContext.WriteLine($"Tree: {tree.FilePath}");
        }

        // Assert
        var modelTree = newCompilation.SyntaxTrees.FirstOrDefault(tree => tree.FilePath.Contains("Model.g.cs"));
        Assert.IsNotNull(modelTree, "Generated Model class not found - No Model.g.cs file was generated");
        
        TestContext.WriteLine($"Found generated model at: {modelTree.FilePath}");
        TestContext.WriteLine(modelTree.ToString());

        var modelClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Model");

        Assert.IsNotNull(modelClass, "Model class declaration not found");

        // Check for a ModelTestRecord class inside the Model
        var testRecordClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ModelTestRecord");

        Assert.IsNotNull(testRecordClass, "ModelTestRecord class not found");

        // Check for AsteroidPostfix property with the correct attributes
        var asteroidProperty = testRecordClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "AsteroidPostfix");

        Assert.IsNotNull(asteroidProperty, "AsteroidPostfix property not found in ModelTestRecord");
        // Check its type is List<List<string>>
        Assert.AreEqual("List<List<string>>?", asteroidProperty.Type.ToString(), 
            "AsteroidPostfix property should be of type List<List<string>>?");
    }

    [TestMethod]
    [Description("Test for repeated simple keys like trait=\"value1\" trait=\"value2\"")]
    public void RepeatedSimpleKeys()
    {
        // Arrange
        var generator = new IncrementalGenerator();
        var csfText = """
                      leader={
                         trait="brilliant"
                         trait="charismatic"
                         trait="resilient"
                         name="Admiral Picard"
                         id=42
                      }
                      """;

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(Path.GetFullPath(SchemaFileName), csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(ConfigOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(RepeatedKeysTests),
            syntaxTrees: [CSharpSyntaxTree.ParseText(ModelTestClass)],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Act
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out _);

        // Assert
        var modelTree = newCompilation.SyntaxTrees.FirstOrDefault(tree => tree.FilePath.Contains("Model.g.cs"));
        Assert.IsNotNull(modelTree, "Generated Model class not found");
        
        TestContext.WriteLine($"Found generated model at: {modelTree.FilePath}");
        TestContext.WriteLine(modelTree.ToString());

        var modelClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Model");

        Assert.IsNotNull(modelClass, "Model class declaration not found");

        // Check for a ModelLeader class inside the Model
        var leaderClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ModelLeader");

        Assert.IsNotNull(leaderClass, "ModelLeader class not found");

        // Check for Trait property with the correct attributes
        var traitProperty = leaderClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "Trait");

        Assert.IsNotNull(traitProperty, "Trait property not found in ModelLeader");
        // Check its type is List<string?>?
        Assert.AreEqual("List<string?>?", traitProperty.Type.ToString(), 
            "Trait property should be of type List<string?>?");
    }
} 