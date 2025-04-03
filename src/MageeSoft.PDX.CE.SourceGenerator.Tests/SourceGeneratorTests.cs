using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.IO;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests;

[TestClass]
public class SourceGeneratorTests
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void SimpleKeyValues()
    {
        // Arrange

        var schemaFileName = "model.csf";
        var absoluteSchemaFilePath = Path.GetFullPath(schemaFileName);
        var generator = new IncrementalGenerator();
        var configOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>
            {
                {
                    "build_property.PDXGenerateModels", "true"
                }
            }
        );

        var modelTestClass = """
                             using MageeSoft.PDX.CE;
                             namespace MageeSoft.PDX.CE.SourceGenerator.Tests
                             {
                                 [GameStateDocument("model.csf")] 
                                 public partial class Model 
                                 {
                                 }
                             }
                             """;

        var csfText = """
                      one_identifier=value_as_identifier
                      two_string="value_as_string"
                      three_date="2100.01.02"
                      four_bool_true=yes
                      five_bool_false=no        
                      """;

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(absoluteSchemaFilePath, csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(configOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(SourceGeneratorTests),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(modelTestClass),
            ],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Act
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out _);

        // Assert
        var modelTree = newCompilation
            .SyntaxTrees
            .FirstOrDefault(tree => tree.FilePath.Contains("Model.g.cs") &&
                                    tree.ToString().Contains("partial class Model")
            );
        
        

        Assert.IsNotNull(modelTree, "Generated Model class not found");
        TestContext.WriteLine($"Found generated model at: {modelTree.FilePath}");
        TestContext.WriteLine(modelTree.ToString());

        var modelClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Model");

        Assert.IsNotNull(modelClass, "Model class declaration not found");

        var properties = modelClass.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();

        Assert.IsTrue(properties.Any(p => p.Identifier.ValueText == "OneIdentifier"), "OneIdentifier property not found");
        Assert.IsTrue(properties.Any(p => p.Identifier.ValueText == "TwoString"), "TwoString property not found");
        Assert.IsTrue(properties.Any(p => p.Identifier.ValueText == "ThreeDate"), "ThreeDate property not found");
        Assert.IsTrue(properties.Any(p => p.Identifier.ValueText == "FourBoolTrue"), "FourBoolTrue property not found");
        Assert.IsTrue(properties.Any(p => p.Identifier.ValueText == "FiveBoolFalse"), "FiveBoolFalse property not found");

        // Verify Bind method exists
        var bindMethod = modelClass.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "Bind");

        Assert.IsNotNull(bindMethod, "Bind method not found");
        Assert.IsTrue(bindMethod.ParameterList.Parameters.Count == 1, "Bind method should have one parameter");
        Assert.IsTrue(bindMethod.ParameterList.Parameters[0].Type!.ToString().Contains("SaveObject"), "Bind parameter should be SaveObject");

        // Verify the Bind method body contains the expected code
        var bindMethodBody = bindMethod.Body;
        Assert.IsNotNull(bindMethodBody, "Bind method body not found");

        // Should call TryGetBool for 'four_bool_true'
        Assert.IsTrue(bindMethodBody.ToString().Contains("TryGetBool(\"four_bool_true\""), "Bind method should call TryGetBool for 'four_bool_true'");

        // Should call TryGetBool for 'five_bool_false'
        Assert.IsTrue(bindMethodBody.ToString().Contains("TryGetBool(\"five_bool_false\""), "Bind method should call TryGetBool for 'five_bool_false'");


    }

    [TestMethod]
    public void SimpleScalarValues()
    {
        // Test scalar values (string, int, float, bool, date)
        var csfText = """
                      simple_string="simple string value"
                      simple_identifier=identifier_value
                      simple_int=42
                      simple_float=3.14
                      simple_date="2100.01.02"
                      simple_bool_true=yes
                      simple_bool_false=no
                      """;

        // Create a file path that will be used consistently
        var schemaFileName = "scalars.csf";
        var absoluteSchemaFilePath = Path.GetFullPath(schemaFileName);

        var generator = new IncrementalGenerator();
        var configOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>
            {
                {
                    "build_property.PDXGenerateModels", "true"
                }
            }
        );

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(absoluteSchemaFilePath, csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(configOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        // Using a simpler syntax to ensure the attribute argument is picked up
        var modelTestClass = @"
using MageeSoft.PDX.CE;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests
{
    [GameStateDocument(""scalars.csf"")] 
    public partial class Scalars 
    {
    
    }
}
";

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(SourceGeneratorTests),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(modelTestClass),
            ],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out _);

        // Find generated model syntax tree
        var modelTree = newCompilation.SyntaxTrees
            .FirstOrDefault(tree =>
                tree.ToString().Contains("// <auto-generated/>") &&
                tree.ToString().Contains("partial class Scalars")
            );

        Assert.IsNotNull(modelTree, "Generated Scalars class not found");

        // Parse the tree to find members
        var root = modelTree.GetRoot();
        var modelClass = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Scalars");

        Assert.IsNotNull(modelClass, "Scalars class declaration not found");

        // Verify scalar-specific property types
        var properties = modelClass.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();

        // String properties
        var stringProperty = properties.FirstOrDefault(p => p.Identifier.ValueText == "SimpleString");
        Assert.IsNotNull(stringProperty, "SimpleString property not found");
        Assert.IsTrue(stringProperty.Type.ToString().Contains("string"), "SimpleString should be of type string");

        var identifierProperty = properties.FirstOrDefault(p => p.Identifier.ValueText == "SimpleIdentifier");
        Assert.IsNotNull(identifierProperty, "SimpleIdentifier property not found");

        // Numeric properties
        var intProperty = properties.FirstOrDefault(p => p.Identifier.ValueText == "SimpleInt");
        Assert.IsNotNull(intProperty, "SimpleInt property not found");
        Assert.IsTrue(intProperty.Type.ToString().Contains("int"), "SimpleInt should be of type int");

        var floatProperty = properties.FirstOrDefault(p => p.Identifier.ValueText == "SimpleFloat");
        Assert.IsNotNull(floatProperty, "SimpleFloat property not found");
        Assert.IsTrue(floatProperty.Type.ToString().Contains("float"), "SimpleFloat should be of type float");

        // Date property
        var dateProperty = properties.FirstOrDefault(p => p.Identifier.ValueText == "SimpleDate");
        Assert.IsNotNull(dateProperty, "SimpleDate property not found");
        Assert.IsTrue(dateProperty.Type.ToString().Contains("DateTime"), "SimpleDate should be of type DateTime");

        // Boolean properties
        var boolTrueProperty = properties.FirstOrDefault(p => p.Identifier.ValueText == "SimpleBoolTrue");
        Assert.IsNotNull(boolTrueProperty, "SimpleBoolTrue property not found");
        Assert.IsTrue(boolTrueProperty.Type.ToString().Contains("bool"), "SimpleBoolTrue should be of type bool");

        var boolFalseProperty = properties.FirstOrDefault(p => p.Identifier.ValueText == "SimpleBoolFalse");
        Assert.IsNotNull(boolFalseProperty, "SimpleBoolFalse property not found");
        Assert.IsTrue(boolFalseProperty.Type.ToString().Contains("bool"), "SimpleBoolFalse should be of type bool");
    }

    [TestMethod]
    public void ListOfSimpleValues()
    {
        // Test list of simple scalar values
        var csfText = """
                      string_list={"item1" "item2" "item3"}
                      int_list={1 2 3 4 5}
                      float_list={1.1 2.2 3.3}
                      bool_list={yes no yes}
                      """;

        // Create a file path that will be used consistently
        var schemaFileName = "lists.csf";
        var absoluteSchemaFilePath = Path.GetFullPath(schemaFileName);

        var generator = new IncrementalGenerator();
        var configOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>
            {
                {
                    "build_property.PDXGenerateModels", "true"
                }
            }
        );

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(absoluteSchemaFilePath, csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(configOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        // Using a simpler syntax to ensure the attribute argument is picked up
        var modelTestClass = @"
using MageeSoft.PDX.CE;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests
{
    [GameStateDocument(""lists.csf"")] 
    public partial class Lists 
    {
    
    }
}
";

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(SourceGeneratorTests),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(modelTestClass),
            ],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out _);

        // Find generated model syntax tree
        var modelTree = newCompilation.SyntaxTrees
            .FirstOrDefault(tree =>
                tree.ToString().Contains("// <auto-generated/>") &&
                tree.ToString().Contains("partial class Lists")
            );

        Assert.IsNotNull(modelTree, "Generated Lists class not found");

        // Parse the tree to find members
        var root = modelTree.GetRoot();
        var modelClass = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Lists");

        Assert.IsNotNull(modelClass, "Lists class declaration not found");

        // Verify list-specific property types
        var properties = modelClass.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();

        // Each property should be a List<T> of the appropriate type
        var stringListProperty = properties.FirstOrDefault(p => p.Identifier.ValueText == "StringList");
        Assert.IsNotNull(stringListProperty, "StringList property not found");
        Assert.IsTrue(stringListProperty.Type.ToString().Contains("List<string>"), "StringList should be of type List<string>");

        var intListProperty = properties.FirstOrDefault(p => p.Identifier.ValueText == "IntList");
        Assert.IsNotNull(intListProperty, "IntList property not found");
        Assert.IsTrue(intListProperty.Type.ToString().Contains("List<int>"), "IntList should be of type List<int>");

        var floatListProperty = properties.FirstOrDefault(p => p.Identifier.ValueText == "FloatList");
        Assert.IsNotNull(floatListProperty, "FloatList property not found");
        Assert.IsTrue(floatListProperty.Type.ToString().Contains("List<float>"), "FloatList should be of type List<float>");

        var boolListProperty = properties.FirstOrDefault(p => p.Identifier.ValueText == "BoolList");
        Assert.IsNotNull(boolListProperty, "BoolList property not found");
        Assert.IsTrue(boolListProperty.Type.ToString().Contains("List<bool>"), "BoolList should be of type List<bool>");

        // Verify bind method has list-specific binding logic
        var bindMethod = modelClass.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "Bind");

        Assert.IsNotNull(bindMethod, "Bind method not found");

        // The bind method should reference TryGetSaveArray for lists
        var bindMethodText = bindMethod.ToString();
        Assert.IsTrue(bindMethodText.Contains("TryGetSaveArray"), "Bind method should use TryGetSaveArray for lists");
        Assert.IsTrue(bindMethodText.Contains("foreach"), "Bind method should use foreach to process list items");
    }

    [TestMethod]
    public void ListOfComplexObjects()
    {
        // Test list of complex objects
        var csfText = """
                      object_list={
                          { id=1 name="Object 1" value=10 }
                          { id=2 name="Object 2" value=20 }
                          { id=3 name="Object 3" value=30 }
                      }
                      """;

        // Create a file path that will be used consistently
        var schemaFileName = "complexlists.csf";
        var absoluteSchemaFilePath = Path.GetFullPath(schemaFileName);

        var generator = new IncrementalGenerator();
        var configOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>
            {
                {
                    "build_property.PDXGenerateModels", "true"
                }
            }
        );

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(absoluteSchemaFilePath, csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(configOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        // Using a simpler syntax to ensure the attribute argument is picked up
        var modelTestClass = @"
using MageeSoft.PDX.CE;
namespace MageeSoft.PDX.CE.SourceGenerator.Tests
{
    [GameStateDocument(""complexlists.csf"")] 
    public partial class ComplexLists 
    {
    
    }
}
";

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(SourceGeneratorTests),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(modelTestClass),
            ],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out _);

        // Find generated model syntax tree
        var modelTree = newCompilation.SyntaxTrees
            .FirstOrDefault(tree =>
                tree.ToString().Contains("// <auto-generated/>") &&
                tree.ToString().Contains("partial class ComplexLists")
            );

        Assert.IsNotNull(modelTree, "Generated ComplexLists class not found");

        // Parse the tree to find members
        var root = modelTree.GetRoot();
        var modelClass = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ComplexLists");

        Assert.IsNotNull(modelClass, "ComplexLists class declaration not found");

        // Find both ObjectList property and nested Object class
        var objectListProperty = modelClass.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "ObjectList");

        Assert.IsNotNull(objectListProperty, "ObjectList property not found");
        Assert.IsTrue(objectListProperty.Type.ToString().Contains("List<"),
            "ObjectList should be of type List<>"
        );

        // For complex object lists, a nested class should be defined
        var nestedClasses = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(c => c.Identifier.ValueText != "ComplexLists")
            .ToList();

        Assert.IsTrue(nestedClasses.Count > 0, "Should have at least one nested class for complex objects");

        // The bind method should use TryGetSaveArray and complex object binding
        var bindMethod = modelClass.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "Bind");

        Assert.IsNotNull(bindMethod, "Bind method not found");
        var bindMethodText = bindMethod.ToString();

        Assert.IsTrue(bindMethodText.Contains("TryGetSaveArray"),
            "Bind method should use TryGetSaveArray for lists"
        );

        Assert.IsTrue(bindMethodText.Contains("List<"),
            "Bind method should initialize a List<>"
        );
    }

    [TestMethod]
    public void StandardDictionary()
    {
        // Test standard dictionary (object with properties)
        var csfText = """
                      dictionary={
                          key1="value1"
                          key2="value2"
                          key3="value3"
                          numeric_key_1=100
                          numeric_key_2=200
                      }
                      """;

        // Create a file path that will be used consistently
        var schemaFileName = "dictionary.csf";
        var absoluteSchemaFilePath = Path.GetFullPath(schemaFileName);

        var generator = new IncrementalGenerator();
        var configOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>
            {
                {
                    "build_property.PDXGenerateModels", "true"
                }
            }
        );

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(absoluteSchemaFilePath, csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(configOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        // Using a simpler syntax to ensure the attribute argument is picked up
        var modelTestClass = @"
using MageeSoft.PDX.CE;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests
{
    [GameStateDocument(""dictionary.csf"")] 
    public partial class Model
    {
    
    }
}
";

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(SourceGeneratorTests),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(modelTestClass),
            ],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out _);

        // Find generated model syntax tree
        var modelTree = newCompilation.SyntaxTrees
            .FirstOrDefault(tree =>
                tree.ToString().Contains("// <auto-generated/>") &&
                tree.ToString().Contains("partial class Model")
            );

        Assert.IsNotNull(modelTree, "Generated Dictionary class not found");

        // Parse the tree to find members
        var root = modelTree.GetRoot();
        var modelClass = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .LastOrDefault(c => c.Identifier.ValueText == "Model");

        Assert.IsNotNull(modelClass, "Model class declaration not found");

        // Verify we have a dictionary property
        var dictionaryProperty = modelClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "Dictionary");

        Assert.IsNotNull(dictionaryProperty, "Dictionary property not found");
        Assert.IsTrue(dictionaryProperty.Type.ToString().Contains("Dictionary<"), "Dictionary property should be of type Dictionary<,>");

        // For standard dictionaries, the bind method should have logic to iterate through properties
        var bindMethod = modelClass.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "Bind");

        Assert.IsNotNull(bindMethod, "Bind method not found");
        var bindMethodText = bindMethod.ToString();

        Assert.IsTrue(bindMethodText.Contains("TryGetSaveObject"),
            "Bind method should use TryGetSaveObject for dictionary"
        );

        Assert.IsTrue(bindMethodText.Contains("Dictionary<"),
            "Bind method should initialize a Dictionary<,>"
        );
    }

    [TestMethod]
    public void PdxStyleDictionary()
    {
        // Test PDX-style dictionary (array of key-value pairs)
        var csfText = """
                      pdx_dict={
                          { "key1" "value1" }
                          { "key2" "value2" }
                          { "key3" "value3" }
                          { 1 100 }
                          { 2 200 }
                      }
                      """;

        // Create a file path that will be used consistently
        var schemaFileName = "pdxdict.csf";
        var absoluteSchemaFilePath = Path.GetFullPath(schemaFileName);

        var generator = new IncrementalGenerator();
        var configOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>
            {
                {
                    "build_property.PDXGenerateModels", "true"
                }
            }
        );

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(absoluteSchemaFilePath, csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(configOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        // Using a simpler syntax to ensure the attribute argument is picked up
        var modelTestClass = @"
using MageeSoft.PDX.CE;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests
{
    [GameStateDocument(""pdxdict.csf"")] 
    public partial class PdxDict 
    {
    
    }
}
";

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(SourceGeneratorTests),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(modelTestClass),
            ],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out _);

        // Find generated model syntax tree
        var modelTree = newCompilation.SyntaxTrees
            .FirstOrDefault(tree =>
                tree.ToString().Contains("// <auto-generated/>") &&
                tree.ToString().Contains("partial class PdxDict")
            );

        Assert.IsNotNull(modelTree, "Generated PdxDict class not found");

        // Parse the tree to find members
        var root = modelTree.GetRoot();
        var modelClass = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "PdxDict");

        Assert.IsNotNull(modelClass, "PdxDict class declaration not found");

        // For PDX dictionaries, we expect a Dictionary property
        var pdxDictProperty = modelClass.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "PdxDict");

        Assert.IsNotNull(pdxDictProperty, "PdxDict property not found");
        Assert.IsTrue(pdxDictProperty.Type.ToString().Contains("Dictionary<"),
            "PdxDict property should be of type Dictionary<,>"
        );

        // The bind method for PDX-style dictionaries should use TryGetSaveArray
        var bindMethod = modelClass.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "Bind");

        Assert.IsNotNull(bindMethod, "Bind method not found");
        var bindMethodText = bindMethod.ToString();

        // PDX dictionaries use arrays of key-value pairs
        Assert.IsTrue(bindMethodText.Contains("TryGetSaveArray"),
            "Bind method should use TryGetSaveArray for PDX-style dictionaries"
        );
    }

    [TestMethod]
    public void NestedObjects()
    {
        // Test nested complex objects
        var csfText = """
                      root_object={
                          id=1
                          name="Root"
                          child={
                              id=2
                              name="Child"
                              grandchild={
                                  id=3
                                  name="Grandchild"
                              }
                          }
                          sibling={
                              id=4
                              name="Sibling"
                          }
                      }
                      """;

        // Create a file path that will be used consistently
        var schemaFileName = "nested.csf";
        var absoluteSchemaFilePath = Path.GetFullPath(schemaFileName);

        var generator = new IncrementalGenerator();
        var configOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>
            {
                {
                    "build_property.PDXGenerateModels", "true"
                }
            }
        );

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(absoluteSchemaFilePath, csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(configOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        // Using a simpler syntax to ensure the attribute argument is picked up
        var modelTestClass = @"
using MageeSoft.PDX.CE;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests
{
    [GameStateDocument(""nested.csf"")] 
    public partial class Nested 
    {
    
    }
}
";

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(SourceGeneratorTests),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(modelTestClass),
            ],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out _);

        // Find generated model syntax tree
        var modelTree = newCompilation.SyntaxTrees
            .FirstOrDefault(tree =>
                tree.ToString().Contains("// <auto-generated/>") &&
                tree.ToString().Contains("partial class Nested")
            );

        Assert.IsNotNull(modelTree, "Generated Nested class not found");

        // Parse the tree to find members
        var root = modelTree.GetRoot();
        var modelClass = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Nested");

        Assert.IsNotNull(modelClass, "Nested class declaration not found");

        // Find nested class definitions
        var nestedClasses = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(c => c.Identifier.ValueText != "Nested")
            .ToList();

        // There should be at least 3 nested classes: Child, Grandchild, and Sibling
        Assert.IsTrue(nestedClasses.Count >= 3, $"Should have at least 3 nested classes, found {nestedClasses.Count}");

        // Verify specific nested class relationships
        var rootObjectProperty = modelClass.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "RootObject");

        Assert.IsNotNull(rootObjectProperty, "RootObject property not found");

        // For complex objects, the bind method should use TryGetSaveObject
        var bindMethod = modelClass.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "Bind");

        Assert.IsNotNull(bindMethod, "Bind method not found");
        var bindMethodText = bindMethod.ToString();
        Assert.IsTrue(bindMethodText.Contains("TryGetSaveObject"),
            "Bind method should use TryGetSaveObject for nested objects"
        );
    }
}