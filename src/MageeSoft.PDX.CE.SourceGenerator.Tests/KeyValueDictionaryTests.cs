using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests;

[TestClass]
// [Ignore] // Temporarily disabled
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

        // Look for nested model class first
        var modelClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ModelNestedObject");
        
        Assert.IsNotNull(modelClass, "ModelNestedObject class not found");

        // Find ClassDictionary property in the ModelNestedObject class
        var classDictionaryProperty = modelClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "ClassDictionary");
        
        Assert.IsNotNull(classDictionaryProperty, "ClassDictionary property not found");
        
        // Look for all classes in the model
        var allClasses = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Select(c => c.Identifier.ValueText)
            .ToList();
            
        TestContext.WriteLine($"Found classes: {string.Join(", ", allClasses)}");
        
        // Look for ModelNestedObjectClassDictionary class
        var classDictionaryClass = modelClass
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ModelNestedObjectClassDictionary");
            
        Assert.IsNotNull(classDictionaryClass, "ModelNestedObjectClassDictionary class not found");
        TestContext.WriteLine($"Found ModelNestedObjectClassDictionary class");
        
        // Check for numeric properties (_0, _1, etc.) in the ModelNestedObjectClassDictionary class
        var numericProps = classDictionaryClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Identifier.ValueText.StartsWith("_") && p.Identifier.ValueText.Length > 1 && char.IsDigit(p.Identifier.ValueText[1]))
            .ToList();
            
        TestContext.WriteLine($"Found {numericProps.Count} numeric properties starting with underscore: {string.Join(", ", numericProps.Select(p => p.Identifier.ValueText))}");
        TestContext.WriteLine($"Property types: {string.Join(", ", numericProps.Select(p => $"{p.Identifier.ValueText}: {p.Type}"))}");

        Assert.IsTrue(numericProps.Count >= 2, 
             $"Expected at least 2 numeric properties (_0, _1, etc.), but found {numericProps.Count}");
             
        // Check that at least some of the properties have integer types
        bool hasIntProps = numericProps.Any(p => p.Type.ToString() == "int");
        Assert.IsTrue(hasIntProps, "Expected at least some of the numeric properties to be of type int");
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

        // Look for nested model class first
        var modelClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ModelNestedObject");
        
        Assert.IsNotNull(modelClass, "ModelNestedObject class not found");

        // Find ClassDictionary property in the ModelNestedObject class
        var classDictionaryProperty = modelClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "ClassDictionary");
        
        Assert.IsNotNull(classDictionaryProperty, "ClassDictionary property not found");
        
        // Look for all classes in the model
        var allClasses = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Select(c => c.Identifier.ValueText)
            .ToList();
            
        TestContext.WriteLine($"Found classes: {string.Join(", ", allClasses)}");
        
        // Check for the specific ModelNestedObjectClassDictionary class
        var dictionaryClass = modelClass
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ModelNestedObjectClassDictionary");
            
        Assert.IsNotNull(dictionaryClass, "ModelNestedObjectClassDictionary class not found");
        TestContext.WriteLine($"Found ModelNestedObjectClassDictionary class");
        
        // Check for numeric properties (_0, _1, _2) inside the dictionary class
        var numericProperties = dictionaryClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Identifier.ValueText.StartsWith("_") && p.Identifier.ValueText.Length > 1 && char.IsDigit(p.Identifier.ValueText[1]))
            .ToList();
            
        TestContext.WriteLine($"Found {numericProperties.Count} numeric properties: {string.Join(", ", numericProperties.Select(p => p.Identifier.ValueText))}");
        Assert.IsTrue(numericProperties.Count >= 3, 
            $"Expected at least 3 numeric properties (_0, _1, _2), but found {numericProperties.Count}");
        
        // Log property types
        TestContext.WriteLine("Property types for numeric properties:");
        foreach (var prop in numericProperties)
        {
            string propName = prop.Identifier.ValueText;
            string actualType = prop.Type.ToString();
            TestContext.WriteLine($"  {propName}: {actualType}");
        }
        
        // Look for all properties to check for our expected ones
        var allProperties = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Select(p => p.Identifier.ValueText)
            .Distinct()
            .ToList();
            
        TestContext.WriteLine($"All properties in codebase: {string.Join(", ", allProperties)}");
        
        // Check for required properties
        Assert.IsTrue(allProperties.Contains("KeyInteger"), "KeyInteger property not found");
        Assert.IsTrue(allProperties.Contains("KeyQuotedString"), "KeyQuotedString property not found");
        Assert.IsTrue(allProperties.Contains("KeyUnquotedFalse"), "KeyUnquotedFalse property not found");
    }
}