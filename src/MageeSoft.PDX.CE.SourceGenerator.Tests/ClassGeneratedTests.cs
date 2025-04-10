using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests;

[TestClass]
// [Ignore] // Temporarily disabled
public class ClassGeneratedTests
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
    public void Test1()
    {
        // Arrange
        var generator = new IncrementalGenerator();
        var csfText = """
                      key={
                       "value1"
                       "value2"
                       "value3"
                      }                   
                      """;

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
    }
}