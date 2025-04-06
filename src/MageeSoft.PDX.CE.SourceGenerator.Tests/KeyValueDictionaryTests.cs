using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests;

[TestClass]
public class KeyValueDictionaryTests
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
    [Description("""
    A dictionary with integer keys and integer values is created.
    """)]
    public void IntegerKeyIntegerValues()
    {
        // Arrange
        var generator = new IncrementalGenerator();
        var csfText = """
                      nested_object={
                        class_dictionary={
                          0=0
                          1=1
                          2=2
                          3=3
                          4=4
                          5=5
                          6=6
                          7=7
                        }
                      }                   
                      """;

        var expectedProperty = CSharpSyntaxTree.ParseText("public Dictionary<int, int?>? ClassDictionary { get; set; }")
            .GetRoot()
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Single();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(Path.GetFullPath(SchemaFileName), csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(ConfigOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(KeyValueArrayTests),
            syntaxTrees: [CSharpSyntaxTree.ParseText(ModelTestClass),],
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

        var actualProperty = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .SingleOrDefault(p => p.ToString().Equals(expectedProperty.ToString()));

        Assert.IsNotNull(actualProperty, "IntegerKeys property not found");

        Assert.AreEqual(expectedProperty.ToString(), actualProperty.ToString(), "Property does not match");
    }


    [TestMethod]
    [Description("""
    A dictionary with integer keys and object values is created.
    """)]
    public void IntegerKeyObjectValues()
    {
        // Arrange
        var generator = new IncrementalGenerator();
        var csfText = """
                      nested_object={
                        class_dictionary={
                          0={
                            key_integer=0
                          }
                          1={
                            key_quoted_string="1"
                          }
                          2={
                             key_unquoted_false=no
                          }
                        }
                      }                   
                      """;

        var expectedModelProperty = CSharpSyntaxTree.ParseText("public Dictionary<int, Model.ModelNestedObject.ModelNestedObjectClassDictionaryItem?>? ClassDictionary { get; set; }")
            .GetRoot()
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Single();

        // should be found across multiple instances in the integer key
        var nestedObjectItemProperties = new List<string>()
            {
                "public SaveObject? SourceObject { get; private set; }",
                "public int? KeyInteger { get; set; }",
                "public string? KeyQuotedString { get; set; }",
                "public bool? KeyUnquotedFalse { get; set; }"
            };

        var expectedNestedClassProperties = nestedObjectItemProperties
            .Select(p => CSharpSyntaxTree.ParseText(p).GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>().Single())
            .ToList();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(Path.GetFullPath(SchemaFileName), csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(ConfigOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(KeyValueArrayTests),
            syntaxTrees: [CSharpSyntaxTree.ParseText(ModelTestClass),],
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

        var actualProperty = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .SingleOrDefault(p => p.ToString().Equals(expectedModelProperty.ToString()));

        Assert.IsNotNull(actualProperty, "ClassDictionary property not found");
        Assert.AreEqual(expectedModelProperty.ToString(), actualProperty.ToString(), "Property does not match");

        var generatedNestedClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .SingleOrDefault(c => c.Identifier.ValueText.Equals("ModelNestedObjectClassDictionaryItem"));
        
        Assert.IsNotNull(generatedNestedClass, "Generated nested class not found");
        TestContext.WriteLine($"Found generated nested class at: {generatedNestedClass.Identifier.ValueText}");

        foreach (var nestedClassProperty in expectedNestedClassProperties)
        {
            var actualNestedProperty = generatedNestedClass
                .ChildNodes()
                .OfType<PropertyDeclarationSyntax>()
                .SingleOrDefault(p => p.ToString().Equals(nestedClassProperty.ToString()));

            Assert.IsNotNull(actualNestedProperty, $"{nestedClassProperty.Identifier.ValueText} property not found");
            Assert.AreEqual(nestedClassProperty.ToString(), actualNestedProperty.ToString(), "Property does not match");
        }
    }

}