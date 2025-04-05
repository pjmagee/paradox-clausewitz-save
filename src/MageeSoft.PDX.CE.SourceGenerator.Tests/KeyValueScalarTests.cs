using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests;

[TestClass]
public class KeyValueIdObjectPairTests
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
    public void IdObjectPairs()
    {
        // Arrange
        var generator = new IncrementalGenerator();
        var csfText = """
                       nested_object={
                         {
                            123456
                            { key_string="Test Key" list_integers={ 1 2 3 } } 
                         }
                         {
                            123457
                            { key_string="Test Key 2" list_integers={ 4 5 6 } } 
                         }  
                       """;

        var expectedProperty = CSharpSyntaxTree.ParseText("public Dictionary<int, Model.NestedObjectItem?>? NestedObject { get; set; }");
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(Path.GetFullPath(SchemaFileName), csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(ConfigOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );
        
        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(KeyValueIdObjectPairTests),
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
        
        var modelClass = modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Model");
        
         
        
    }
}

[TestClass]
public class KeyValueScalarTests
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
    [DataRow("value")]
    [DataRow("value_with_underscores")]
    [Description("""
    These are character sequences (such as letters, digits, or underscores) that terminate at a boundary. 
    Typically, identifiers like country tags or culture names appear as standalone words. 
    Additionally, some special keywords and literals also belong in this group.
    """)]
    public void Unquoted_Strings(string value)
    {
        // Arrange
        var generator = new IncrementalGenerator();
        var csfText = $"""
                      key={value}                      
                      """;

        var expectedProperty = CSharpSyntaxTree.ParseText("public string? Key { get; set; }")
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
            assemblyName: nameof(KeyValueScalarTests),
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
        
        // Verify property assignment
        Assert.AreEqual(
            expected:
            "model.Key = obj.TryGetString(\"key\", out string keyStringValue) && keyStringValue != \"none\" ? keyStringValue : null;",
            actual: bindMethod.Body!.Statements[3].ToString(),
            message: "Bind method property assignment is incorrect"
        );
        
        // Verify method return statement of Model instance
        Assert.AreEqual(
            expected: "return model;",
            actual: bindMethod.Body!.Statements.Last().ToString(),
            message: "Bind method return statement is incorrect"
        );
    }
    
    [TestMethod]
    [DataRow("value")]
    [DataRow("value with spaces")]
    [DataRow("value with special characters: !@#$%^&*()")]
    [DataRow("a\\\"b")]
    [Description("""
    If a value needs to contain spaces, special characters, or purely be stored as a string literal, it can be wrapped in double quotes
    """)]
    public void Quoted_Strings(string value)
    {
        // Arrange
        var generator = new IncrementalGenerator();
        var csfText = $"""
                      key="{value}"                      
                      """;

        var expectedProperty = CSharpSyntaxTree.ParseText("public string? Key { get; set; }")
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
            assemblyName: nameof(KeyValueScalarTests),
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
        
        // Verify property assignment
        Assert.AreEqual(
            expected:
            "model.Key = obj.TryGetString(\"key\", out string keyStringValue) && keyStringValue != \"none\" ? keyStringValue : null;",
            actual: bindMethod.Body!.Statements[3].ToString(),
            message: "Bind method property assignment is incorrect"
        );
        
        // Verify method return statement of Model instance
        Assert.AreEqual(
            expected: "return model;",
            actual: bindMethod.Body!.Statements.Last().ToString(),
            message: "Bind method return statement is incorrect"
        );
    }

    [TestMethod]
    [DataRow("no")]
    [DataRow("yes")]
    [Description("""
    Clausewitz engine uses yes and no to represent boolean true/false values.
    """)]
    public void Unquoted_Booleans(string boolValue)
    {
        // Arrange
        var generator = new IncrementalGenerator();
        var csfText = $"""
                      key={boolValue}              
                      """;

        var expectedProperty = CSharpSyntaxTree.ParseText("public bool? Key { get; set; }")
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
            assemblyName: nameof(KeyValueScalarTests),
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
            expected: "if (obj.TryGetBool(\"key\", out bool keyValue)) model.Key = keyValue;",
            actual: bindMethod.Body!.Statements[3].ToString(),
            message: "Bind method property assignment is incorrect"
        );
        
        // Verify method return statement of Model instance
        Assert.AreEqual(
            expected: "return model;",
            actual: bindMethod.Body!.Statements.Last().ToString(),
            message: "Bind method return statement is incorrect"
        );
    }

    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(0)]
    [DataRow(int.MaxValue)]
    [Description("""
    The engine usually represents numbers as 32-bit signed integers, 64-bit unsigned integers, or 32-bit floats, depending on context.
    Some saves contain very large 64-bit unsigned integers; these must be preserved precisely to avoid any loss of accuracy.
    """)]
    public void Unquoted_Integers(int value)
    {
        // Arrange
        var generator = new IncrementalGenerator();
        var csfText = $"""
                      key={value}              
                      """;

        var expectedProperty = CSharpSyntaxTree.ParseText("public int? Key { get; set; }")
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
            assemblyName: nameof(KeyValueScalarTests),
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
            expected: "if (obj.TryGetInt(\"key\", out int keyValue)) model.Key = keyValue;",
            actual: bindMethod.Body!.Statements[3].ToString(),
            message: "Bind method property assignment is incorrect"
        );
        
        // Verify method return statement of Model instance
        Assert.AreEqual(
            expected: "return model;",
            actual: bindMethod.Body!.Statements.Last().ToString(),
            message: "Bind method return statement is incorrect"
        );
    }
    
    [TestMethod]
    [DataRow(long.MinValue)]
    [DataRow(long.MaxValue)]
    [Description("""
    The engine usually represents numbers as 32-bit signed integers, 64-bit unsigned integers, or 32-bit floats, depending on context.
    Some saves contain very large 64-bit unsigned integers; these must be preserved precisely to avoid any loss of accuracy.
    """)]
    public void Unquoted_Longs(long value)
    {
        // Arrange
        var generator = new IncrementalGenerator();

        var csfText = $"""
                       key={value}          
                       """;

        var expectedProperty = CSharpSyntaxTree.ParseText("public long? Key { get; set; }")
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
            assemblyName: nameof(KeyValueScalarTests),
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
            expected: "if (obj.TryGetLong(\"key\", out long keyValue)) model.Key = keyValue;",
            actual: bindMethod.Body!.Statements[3].ToString(),
            message: "Bind method property assignment is incorrect"
        );
        
        // Verify method return statement of Model instance
        Assert.AreEqual(
            expected: "return model;",
            actual: bindMethod.Body!.Statements.Last().ToString(),
            message: "Bind method return statement is incorrect"
        );
    }

    [TestMethod]
    [DataRow("1821.01.01")]
    [DataRow("1821.1.1")]
    [Description("""
    Dates are expressed numerically as YYYY.M.D (year.month.day) without quotes.
    While the game logic parses these as date objects, in the file itself they're simply unquoted strings composed of digits and periods
    """)]
    public void Quoted_Dates(string date)
    {
        // Arrange
        var generator = new IncrementalGenerator();

        var csfText = $"""
                       key="{date}"       
                       """;

        var expectedProperty = CSharpSyntaxTree.ParseText("public DateTime? Key { get; set; }")
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
            assemblyName: nameof(KeyValueScalarTests),
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
            expected: "if (obj.TryGetDateTime(\"key\", out DateTime keyValue)) model.Key = keyValue;",
            actual: bindMethod.Body!.Statements[3].ToString(),
            message: "Bind method property assignment is incorrect"
        );
        
        // Verify method return statement of Model instance
        Assert.AreEqual(
            expected: "return model;",
            actual: bindMethod.Body!.Statements.Last().ToString(),
            message: "Bind method return statement is incorrect"
        );
    }

    [TestMethod]
    // quoted GUID
    [DataRow("00000000-0000-0000-0000-000000000000")]
    [DataRow("12345678-1234-1234-1234-123456789abc")]
    [DataRow("87654321-4321-4321-4321-cba987654321")]
    public void Quoted_Guids(string guid)
    {
        // Arrange
        var generator = new IncrementalGenerator();

        var csfText = $"""
                       key="{guid}"       
                       """;

        var expectedProperty = CSharpSyntaxTree.ParseText("public Guid? Key { get; set; }")
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
            assemblyName: nameof(KeyValueScalarTests),
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
            expected: "if (obj.TryGetGuid(\"key\", out Guid keyValue)) model.Key = keyValue;",
            actual: bindMethod.Body!.Statements[3].ToString(),
            message: "Bind method property assignment is incorrect"
        );
        
        // Verify method return statement of Model instance
        Assert.AreEqual(
            expected: "return model;",
            actual: bindMethod.Body!.Statements.Last().ToString(),
            message: "Bind method return statement is incorrect"
        );
    }

    [TestMethod]
    [DataRow(0.5f)]
    [DataRow(0.25f)]
    [DataRow(1.25f)]
    [DataRow(10.25f)]
    [DataRow(100.25f)]
    [DataRow(float.MinValue)]
    [DataRow(float.MaxValue)]
    public void Unquoted_Floats(float value)
    {
        // Arrange
        var generator = new IncrementalGenerator();

        var csfText = $"""
                       key={value}      
                       """;

        var expectedProperty = CSharpSyntaxTree.ParseText("public float? Key { get; set; }")
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
            assemblyName: nameof(KeyValueScalarTests),
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
            expected: "if (obj.TryGetFloat(\"key\", out float keyValue)) model.Key = keyValue;",
            actual: bindMethod.Body!.Statements[3].ToString(),
            message: "Bind method property assignment is incorrect"
        );
        
        // Verify method return statement of Model instance
        Assert.AreEqual(
            expected: "return model;",
            actual: bindMethod.Body!.Statements.Last().ToString(),
            message: "Bind method return statement is incorrect"
        );
    }
}