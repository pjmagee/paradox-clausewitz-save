using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests;

[TestClass]
public class KeyValueArrayTests
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
    public void Strings()
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

        var expectedProperty = CSharpSyntaxTree.ParseText("public List<string>? Key { get; set; }")
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

        var modelClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Model");

        Assert.IsNotNull(modelClass, "Model class declaration not found");

        var properties = modelClass.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .SingleOrDefault(p => p.ToString().Equals(expectedProperty.ToString()));

        Assert.IsNotNull(properties, $"{expectedProperty.Identifier.ValueText} property not found");
        
        var bindMethod = modelClass
            .ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .SingleOrDefault(m => m.Identifier.ValueText.Equals("Bind"));
        
        Assert.IsNotNull(bindMethod, "Bind method not found");
        Assert.AreEqual(1, bindMethod.ParameterList.Parameters.Count, "Bind method has incorrect number of parameters");
        
        // Verify parameter type
        Assert.AreEqual("SaveObject?", bindMethod.ParameterList.Parameters[0].Type!.ToString(), "Bind method parameter type is incorrect");
        
        // Verify parameter name
        Assert.AreEqual(
            expected: "obj",
            actual: bindMethod.ParameterList.Parameters[0].Identifier.ValueText,
            message: "Bind method parameter name is incorrect"
        );
        
        // Verify null check and return statement
        Assert.AreEqual(
            expected: "if (obj == null) return null;",
            actual: bindMethod.Body!.Statements[0].ToString(),
            message: "Bind method null check is incorrect"
        ); 
        
        // Verify Model instance creation
        Assert.AreEqual(
            expected: "Model model = new Model();",
            actual: bindMethod.Body!.Statements[1].ToString(),
            message: "Bind method model creation is incorrect"
        );
        
        // Verify SourceObject assignment
        Assert.AreEqual(
            expected: "model.SourceObject = obj;",
            actual: bindMethod.Body!.Statements[2].ToString(),
            message: "Bind method SourceObject assignment is incorrect"
        );
        
        Assert.That.StatementsAreEqual(
            expected:
            """
            if (obj.TryGetSaveArray("key", out SaveArray keyArray) && keyArray != null)
                model.Key = new List<string>();
            """,
            actual: bindMethod.Body!.Statements[3].ToString(),
            message: "Bind method property assignment is incorrect"
        );
        
        // Populate list from array of scalar values
        Assert.That.StatementsAreEqual(
            expected: """
                        if (keyArray != null) 
                        { 
                          foreach (var item in keyArray.Items) 
                          { 
                              if (item is Scalar<string> scalarValue) 
                              { 
                                  model.Key.Add(scalarValue.Value); 
                              } 
                          } 
                        }
                      """,
            actual: bindMethod.Body!.Statements[4].ToString(),
            message: "Bind method list population is incorrect"
        );
        
        // Verify method return statement of Model instance
        Assert.AreEqual(
            expected: "return model;",
            actual: bindMethod.Body!.Statements.Last().ToString(),
            message: "Bind method return statement is incorrect"
        );
    }

    [TestMethod]
    [DataRow([new[]{ "yes", "no", "yes" }])]
    [DataRow([new[]{ "yes", "yes", "yes" }])]
    [DataRow([new[]{ "no", "no", "no" }])]
    public void Booleans(string[] values)
    {
        // Arrange
        var generator = new IncrementalGenerator();
         
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("key={");
        foreach (var value in values)
        {
            stringBuilder.AppendLine($" {value}");
        }
        stringBuilder.AppendLine("}");
        var csfText = stringBuilder.ToString();

        var expectedProperty = CSharpSyntaxTree.ParseText("public List<bool?>? Key { get; set; }")
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

        var modelClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Model");

        Assert.IsNotNull(modelClass, "Model class declaration not found");

        var properties = modelClass.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .SingleOrDefault(p => p.ToString().Equals(expectedProperty.ToString()));

        Assert.IsNotNull(properties, $"{expectedProperty.Identifier.ValueText} property not found");
        
        var bindMethod = modelClass
            .ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .SingleOrDefault(m => m.Identifier.ValueText.Equals("Bind"));
        
        Assert.IsNotNull(bindMethod, "Bind method not found");
        Assert.AreEqual(1, bindMethod.ParameterList.Parameters.Count, "Bind method has incorrect number of parameters");
        
        // Verify parameter type
        Assert.AreEqual("SaveObject?", bindMethod.ParameterList.Parameters[0].Type!.ToString(), "Bind method parameter type is incorrect");
        
        // Verify parameter name
        Assert.AreEqual(
            expected: "obj",
            actual: bindMethod.ParameterList.Parameters[0].Identifier.ValueText,
            message: "Bind method parameter name is incorrect"
        );
        
        // Verify null check and return statement
        Assert.AreEqual(
            expected: "if (obj == null) return null;",
            actual: bindMethod.Body!.Statements[0].ToString(),
            message: "Bind method null check is incorrect"
        ); 
        
        // Verify Model instance creation
        Assert.AreEqual(
            expected: "Model model = new Model();",
            actual: bindMethod.Body!.Statements[1].ToString(),
            message: "Bind method model creation is incorrect"
        );
        
        // Verify SourceObject assignment
        Assert.AreEqual(
            expected: "model.SourceObject = obj;",
            actual: bindMethod.Body!.Statements[2].ToString(),
            message: "Bind method SourceObject assignment is incorrect"
        );
        
        Assert.That.StatementsAreEqual(
            expected:
            """
            if (obj.TryGetSaveArray("key", out SaveArray keyArray) && keyArray != null)
                model.Key = new List<bool?>();
            """,
            actual: bindMethod.Body!.Statements[3].ToString(),
            message: "Bind method property assignment is incorrect"
        );
        
        // Populate list from array of scalar values
        Assert.That.StatementsAreEqual(
            expected: """
                        if (keyArray != null) 
                        { 
                          foreach (var item in keyArray.Items) 
                          { 
                              if (item is Scalar<bool> scalarValue) 
                              { 
                                  model.Key.Add(scalarValue.Value); 
                              } 
                          } 
                        }
                      """,
            actual: bindMethod.Body!.Statements[4].ToString(),
            message: "Bind method list population is incorrect"
        );
        
        // Verify method return statement of Model instance
        Assert.AreEqual(
            expected: "return model;",
            actual: bindMethod.Body!.Statements.Last().ToString(),
            message: "Bind method return statement is incorrect"
        );
    }

    [TestMethod]
    [DataRow([new[]{ 1.0f, 2.0f, 3.0f }])]
    [DataRow([new[]{ 10.0f, 20.0f, 30.0f, 40.0f }])]
    [DataRow([new[]{ 10.5f, 20.5f, 30.5f, 40.5f }])]
    [DataRow([new[]{ 1.12345f, 20.12345f, 300.12345f, 12345.12345f }])]
    public void Floats(float[] values)
    {
        // Arrange
        var generator = new IncrementalGenerator();

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("key={");
        foreach (var number in values)
        {
            stringBuilder.AppendLine($" {number:F1}");
        }
        stringBuilder.AppendLine("}");
        
        var csfText = stringBuilder.ToString();

        var expectedProperty = CSharpSyntaxTree.ParseText("public List<float?>? Key { get; set; }")
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

        var modelClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Model");

        Assert.IsNotNull(modelClass, "Model class declaration not found");

        var properties = modelClass.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .SingleOrDefault(p => p.ToString().Equals(expectedProperty.ToString()));

        Assert.IsNotNull(properties, $"{expectedProperty.Identifier.ValueText} property not found");
        
        var bindMethod = modelClass
            .ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .SingleOrDefault(m => m.Identifier.ValueText.Equals("Bind"));
        
        Assert.IsNotNull(bindMethod, "Bind method not found");
        Assert.AreEqual(1, bindMethod.ParameterList.Parameters.Count, "Bind method has incorrect number of parameters");
        
        // Verify parameter type
        Assert.AreEqual("SaveObject?", bindMethod.ParameterList.Parameters[0].Type!.ToString(), "Bind method parameter type is incorrect");
        
        // Verify parameter name
        Assert.AreEqual(
            expected: "obj",
            actual: bindMethod.ParameterList.Parameters[0].Identifier.ValueText,
            message: "Bind method parameter name is incorrect"
        );
        
        // Verify null check and return statement
        Assert.AreEqual(
            expected: "if (obj == null) return null;",
            actual: bindMethod.Body!.Statements[0].ToString(),
            message: "Bind method null check is incorrect"
        ); 
        
        // Verify Model instance creation
        Assert.AreEqual(
            expected: "Model model = new Model();",
            actual: bindMethod.Body!.Statements[1].ToString(),
            message: "Bind method model creation is incorrect"
        );
        
        // Verify SourceObject assignment
        Assert.AreEqual(
            expected: "model.SourceObject = obj;",
            actual: bindMethod.Body!.Statements[2].ToString(),
            message: "Bind method SourceObject assignment is incorrect"
        );
        
        Assert.That.StatementsAreEqual(
            expected:
            """
            if (obj.TryGetSaveArray("key", out SaveArray keyArray) && keyArray != null)
                model.Key = new List<float?>();
            """,
            actual: bindMethod.Body!.Statements[3].ToString(),
            message: "Bind method property assignment is incorrect"
        );
        
        // Populate list from array of scalar values
        Assert.That.StatementsAreEqual(
            expected: """
                        if (keyArray != null) 
                        { 
                          foreach (var item in keyArray.Items) 
                          { 
                              if (item is Scalar<float> scalarValue) 
                              { 
                                  model.Key.Add(scalarValue.Value); 
                              } 
                          } 
                        }
                      """,
            actual: bindMethod.Body!.Statements[4].ToString(),
            message: "Bind method list population is incorrect"
        );
        
        // Verify method return statement of Model instance
        Assert.AreEqual(
            expected: "return model;",
            actual: bindMethod.Body!.Statements.Last().ToString(),
            message: "Bind method return statement is incorrect"
        );
    }

    [TestMethod]
    [DataRow([new[]{ 0, 1, 2, 3 }])]
    [DataRow([new[]{ 1, 20, 300 }])]
    [DataRow([new[]{ int.MinValue, int.MaxValue / 2, int.MaxValue }])]
    public void Integers(int[] values)
    {
        // Arrange
        var generator = new IncrementalGenerator();
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("key={");
        foreach (var number in values)
        {
            stringBuilder.AppendLine($" {number}");
        }
        stringBuilder.AppendLine("}");
        
        var csfText = stringBuilder.ToString();

        var expectedProperty = CSharpSyntaxTree.ParseText("public List<int?>? Key { get; set; }")
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

        var modelClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Model");

        Assert.IsNotNull(modelClass, "Model class declaration not found");

        var properties = modelClass.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .SingleOrDefault(p => p.ToString().Equals(expectedProperty.ToString()));

        Assert.IsNotNull(properties, $"{expectedProperty.Identifier.ValueText} property not found");
        
        var bindMethod = modelClass
            .ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .SingleOrDefault(m => m.Identifier.ValueText.Equals("Bind"));
        
        Assert.IsNotNull(bindMethod, "Bind method not found");
        Assert.AreEqual(1, bindMethod.ParameterList.Parameters.Count, "Bind method has incorrect number of parameters");
        
        // Verify parameter type
        Assert.AreEqual("SaveObject?", bindMethod.ParameterList.Parameters[0].Type!.ToString(), "Bind method parameter type is incorrect");
        
        // Verify parameter name
        Assert.AreEqual(
            expected: "obj",
            actual: bindMethod.ParameterList.Parameters[0].Identifier.ValueText,
            message: "Bind method parameter name is incorrect"
        );
        
        // Verify null check and return statement
        Assert.AreEqual(
            expected: "if (obj == null) return null;",
            actual: bindMethod.Body!.Statements[0].ToString(),
            message: "Bind method null check is incorrect"
        ); 
        
        // Verify Model instance creation
        Assert.AreEqual(
            expected: "Model model = new Model();",
            actual: bindMethod.Body!.Statements[1].ToString(),
            message: "Bind method model creation is incorrect"
        );
        
        // Verify SourceObject assignment
        Assert.AreEqual(
            expected: "model.SourceObject = obj;",
            actual: bindMethod.Body!.Statements[2].ToString(),
            message: "Bind method SourceObject assignment is incorrect"
        );
        
        Assert.That.StatementsAreEqual(
            expected:
            """
            if (obj.TryGetSaveArray("key", out SaveArray keyArray) && keyArray != null)
                model.Key = new List<int?>();
            """,
            actual: bindMethod.Body!.Statements[3].ToString(),
            message: "Bind method property assignment is incorrect"
        );
        
        // Populate list from array of scalar values
        Assert.That.StatementsAreEqual(
            expected: """
                        if (keyArray != null) 
                        { 
                          foreach (var item in keyArray.Items) 
                          { 
                              if (item is Scalar<int> scalarValue) 
                              { 
                                  model.Key.Add(scalarValue.Value); 
                              } 
                          } 
                        }
                      """,
            actual: bindMethod.Body!.Statements[4].ToString(),
            message: "Bind method list population is incorrect"
        );
        
        // Verify method return statement of Model instance
        Assert.AreEqual(
            expected: "return model;",
            actual: bindMethod.Body!.Statements.Last().ToString(),
            message: "Bind method return statement is incorrect"
        );
    }

    [TestMethod]
    [Description("""
                 An array of objects is created based on repeated instances of the same key and its found properties.
                 Because optional properties are not found in all instances in the data, we must build up the object properties from all instances of the key.
                 This should create us a comprehensive class definition which can be used to map to all instances of the key in the data.
                 """
    )]
    public void Object()
    {
        // Arrange
        var generator = new IncrementalGenerator();
        
        var csfText = """
                      nested_object=
                      {
                          { }
                          { key_quoted_string="one" }
                          { key_integer=2 }
                          { key_float=3.25 }
                          { key_date="2023.01.01" }
                          { key_bool=yes }
                      }
                      """;

        var expectedProperty = CSharpSyntaxTree.ParseText("public List<Model.ModelNestedObjectItem?>? NestedObject { get; set; }")
            .GetRoot()
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Single();

        var nestedClassProperties = new List<string>
        {
            "public SaveObject? SourceObject { get; private set; }",
            "public string? KeyQuotedString { get; set; }",
            "public int? KeyInteger { get; set; }",
            "public float? KeyFloat { get; set; }",
            "public DateTime? KeyDate { get; set; }",
            "public bool? KeyBool { get; set; }"
        };

        var expectedNestedClass = CSharpSyntaxTree.ParseText($@"""
            public class ModelNestedObjectItem
            {{   
                {string.Join('\n', nestedClassProperties)}
            }}
            """)
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single();

        var expectedNestedClassProperties = nestedClassProperties
            .Select(p => CSharpSyntaxTree.ParseText(p).GetRoot().ChildNodes().OfType<PropertyDeclarationSyntax>().Single())
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
        var generatedTree = newCompilation.SyntaxTrees.FirstOrDefault(tree => tree.FilePath.Contains("Model.g.cs"));
        Assert.IsNotNull(generatedTree, "Generated Model class not found");

        TestContext.WriteLine($"Found generated model at: {generatedTree.FilePath}");
        TestContext.WriteLine(generatedTree.ToString());

        var generatedClass = generatedTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Model");

        Assert.IsNotNull(generatedClass, "Model class declaration not found");

        var generatedNestedClass = generatedClass.DescendantNodes().OfType<ClassDeclarationSyntax>().SingleOrDefault();
        Assert.IsNotNull(generatedNestedClass, "Nested class not found");
        Assert.AreEqual(expectedNestedClass.Identifier.Text, generatedNestedClass.Identifier.Text, "Nested class names do not match");

        foreach(var expectedNestedClassProperty in expectedNestedClassProperties)
        {
            var actualProperty = generatedNestedClass
                .ChildNodes()
                .OfType<PropertyDeclarationSyntax>()
                .SingleOrDefault(p => p.ToString().Equals(expectedNestedClassProperty.ToString()));

            Assert.IsNotNull(
                value: actualProperty,
                message: "Nested class property not found for " + expectedNestedClassProperty.Identifier.ValueText
            );  
        }

        Assert.AreEqual(
            expected: expectedNestedClassProperties.Count(),
            actual: generatedNestedClass.DescendantNodes().OfType<PropertyDeclarationSyntax>().Count(),
            message: "Nested class properties count does not match"
        );
        
    }
}