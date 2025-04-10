using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests;

[TestClass]
public class PerformanceTests
{
    readonly static AnalyzerConfigOptions ConfigOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>(
            new List<KeyValuePair<string, string>>([
                    new KeyValuePair<string, string>("build_property.PDXGenerateModels", "true")
                ]
            )
        )
    );
    
    const string SchemaFileName = "performance_test.csf";
    const string ModelTestClass = 
        """
        using MageeSoft.PDX.CE;
        namespace MageeSoft.PDX.CE.SourceGenerator.Tests
        {
            [GameStateDocument("performance_test.csf")] 
            public partial class PerformanceModel 
            {
            }
        }
        """;

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [Description("Stress test for the source generator with a large save file")]
    public void StressTest_LargeFile()
    {
        // Parameters that control the complexity of the generated file
        int width = 100;        // Number of properties at each level
        int depth = 5;          // Maximum depth of nested objects
        int arraySize = 50;     // Size of generated arrays
        int repeatedKeys = 20;  // Number of repeated keys at the root level

        // Generate a large test save file
        var csfText = GenerateLargeSaveFile(width, depth, arraySize, repeatedKeys);
        TestContext.WriteLine($"Generated test file size: {csfText.Length} characters");
        
        // Configure the generator
        var generator = new IncrementalGenerator();

        // Start the stopwatch
        var stopwatch = Stopwatch.StartNew();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(Path.GetFullPath(SchemaFileName), csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(ConfigOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(PerformanceTests),
            syntaxTrees: [CSharpSyntaxTree.ParseText(ModelTestClass)],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Act - run the generator
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        // Stop the stopwatch
        stopwatch.Stop();
        
        // Log timing information
        TestContext.WriteLine($"Generator execution time: {stopwatch.ElapsedMilliseconds}ms");
        
        // Report diagnostics
        foreach (var diagnostic in diagnostics)
        {
            TestContext.WriteLine($"Diagnostic: {diagnostic}");
        }

        // Verify generation completed successfully
        var generatedTree = outputCompilation.SyntaxTrees.FirstOrDefault(tree => tree.FilePath.Contains("PerformanceModel.g.cs"));
        Assert.IsNotNull(generatedTree, "Generated model not found");
        
        // Log some stats about the generated code
        var generatedCode = generatedTree.ToString();
        TestContext.WriteLine($"Generated code size: {generatedCode.Length} characters");
        TestContext.WriteLine($"Generated code lines: {generatedCode.Split('\n').Length}");
        
        // Count classes and properties in the generated code
        var root = generatedTree.GetRoot();
        int classCount = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Count();
        int propertyCount = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().Count();
        
        TestContext.WriteLine($"Generated classes: {classCount}");
        TestContext.WriteLine($"Generated properties: {propertyCount}");
    }

    [DataTestMethod]
    [Description("Parameterized stress test that can be used for profiling the source generator")]
    [DataRow(50, 3, 20, 10, "Small test")]
    [DataRow(100, 4, 30, 15, "Medium test")]
    [DataRow(200, 5, 40, 20, "Large test")]
    public void StressTest_Parameterized(int width, int depth, int arraySize, int repeatedKeys, string testName)
    {
        TestContext.WriteLine($"Running {testName} with parameters:");
        TestContext.WriteLine($"  Width: {width}");
        TestContext.WriteLine($"  Depth: {depth}");
        TestContext.WriteLine($"  Array Size: {arraySize}");
        TestContext.WriteLine($"  Repeated Keys: {repeatedKeys}");

        // Generate a large test save file
        var csfText = GenerateLargeSaveFile(width, depth, arraySize, repeatedKeys);
        TestContext.WriteLine($"Generated test file size: {csfText.Length} characters");
        
        // Configure the generator
        var generator = new IncrementalGenerator();

        // Start the stopwatch
        var stopwatch = Stopwatch.StartNew();

        // Give GC a chance to collect before running the test
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(Path.GetFullPath(SchemaFileName), csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(ConfigOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );

        var compilation = CSharpCompilation.Create(
            assemblyName: $"{nameof(PerformanceTests)}_{testName.Replace(" ", "_")}",
            syntaxTrees: [CSharpSyntaxTree.ParseText(ModelTestClass)],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Act - run the generator
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        // Stop the stopwatch
        stopwatch.Stop();
        
        // Log timing information
        TestContext.WriteLine($"Generator execution time: {stopwatch.ElapsedMilliseconds}ms");
        
        // Report diagnostics
        foreach (var diagnostic in diagnostics)
        {
            TestContext.WriteLine($"Diagnostic: {diagnostic}");
        }

        // Verify generation completed successfully
        var generatedTree = outputCompilation.SyntaxTrees.FirstOrDefault(tree => tree.FilePath.Contains("PerformanceModel.g.cs"));
        Assert.IsNotNull(generatedTree, "Generated model not found");
        
        // Log some stats about the generated code
        var generatedCode = generatedTree.ToString();
        TestContext.WriteLine($"Generated code size: {generatedCode.Length} characters");
        TestContext.WriteLine($"Generated code lines: {generatedCode.Split('\n').Length}");
        
        // Count classes and properties in the generated code
        var root = generatedTree.GetRoot();
        int classCount = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Count();
        int propertyCount = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().Count();
        
        TestContext.WriteLine($"Generated classes: {classCount}");
        TestContext.WriteLine($"Generated properties: {propertyCount}");
        
        // Performance metrics
        double msPerProperty = (double)stopwatch.ElapsedMilliseconds / propertyCount;
        TestContext.WriteLine($"Milliseconds per property: {msPerProperty:F2}ms");
        TestContext.WriteLine($"Properties per second: {(propertyCount * 1000.0 / stopwatch.ElapsedMilliseconds):F0}");
    }

    /// <summary>
    /// Generates a large save file with configurable complexity
    /// </summary>
    private string GenerateLargeSaveFile(int width, int depth, int arraySize, int repeatedKeys)
    {
        var sb = new StringBuilder();
        
        // Create the root object
        sb.AppendLine("root={");
        
        // Add regular properties at root level
        for (int i = 0; i < width; i++)
        {
            // Mix of different scalar types
            switch (i % 7)
            {
                case 0: sb.AppendLine($"    prop_int_{i}={i}"); break;
                case 1: sb.AppendLine($"    prop_float_{i}={i}.{i}"); break;
                case 2: sb.AppendLine($"    prop_string_{i}=\"String value {i}\""); break;
                case 3: sb.AppendLine($"    prop_date_{i}=\"2200.{(i % 12) + 1}.{(i % 28) + 1}\""); break;
                case 4: sb.AppendLine($"    prop_bool_{i}={((i % 2) == 0 ? "yes" : "no")}"); break;
                case 5: sb.AppendLine($"    prop_guid_{i}=\"{Guid.NewGuid()}\""); break;
                case 6: sb.AppendLine($"    prop_long_{i}={i * 1000000000}"); break;
            }
        }
        
        // Add repeated keys
        for (int i = 0; i < repeatedKeys; i++)
        {
            sb.AppendLine($"    repeated_key=\"Value {i}\"");
        }
        
        // Add repeated array keys
        for (int i = 0; i < repeatedKeys / 5; i++)
        {
            sb.AppendLine($"    repeated_array={{");
            for (int j = 0; j < 5; j++)
            {
                sb.Append($"        \"Item {i}-{j}\"");
                if (j < 4) sb.Append(' ');
            }
            sb.AppendLine($"    }}");
        }
        
        // Add arrays
        for (int i = 0; i < width / 10; i++)
        {
            sb.AppendLine($"    array_{i}={{");
            for (int j = 0; j < arraySize; j++)
            {
                // Mix of different array types
                switch (i % 5)
                {
                    case 0: sb.Append($" {j}"); break; // int array
                    case 1: sb.Append($" {j}.{j}"); break; // float array
                    case 2: sb.Append($" \"{Guid.NewGuid()}\""); break; // string/guid array
                    case 3: sb.Append($" \"2200.{(j % 12) + 1}.{(j % 28) + 1}\""); break; // date array
                    case 4: sb.Append($" {((j % 2) == 0 ? "yes" : "no")}"); break; // bool array
                }
            }
            sb.AppendLine($"    }}");
        }
        
        // Add nested objects with recursive depth
        for (int i = 0; i < width / 5; i++)
        {
            GenerateNestedObject(sb, $"nested_obj_{i}", 1, depth, width / 2, arraySize / 2);
        }
        
        // Add dictionary-like structures
        sb.AppendLine($"    string_dict={{");
        for (int i = 0; i < width / 5; i++)
        {
            sb.AppendLine($"        key_{i}=\"value_{i}\"");
        }
        sb.AppendLine($"    }}");
        
        sb.AppendLine($"    numeric_dict={{");
        for (int i = 0; i < width / 5; i++)
        {
            sb.AppendLine($"        {i}={{");
            sb.AppendLine($"            name=\"Item {i}\"");
            sb.AppendLine($"            value={i * 10}");
            sb.AppendLine($"        }}");
        }
        sb.AppendLine($"    }}");
        
        // Close the root object
        sb.AppendLine("}");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Recursively generates nested objects for testing
    /// </summary>
    private void GenerateNestedObject(StringBuilder sb, string name, int currentDepth, int maxDepth, int width, int arraySize)
    {
        string indent = new string(' ', (currentDepth + 1) * 4);
        
        sb.AppendLine($"{indent}{name}={{");
        
        // Add properties at this level
        for (int i = 0; i < width; i++)
        {
            string propIndent = indent + "    ";
            switch (i % 7)
            {
                case 0: sb.AppendLine($"{propIndent}prop_int_{i}={i}"); break;
                case 1: sb.AppendLine($"{propIndent}prop_float_{i}={i}.{i}"); break;
                case 2: sb.AppendLine($"{propIndent}prop_string_{i}=\"String value {i}\""); break;
                case 3: sb.AppendLine($"{propIndent}prop_date_{i}=\"2200.{(i % 12) + 1}.{(i % 28) + 1}\""); break;
                case 4: sb.AppendLine($"{propIndent}prop_bool_{i}={((i % 2) == 0 ? "yes" : "no")}"); break;
                case 5: sb.AppendLine($"{propIndent}prop_guid_{i}=\"{Guid.NewGuid()}\""); break;
                case 6: sb.AppendLine($"{propIndent}prop_long_{i}={i * 1000000000}"); break;
            }
        }
        
        // Add an array at this level
        if (currentDepth < maxDepth)
        {
            sb.AppendLine($"{indent}    array={{");
            for (int j = 0; j < arraySize; j++)
            {
                sb.Append($"{indent}    {j}");
                if (j < arraySize - 1) sb.Append(' ');
            }
            sb.AppendLine();
            sb.AppendLine($"{indent}    }}");
        }
        
        // Recursively add more nested objects if we haven't reached max depth
        if (currentDepth < maxDepth)
        {
            for (int i = 0; i < 3; i++) // Limit to 3 nested objects per level to avoid exponential growth
            {
                GenerateNestedObject(sb, $"sub_obj_{i}", currentDepth + 1, maxDepth, width / 2, arraySize / 2);
            }
        }
        
        sb.AppendLine($"{indent}}}");
    }

    [TestMethod]
    [Description("Test with a real save file provided by the user")]
    public void ProfileRealFile()
    {
        // Replace this path with the actual path to your real save file
        string realFilePath = Path.Combine("TestData", "stellaris_gamestate.csf");
        
        if (!File.Exists(realFilePath))
        {
            TestContext.WriteLine($"File not found: {realFilePath}");
            TestContext.WriteLine("Please update the path in the ProfileRealFile method to point to your actual file.");
            Assert.Inconclusive("Real file not found - update the path in the test method.");
            return;
        }
        
        // Read the file
        var csfText = File.ReadAllText(realFilePath);
        TestContext.WriteLine($"Loaded real save file. Size: {csfText.Length} characters");
        
        // Configure the generator
        var generator = new IncrementalGenerator();
        
        // Start the stopwatch
        var stopwatch = Stopwatch.StartNew();
        
        // Give GC a chance to collect before running the test
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [new TestAdditionalFile(Path.GetFullPath(SchemaFileName), csfText)],
            optionsProvider: new TestAnalyzerConfigOptionsProvider(ConfigOptions),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
        );
        
        var compilation = CSharpCompilation.Create(
            assemblyName: "RealFileTest",
            syntaxTrees: [CSharpSyntaxTree.ParseText(ModelTestClass)],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        
        // Act - run the generator
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        
        // Stop the stopwatch
        stopwatch.Stop();
        
        // Log timing information
        TestContext.WriteLine($"Generator execution time: {stopwatch.ElapsedMilliseconds}ms");
        
        // Report diagnostics
        foreach (var diagnostic in diagnostics)
        {
            TestContext.WriteLine($"Diagnostic: {diagnostic}");
        }
        
        // Verify generation completed successfully
        var generatedTree = outputCompilation.SyntaxTrees.FirstOrDefault(tree => tree.FilePath.Contains("PerformanceModel.g.cs"));
        
        if (generatedTree != null)
        {
            // Log some stats about the generated code
            var generatedCode = generatedTree.ToString();
            TestContext.WriteLine($"Generated code size: {generatedCode.Length} characters");
            TestContext.WriteLine($"Generated code lines: {generatedCode.Split('\n').Length}");
            
            // Count classes and properties in the generated code
            var root = generatedTree.GetRoot();
            int classCount = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Count();
            int propertyCount = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().Count();
            
            TestContext.WriteLine($"Generated classes: {classCount}");
            TestContext.WriteLine($"Generated properties: {propertyCount}");
            
            // Performance metrics
            double msPerProperty = (double)stopwatch.ElapsedMilliseconds / propertyCount;
            TestContext.WriteLine($"Milliseconds per property: {msPerProperty:F2}ms");
            TestContext.WriteLine($"Properties per second: {(propertyCount * 1000.0 / stopwatch.ElapsedMilliseconds):F0}");
            
            // Success!
            Assert.IsNotNull(generatedTree, "Generated model found successfully");
        }
        else
        {
            TestContext.WriteLine("Failed to generate model from real file.");
            Assert.Fail("Generated model not found");
        }
    }
} 