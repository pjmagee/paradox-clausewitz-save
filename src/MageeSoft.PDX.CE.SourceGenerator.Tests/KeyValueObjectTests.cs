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

        var expectedProperty = CSharpSyntaxTree.ParseText("public List<Model.ModelNestedObject?>? NestedObjects { get; set; }")
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
            "public Guid? KeyGuid { get; set; }"
        };

        var expectedNestedClassProperties = nestedClassProperties.Select(p => CSharpSyntaxTree
            .ParseText(p).GetRoot()
            .ChildNodes().OfType<PropertyDeclarationSyntax>().Single()
        ).ToList(); 

        var expectedNestedClass = CSharpSyntaxTree.ParseText(
            $@"""
            public class ModelNestedObject
            {{   
                {string.Join('\n', expectedNestedClassProperties)}
            }}
            """
        ).GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

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

        var property = generatedClass
            .ChildNodes()
            .OfType<PropertyDeclarationSyntax>()
            .SingleOrDefault(p => p.ToString().Equals(expectedProperty.ToString()));
        
        Assert.IsNotNull(property, "Nested class property not found");        
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

        var classProperties = new Dictionary<string, List<string>>()
        {
            {
                "Model",
                new List<string>()
                {
                    "public SaveObject? SourceObject { get; private set; }",
                    "public Model.ModelClassOne? ClassOne { get; set; }"
                }
            },
            {
                "ModelClassOne",
                new List<string>()
                {
                    "public SaveObject? SourceObject { get; private set; }",
                    "public string? QuotedString { get; set; }",
                    "public Model.ModelClassOne.ModelClassOneClassTwo? ClassTwo { get; set; }"
                }
            },
            {
                "ModelClassOneClassTwo",
                new List<string>()
                {
                    "public SaveObject? SourceObject { get; private set; }",
                    "public Guid? QuotedGuid { get; set; }",
                    "public Model.ModelClassOne.ModelClassOneClassTwo.ModelClassOneClassTwoClassThree? ClassThree { get; set; }"
                }
            },
            {
                "ModelClassOneClassTwoClassThree",
                new List<string>()
                {
                    "public SaveObject? SourceObject { get; private set; }",
                    "public bool? ATrue { get; set; }"
                }
            }
        };

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

        foreach(var classProperty in classProperties)
        {
            var generatedClass = generatedTree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.ValueText == classProperty.Key);

            Assert.IsNotNull(generatedClass, $"Nested class {classProperty.Key} not found");

            var generatedNestedClassProperties = generatedClass
                .ChildNodes()
                .OfType<PropertyDeclarationSyntax>()
                .ToList();

            Assert.AreEqual(classProperty.Value.Count, generatedNestedClassProperties.Count, $"Nested class {classProperty.Key} properties count does not match");

            foreach(var expectedNestedClassProperty in classProperty.Value)
            {
                var actualProperty = generatedNestedClassProperties.SingleOrDefault(p => p.ToString().Equals(expectedNestedClassProperty));
                Assert.IsNotNull(actualProperty, $"Nested class property {expectedNestedClassProperty} not found for {classProperty.Key}");
            }

            Assert.AreEqual(
                expected: classProperty.Value.Count,
                actual: generatedNestedClassProperties.Count,
                message: $"Nested class {classProperty.Key} properties count does not match"
            );
        }
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

        var expectedProperty = CSharpSyntaxTree.ParseText("public Model.ModelEmptyStructure? EmptyStructure { get; set; }")
            .GetRoot()
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Single();

        var nestedClassProperties = new List<string>
        {
            "public SaveObject? SourceObject { get; private set; }"
        };
        
        var expectedNestedClassProperties = nestedClassProperties.Select(p => CSharpSyntaxTree
            .ParseText(p).GetRoot()
            .ChildNodes().OfType<PropertyDeclarationSyntax>().Single()
        ).ToList();

        var expectedNestedClass = CSharpSyntaxTree.ParseText(
            $@"""
            public class ModelEmptyStructure
            {{   
                {string.Join('\n', expectedNestedClassProperties)}
            }}
            """
        ).GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();    

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

        var property = generatedClass
            .ChildNodes()
            .OfType<PropertyDeclarationSyntax>()
            .SingleOrDefault(p => p.ToString().Equals(expectedProperty.ToString()));
        
        Assert.IsNotNull(property, "Nested class property not found");

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

        var expectedProperty = CSharpSyntaxTree.ParseText("public List<Model.ModelPlayerItem?>? Player { get; set; }")
            .GetRoot()
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Single();

        var nestedClassProperties = new List<string>
        {
            "public SaveObject? SourceObject { get; private set; }",
            "public string? Name { get; set; }",
            "public int? Country { get; set; }"
        };

        var expectedNestedClassProperties = nestedClassProperties.Select(p => CSharpSyntaxTree
            .ParseText(p).GetRoot()
            .ChildNodes().OfType<PropertyDeclarationSyntax>().Single()
        ).ToList();
        
        var expectedNestedClass = CSharpSyntaxTree.ParseText(
            $@"""
            public class ModelPlayerItem
            {{   
                {string.Join('\n', expectedNestedClassProperties)}
            }}
            """
        ).GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();    

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

        var property = generatedClass
            .ChildNodes()
            .OfType<PropertyDeclarationSyntax>()
            .SingleOrDefault(p => p.ToString().Equals(expectedProperty.ToString()));
        
        Assert.IsNotNull(property, "Nested class property not found");  
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
       
        var expectedProperty = CSharpSyntaxTree.ParseText("public Model.ModelNestedObject? NestedObject { get; set; }")
            .GetRoot()
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Single();

        var nestedClassProperties = new List<string>
        {
            "public SaveObject? SourceObject { get; private set; }",
            "public string? QuotedString { get; set; }",
            "public string? UnquotedString { get; set; }",
            "public bool? UnquotedTrue { get; set; }",
            "public bool? UnquotedFalse { get; set; }",
            "public int? UnquotedInteger { get; set; }",
            "public float? UnquotedFloat { get; set; }",
            "public DateTime? QuotedDate { get; set; }",
            "public string? UnquotedDate { get; set; }",
            "public Guid? QuotedGuid { get; set; }"
        };

        var expectedNestedClassProperties = nestedClassProperties.Select(p => CSharpSyntaxTree
            .ParseText(p).GetRoot()
            .ChildNodes().OfType<PropertyDeclarationSyntax>().Single()
        ).ToList();
        
        var expectedClass = CSharpSyntaxTree.ParseText(
            """
            public class ModelNestedObject
            {
                public NestedObject? Bind(SaveObject? obj)
                {
                    return null;
                }
            }
            """
        ).GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

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
        Assert.AreEqual(expectedClass.Identifier.Text, generatedNestedClass.Identifier.Text, "Nested class names do not match");

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

        var property = generatedClass
            .ChildNodes()
            .OfType<PropertyDeclarationSyntax>()
            .SingleOrDefault(p => p.ToString().Equals(expectedProperty.ToString()));
        
        Assert.IsNotNull(property, "Nested class property not found");
    }
}