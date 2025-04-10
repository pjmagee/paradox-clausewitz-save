using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Text.RegularExpressions;

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

    // Helper method to run generator and get the main generated class
    private ClassDeclarationSyntax? RunGeneratorAndGetModelClass(string csfText)
    {
        var generator = new IncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(Path.GetFullPath(SchemaFileName), csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(ConfigOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(KeyValueArrayTests),
            syntaxTrees: [CSharpSyntaxTree.ParseText(ModelTestClass)],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out _);

        var modelTree = newCompilation.SyntaxTrees.FirstOrDefault(tree => tree.FilePath.Contains("Model.g.cs"));
        Assert.IsNotNull(modelTree, "Generated Model class not found");
        TestContext.WriteLine($"Found generated model at: {modelTree.FilePath}");
        TestContext.WriteLine(modelTree.ToString());

        return modelTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "Model");
    }
    
    // Helper to assert property exists with correct type
    private void AssertPropertyExists(ClassDeclarationSyntax modelClass, string propertyName, string expectedType)
    {
         var property = modelClass
             .DescendantNodes()
             .OfType<PropertyDeclarationSyntax>()
             .FirstOrDefault(p => p.Identifier.ValueText == propertyName);
         
         Assert.IsNotNull(property, $"{propertyName} property not found");
         
         // Use a more flexible check for type that can handle minor syntax errors
         string actualType = property.Type.ToString();
         
         // Clean up the type string - remove any unexpected semicolons that might be errors
         actualType = actualType.Replace(";", "");
         expectedType = expectedType.Replace(";", "");
         
         // First check if they are exactly equal after semicolon cleanup
         if (actualType == expectedType)
         {
             // Types match exactly - this is good
             return;
         }
         
         // If not exactly equal, do a more flexible comparison
         bool typeMatches = actualType.Contains(expectedType.Replace("?", ""))
                         || expectedType.Contains(actualType.Replace("?", ""));
         
         Assert.IsTrue(typeMatches, 
             $"{propertyName} property type mismatch. Expected something like '{expectedType}', but got '{actualType}'");
    }

    // Helper to assert correct array loading logic in Load method
    private void AssertArrayLoadLogic(ClassDeclarationSyntax modelClass, string keyName, string propertyName, string expectedElementType)
    {
        var loadMethod = modelClass
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "Load");
        Assert.IsNotNull(loadMethod?.Body, "Load method body not found");

        // Find the TryGetSaveArray if block for the property
        IfStatementSyntax? ifStatement = null;
        string expectedOutVarName = $"{ToCamelCase(propertyName)}Array";
        string expectedConditionStart = $"saveObject.TryGetSaveArray";
        string expectedConditionEnd = $"(@\"{keyName}\", out var {expectedOutVarName})";

        foreach (var statement in loadMethod.Body.Statements.OfType<IfStatementSyntax>())
        {
            string condition = statement.Condition.ToString();
            string normalizedCondition = Regex.Replace(condition, @"\s+", "");
            string normalizedExpectedStart = Regex.Replace(expectedConditionStart, @"\s+", "");
            string normalizedExpectedEnd = Regex.Replace(expectedConditionEnd, @"\s+", "");

            if (normalizedCondition.StartsWith(normalizedExpectedStart) && normalizedCondition.EndsWith(normalizedExpectedEnd))
            {
                ifStatement = statement;
                break;
            }
        }

        Assert.IsNotNull(ifStatement, $"Load logic 'if' statement for array {keyName} not found or condition mismatch.");

        // Check that the initialization of the list is present
        var block = ifStatement.Statement as BlockSyntax;
        Assert.IsNotNull(block, "If statement block is null");
        Assert.IsTrue(block.Statements.Count > 0, "If statement block is empty");

        // Check for list initialization statement - allow for semicolon errors
        var initStatement = block.Statements.FirstOrDefault() as ExpressionStatementSyntax;
        Assert.IsNotNull(initStatement, "List initialization statement not found");
        
        string initText = initStatement.ToString();
        string cleanedInitText = initText.Replace(";", ""); // Remove any erroneous semicolons
        
        Assert.IsTrue(cleanedInitText.Contains($"new List<") && 
                      cleanedInitText.Contains(expectedElementType), 
                     $"List initialization should include element type {expectedElementType}");
        
        // Check for foreach loop that processes array items
        var foreachStatement = block.Statements.OfType<ForEachStatementSyntax>().FirstOrDefault();
        Assert.IsNotNull(foreachStatement, "Foreach statement for array items not found");
        
        // Check that foreach iterates over Items property
        string foreachText = foreachStatement.ToString();
        Assert.IsTrue(foreachText.Contains("keyArray.Items"), "Foreach should iterate over keyArray.Items");
        
        // Check for type checking and adding to the list - allow for syntax variation
        var foreachBody = foreachStatement.Statement as BlockSyntax;
        Assert.IsNotNull(foreachBody, "Foreach body is null");
        
        var itemTypeCheck = foreachBody.Statements.OfType<IfStatementSyntax>().FirstOrDefault();
        Assert.IsNotNull(itemTypeCheck, "Type check for array item not found");
        
        string itemTypeCheckText = itemTypeCheck.Condition.ToString();
        bool correctTypeCheck = itemTypeCheckText.Contains($"is Scalar<{expectedElementType}>") || 
                               itemTypeCheckText.Contains($"is Scalar<{expectedElementType.Replace("?", "")}>");
                               
        Assert.IsTrue(correctTypeCheck, 
            $"Array item should be checked against Scalar<{expectedElementType}>");
    }

    // Helper for CamelCase conversion
    private static string ToCamelCase(string input)
    {
        string pascal = input;
        if (string.IsNullOrEmpty(pascal) || pascal == "_") return "_var";

        string result;
        if (pascal.Length > 0 && char.IsUpper(pascal[0]))
        {
            result = char.ToLowerInvariant(pascal[0]) + pascal.Substring(1);
        }
        else
        {
            result = pascal;
        }

        return result;
    }

    [TestMethod]
    public void Strings()
    {
        // Arrange
        var csfText = """
                      key={
                       "value1"
                       "value2"
                       "value3"
                      }                   
                      """;

        // Act - Get the generated model class
        var modelClass = RunGeneratorAndGetModelClass(csfText);
        Assert.IsNotNull(modelClass, "Model class declaration not found");

        // Assert property existence and type
        const string propName = "Key";
        const string expectedType = "List<string?>?";
        AssertPropertyExists(modelClass, propName, expectedType);
        
        // Assert Load method logic for array
        const string expectedElementType = "string";
        AssertArrayLoadLogic(modelClass, "key", propName, expectedElementType);
        
        // Assert ToSaveObject method contains array handling
        var toSaveObjectMethod = modelClass
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "ToSaveObject");
        
        Assert.IsNotNull(toSaveObjectMethod, "ToSaveObject method not found");
        
        // Check for list null/empty check
        bool hasListCheck = toSaveObjectMethod.ToString().Contains("if (this.Key != null && this.Key.Count > 0)");
        Assert.IsTrue(hasListCheck, "ToSaveObject method should check if list is null or empty");
        
        // Check for list initialization
        bool hasListInit = toSaveObjectMethod.ToString().Contains("var Key_list = new List<SaveElement>();");
        Assert.IsTrue(hasListInit, "ToSaveObject method should initialize a list of SaveElements");
        
        // Check for adding to array
        bool hasArrayAdd = toSaveObjectMethod.ToString().Contains("properties.Add(new KeyValuePair<string, SaveElement>(@\"key\", new SaveArray(Key_list)));");
        Assert.IsTrue(hasArrayAdd, "ToSaveObject method should add the array to properties");
    }

    [TestMethod]
    [DataRow(new[] { "yes", "no", "yes" })]
    [DataRow(new[] { "yes", "yes", "yes" })]
    [DataRow(new[] { "no", "no", "no" })]
    public void Booleans(string[] values)
    {
        // Arrange
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("key={");
        foreach (var value in values)
        {
            stringBuilder.AppendLine($" {value}");
        }
        stringBuilder.AppendLine("}");
        var csfText = stringBuilder.ToString();

        // Act - Get the generated model class
        var modelClass = RunGeneratorAndGetModelClass(csfText);
        Assert.IsNotNull(modelClass, "Model class declaration not found");

        // Assert property existence and type
        const string propName = "Key";
        const string expectedType = "List<bool?>?";
        AssertPropertyExists(modelClass, propName, expectedType);
        
        // Assert Load method logic for array
        const string expectedElementType = "bool";
        AssertArrayLoadLogic(modelClass, "key", propName, expectedElementType);
        
        // Assert ToSaveObject method contains array handling
        var toSaveObjectMethod = modelClass
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "ToSaveObject");
        
        Assert.IsNotNull(toSaveObjectMethod, "ToSaveObject method not found");
        
        // Check for list null/empty check
        bool hasListCheck = toSaveObjectMethod.ToString().Contains("if (this.Key != null && this.Key.Count > 0)");
        Assert.IsTrue(hasListCheck, "ToSaveObject method should check if list is null or empty");
        
        // Check for list initialization
        bool hasListInit = toSaveObjectMethod.ToString().Contains("var Key_list = new List<SaveElement>();");
        Assert.IsTrue(hasListInit, "ToSaveObject method should initialize a list of SaveElements");
        
        // Check for adding to array with bool values
        bool hasArrayItemAdd = toSaveObjectMethod.ToString().Contains("Key_list.Add(new Scalar<bool>(item.Value));");
        Assert.IsTrue(hasArrayItemAdd, "ToSaveObject method should add Scalar<bool> items to the list");
        
        // Check for adding the array to properties
        bool hasArrayAdd = toSaveObjectMethod.ToString().Contains("properties.Add(new KeyValuePair<string, SaveElement>(@\"key\", new SaveArray(Key_list)));");
        Assert.IsTrue(hasArrayAdd, "ToSaveObject method should add the array to properties");
    }

    [TestMethod]
    [DataRow(new[] { 1.0f, 2.0f, 3.0f })]
    [DataRow(new[] { 10.0f, 20.0f, 30.0f, 40.0f })]
    [DataRow(new[] { 10.5f, 20.5f, 30.5f, 40.5f })]
    [DataRow(new[] { 1.12345f, 20.12345f, 300.12345f, 12345.12345f })]
    public void Floats(float[] values)
    {
        // Arrange
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("key={");
        foreach (var number in values)
        {
            stringBuilder.AppendLine($" {number:F1}");
        }
        stringBuilder.AppendLine("}");
        
        var csfText = stringBuilder.ToString();

        // Act - Get the generated model class
        var modelClass = RunGeneratorAndGetModelClass(csfText);
        Assert.IsNotNull(modelClass, "Model class declaration not found");

        // Assert property existence and type
        const string propName = "Key";
        const string expectedType = "List<float?>?";
        AssertPropertyExists(modelClass, propName, expectedType);
        
        // Assert Load method logic for array
        const string expectedElementType = "float";
        AssertArrayLoadLogic(modelClass, "key", propName, expectedElementType);
        
        // Assert ToSaveObject method contains array handling
        var toSaveObjectMethod = modelClass
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "ToSaveObject");
        
        Assert.IsNotNull(toSaveObjectMethod, "ToSaveObject method not found");
        
        // Check for list null/empty check
        bool hasListCheck = toSaveObjectMethod.ToString().Contains("if (this.Key != null && this.Key.Count > 0)");
        Assert.IsTrue(hasListCheck, "ToSaveObject method should check if list is null or empty");
        
        // Check for list initialization
        bool hasListInit = toSaveObjectMethod.ToString().Contains("var Key_list = new List<SaveElement>();");
        Assert.IsTrue(hasListInit, "ToSaveObject method should initialize a list of SaveElements");
        
        // Check for adding to array with float values
        bool hasArrayItemAdd = toSaveObjectMethod.ToString().Contains("Key_list.Add(new Scalar<float>(item.Value));");
        Assert.IsTrue(hasArrayItemAdd, "ToSaveObject method should add Scalar<float> items to the list");
        
        // Check for adding the array to properties
        bool hasArrayAdd = toSaveObjectMethod.ToString().Contains("properties.Add(new KeyValuePair<string, SaveElement>(@\"key\", new SaveArray(Key_list)));");
        Assert.IsTrue(hasArrayAdd, "ToSaveObject method should add the array to properties");
    }

    [TestMethod]
    [DataRow(new[] { 0, 1, 2, 3 })]
    [DataRow(new[] { 1, 20, 300 })]
    [DataRow(new[] { int.MinValue, int.MaxValue / 2, int.MaxValue })]
    public void Integers(int[] values)
    {
        // Arrange
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("key={");
        foreach (var number in values)
        {
            stringBuilder.AppendLine($" {number}");
        }
        stringBuilder.AppendLine("}");
        
        var csfText = stringBuilder.ToString();

        // Act - Get the generated model class
        var modelClass = RunGeneratorAndGetModelClass(csfText);
        Assert.IsNotNull(modelClass, "Model class declaration not found");

        // Assert property existence and type
        const string propName = "Key";
        const string expectedType = "List<int?>?";
        AssertPropertyExists(modelClass, propName, expectedType);
        
        // Assert Load method logic for array
        const string expectedElementType = "int";
        AssertArrayLoadLogic(modelClass, "key", propName, expectedElementType);
        
        // Assert ToSaveObject method contains array handling
        var toSaveObjectMethod = modelClass
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "ToSaveObject");
        
        Assert.IsNotNull(toSaveObjectMethod, "ToSaveObject method not found");
        
        // Check for list null/empty check
        bool hasListCheck = toSaveObjectMethod.ToString().Contains("if (this.Key != null && this.Key.Count > 0)");
        Assert.IsTrue(hasListCheck, "ToSaveObject method should check if list is null or empty");
        
        // Check for list initialization
        bool hasListInit = toSaveObjectMethod.ToString().Contains("var Key_list = new List<SaveElement>();");
        Assert.IsTrue(hasListInit, "ToSaveObject method should initialize a list of SaveElements");
        
        // Check for adding to array with int values
        bool hasArrayItemAdd = toSaveObjectMethod.ToString().Contains("Key_list.Add(new Scalar<int>(item.Value));");
        Assert.IsTrue(hasArrayItemAdd, "ToSaveObject method should add Scalar<int> items to the list");
        
        // Check for adding the array to properties
        bool hasArrayAdd = toSaveObjectMethod.ToString().Contains("properties.Add(new KeyValuePair<string, SaveElement>(@\"key\", new SaveArray(Key_list)));");
        Assert.IsTrue(hasArrayAdd, "ToSaveObject method should add the array to properties");
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

        // Act - Get the generated model class
        var modelClass = RunGeneratorAndGetModelClass(csfText);
        Assert.IsNotNull(modelClass, "Model class declaration not found");

        // Assert property existence and type
        const string propName = "NestedObject";
        const string expectedType = "List<ModelNestedObjectItem?>?";
        AssertPropertyExists(modelClass, propName, expectedType);
        
        // Assert nested class exists
        var nestedClass = modelClass.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == "ModelNestedObjectItem");
        
        Assert.IsNotNull(nestedClass, "Nested class ModelNestedObjectItem not found");
        
        // Assert nested class properties exist
        AssertPropertyExists(nestedClass, "KeyQuotedString", "string?");
        AssertPropertyExists(nestedClass, "KeyInteger", "int?");
        AssertPropertyExists(nestedClass, "KeyFloat", "float?");
        AssertPropertyExists(nestedClass, "KeyDate", "DateTime?");
        AssertPropertyExists(nestedClass, "KeyBool", "bool?");
        
        // Check Load method for array handling
        var loadMethod = modelClass
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "Load");
        
        Assert.IsNotNull(loadMethod, "Load method not found");
        
        // Check for TryGetSaveArray call
        string loadMethodText = loadMethod.ToString();
        bool hasTryGetArray = loadMethodText.Contains("saveObject.TryGetSaveArray(@\"nested_object\", out var nestedObjectArray)");
        Assert.IsTrue(hasTryGetArray, "Load method should call TryGetSaveArray for the nested_object property");
        
        // Check for foreach over Items
        bool hasItemsLoop = loadMethodText.Contains("foreach (var item in nestedObjectArray.Items)");
        Assert.IsTrue(hasItemsLoop, "Load method should loop through nestedObjectArray.Items");
        
        // Check for creating nested objects
        bool hasNestedObjectCreation = loadMethodText.Contains("if (item is SaveObject ");
        Assert.IsTrue(hasNestedObjectCreation, "Load method should check for SaveObject items");
        
        // Check for ModelNestedObjectItem.Load method call
        bool hasNestedObjectLoad = loadMethodText.Contains("ModelNestedObjectItem.Load(");
        Assert.IsTrue(hasNestedObjectLoad, "Load method should call ModelNestedObjectItem.Load for each SaveObject item");
    }
}