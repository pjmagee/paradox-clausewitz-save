using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests;

[TestClass]
public class KeyValueObjectTests
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
    A class definition is created based on repeated instances of the same key and its found properties.
    Because null or optional are not found in all instances in the data.
    This means we need an object analysis to determine the properties that are valid for a key and its associated schema/class definition.
    """)]
    public void DuplicatedKeyToCompleteClass()
    {
        // Arrange
        var generator = new IncrementalGenerator();
        
        /*
         * We should be able to build up a class definition based on repeated instances of the same key and its found properties.
         * Not all properties are found in all instances in the data, so we need to build up a class definition based on found properties across all instances.
         */
        var csfText = """
        nested_object={
            key_quoted_string="1"
        }
        nested_object={
            key_integer=1
        }
        nested_object={
            key_float=1.25
        }
        nested_object={
            key_date="2023.01.01"
        }
        nested_object={
            key_guid="00000000-0000-0000-0000-000000000000"
        }
        """;

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(Path.GetFullPath(SchemaFileName), csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(ConfigOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );
        
        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(KeyValueObjectTests),
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
        
        // Check for the class name based on the generator's naming pattern
        Assert.AreEqual(
            expected: "ModelNestedObjectItem",
            actual: generatedNestedClass.Identifier.Text,
            message: "Nested class names do not match"
        );
        
        // Check that all the expected properties exist
        var expectedProperties = new[] {
            "KeyQuotedString",
            "KeyInteger",
            "KeyFloat",
            "KeyDate",
            "KeyGuid"
        };
        
        foreach(var propertyName in expectedProperties)
        {
            var actualProperty = generatedNestedClass
                .ChildNodes()
                .OfType<PropertyDeclarationSyntax>()
                .SingleOrDefault(p => p.Identifier.ValueText == propertyName);

            Assert.IsNotNull(
                value: actualProperty,
                message: $"Property not found: {propertyName}"
            );
        }

        // Check for the list property
        var listProperty = generatedClass
            .ChildNodes()
            .OfType<PropertyDeclarationSyntax>()
            .SingleOrDefault(p => p.Identifier.ValueText == "NestedObject");
        
        Assert.IsNotNull(listProperty, "NestedObject property not found");
        
        // Check that it's a List of ModelNestedObjectItem
        var propertyType = listProperty.Type.ToString();
        Assert.IsTrue(
            propertyType.Contains("List<ModelNestedObjectItem") || 
            propertyType.Contains("List<Model.ModelNestedObjectItem"),
            $"Expected List<ModelNestedObjectItem> but got {propertyType}"
        );
    }
   

    [TestMethod]
    [Description("""
     Multiple inner nested objects are created.
     This is like a cascading of nested objects and we should end up with many inner defined scoped classes.
    """)]
    public void NestedObjects()
    {
        // Arrange
        var generator = new IncrementalGenerator();

        var csfText = """
        class_one={
            quoted_string="1"
            class_two={
                quoted_guid="00000000-0000-0000-0000-000000000000"
                class_three={
                    true=yes
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
            assemblyName: nameof(KeyValueObjectTests),
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

        // Verify the class hierarchy exists
        var modelClass = generatedTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Model");
            
        Assert.IsNotNull(modelClass, "Model class not found");
        
        // Verify Model has ClassOne property
        var classOneProperty = modelClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "ClassOne");
            
        Assert.IsNotNull(classOneProperty, "ClassOne property not found");
        
        // Verify ModelClassOne class exists
        var modelClassOne = generatedTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ModelClassOne");
            
        Assert.IsNotNull(modelClassOne, "ModelClassOne class not found");
        
        // Verify ModelClassOne has expected properties
        var expectedModelClassOneProps = new[] { "QuotedString", "ClassTwo" };
        foreach (var propName in expectedModelClassOneProps)
        {
            var prop = modelClassOne
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.ValueText == propName);
                
            Assert.IsNotNull(prop, $"{propName} property not found in ModelClassOne");
        }
        
        // Verify ModelClassOneClassTwo class exists and has properties
        var modelClassOneClassTwo = generatedTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ModelClassOneClassTwo");
            
        Assert.IsNotNull(modelClassOneClassTwo, "ModelClassOneClassTwo class not found");
        
        var expectedClassTwoProps = new[] { "QuotedGuid", "ClassThree" };
        foreach (var propName in expectedClassTwoProps)
        {
            var prop = modelClassOneClassTwo
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.ValueText == propName);
                
            Assert.IsNotNull(prop, $"{propName} property not found in ModelClassOneClassTwo");
        }
        
        // Verify the deepest class and its property
        var modelClassThree = generatedTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ModelClassOneClassTwoClassThree");
            
        Assert.IsNotNull(modelClassThree, "ModelClassOneClassTwoClassThree class not found");
        
        var trueProperty = modelClassThree
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "True");
            
        Assert.IsNotNull(trueProperty, "True property not found in the deepest class");
    }

    [TestMethod]
    [Description("""
    An empty structure is complicated by the fact that it's not clear whether it's an empty array or an empty object.
    Braces {} delimit nested structures (analogous to objects or arrays)​
    This is complicated by the fact that the empty structure is not a valid object.
    During schema extraction, keys with the same name are merged into a single property.
    This allows us to build up all the valid properties from all instances of the structure with the same key for a schema.
    """)]
    public void ObjectEmpty()
    {
        // Arrange
        var generator = new IncrementalGenerator();
        
        var csfText = """
        empty_structure=
        {
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
        Assert.AreEqual("ModelEmptyStructure", generatedNestedClass.Identifier.Text, "Nested class names do not match");
        
        // An empty structure should have no properties since it's empty
        // Just verify the EmptyStructure property exists in the main Model class
        var emptyStructureProperty = generatedClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "EmptyStructure");
        
        Assert.IsNotNull(emptyStructureProperty, "EmptyStructure property not found");
    }

    
    [TestMethod]    
    [Description("""
    Arrays of objects: You can combine these concepts: an array can contain object elements. 
    In the syntax, this looks like an outer { } with multiple inner { } blocks back-to-back.
    """)]
    public void ObjectArray()
    {
        // Arrange
        var generator = new IncrementalGenerator();
        
        var csfText = """
        player=
        {                          
            {
                name="Player 1"
                country=0
            }

            {
                name="Player 2"
                country=1
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

        // Verify the Player property exists and is a List - be flexible with naming
        var playerProperty = generatedClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "Player" || p.Identifier.ValueText == "Players");
        
        if (playerProperty == null)
        {
            // If we can't find Player or Players directly, dump all property names to help debug
            var allProperties = generatedClass
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Select(p => p.Identifier.ValueText)
                .ToList();
                
            TestContext.WriteLine($"Available properties in Model class: {string.Join(", ", allProperties)}");
            
            // As a fallback, look for any property that might be a List type
            playerProperty = generatedClass
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Type.ToString().Contains("List<"));
                
            Assert.IsNotNull(playerProperty, "No List property found that could represent Player");
            TestContext.WriteLine($"Found list property: {playerProperty.Identifier.ValueText}");
        }
        else
        {
            TestContext.WriteLine($"Found player property: {playerProperty.Identifier.ValueText}");
        }
        
        // Check that it's a list type
        var propertyType = playerProperty.Type.ToString();
        Assert.IsTrue(propertyType.Contains("List<"), $"Expected List<> type but got {propertyType}");
        
        // Extract the likely class name from the property type
        string itemClassName = "ModelPlayerItem"; // Default expectation
        if (propertyType.Contains("List<"))
        {
            int startIndex = propertyType.IndexOf("List<") + 5;
            int endIndex = propertyType.IndexOf(">", startIndex);
            if (endIndex > startIndex)
            {
                string typeName = propertyType.Substring(startIndex, endIndex - startIndex);
                // Clean up any nullable markers or namespace prefixes
                typeName = typeName.Replace("?", "").Replace("Model.", "");
                itemClassName = typeName;
                TestContext.WriteLine($"Extracted item class name: {itemClassName}");
            }
        }
        
        // Check for the nested item class using the extracted class name
        var playerItemClass = generatedTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == itemClassName);
            
        if (playerItemClass == null)
        {
            // If we can't find the exact class name, try to find any class that looks like a player item
            var allClasses = generatedTree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(c => c.Identifier.ValueText)
                .Where(name => name != "Model")
                .ToList();
                
            TestContext.WriteLine($"Available classes: {string.Join(", ", allClasses)}");
            
            // Try to find a class with Player or similar in the name
            var playerRelatedClass = generatedTree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => 
                   c.Identifier.ValueText.Contains("Player") || 
                   c.Identifier.ValueText.EndsWith("Item"));
                   
            Assert.IsNotNull(playerRelatedClass, "Could not find any class that might be related to Player items");
            playerItemClass = playerRelatedClass;
            TestContext.WriteLine($"Found player-related class: {playerItemClass.Identifier.ValueText}");
        }
        else
        {
            TestContext.WriteLine($"Found exact player item class: {playerItemClass.Identifier.ValueText}");
        }
        
        // Verify the expected properties exist in the class
        var expectedProperties = new[] { "Name", "Country" };
        foreach (var propName in expectedProperties)
        {
            var prop = playerItemClass
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.ValueText == propName);
                
            Assert.IsNotNull(prop, $"{propName} property not found in {playerItemClass.Identifier.ValueText}");
        }
    }

    [TestMethod]
    [Description("""
    A brace block that contains one or more key = value pairs is an object. 
    It's analogous to a JSON object or a struct - a set of named fields with values
    """)]
    public void ObjectScalars()
    {
        // A brace block that contains 1 or more key=value pairs is an object
        // Arrange
        var generator = new IncrementalGenerator();
        var csfText = """
                      nested_object={
                       quoted_string="value"
                       unquoted_string=value
                       unquoted_true=yes
                       unquoted_false=no
                       unquoted_integer=1
                       unquoted_float=0.12345
                       quoted_date="2023.01.01"
                       unquoted_date=2023.01.01
                       quoted_guid="00000000-0000-0000-0000-000000000000"
                      }                   
                      """;

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(Path.GetFullPath(SchemaFileName), csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(ConfigOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(KeyValueObjectTests),
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

        // Check for Model class
        var generatedClass = generatedTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Model");
        
        Assert.IsNotNull(generatedClass, "Model class declaration not found");

        // Check for ModelNestedObject class
        var nestedObjectClass = generatedTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ModelNestedObject");
            
        Assert.IsNotNull(nestedObjectClass, "ModelNestedObject class not found");
        
        // Check for NestedObject property in Model
        var nestedObjectProperty = generatedClass
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == "NestedObject");
            
        Assert.IsNotNull(nestedObjectProperty, "NestedObject property not found");
        
        // Check that all expected scalar properties exist in the ModelNestedObject class
        var expectedProperties = new[] {
            "QuotedString",
            "UnquotedString",
            "UnquotedTrue",
            "UnquotedFalse",
            "UnquotedInteger",
            "UnquotedFloat",
            "QuotedDate",
            "UnquotedDate",
            "QuotedGuid"
        };
        
        foreach (var propName in expectedProperties)
        {
            var prop = nestedObjectClass
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.ValueText == propName);
                
            Assert.IsNotNull(prop, $"{propName} property not found in ModelNestedObject");
        }
    }
}