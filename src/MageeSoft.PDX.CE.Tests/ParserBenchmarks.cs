using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using MageeSoft.PDX.CE;
using MageeSoft.PDX.CE2;

namespace MageeSoft.PDX.CE.Tests;

[TestClass]
public class ParserBenchmarks
{
    public TestContext TestContext { get; set; } = null!;
    
    // Path to the test gamestate file
    private string GamestatePath => Path.Combine(AppContext.BaseDirectory, "Stellaris", "TestData", "gamestate");

    // Keep content in memory to ensure fair comparison
    private string _gameStateContent = string.Empty;
    
    [TestInitialize]
    public void Initialize()
    {
        // Load the file content first so file I/O doesn't affect benchmarks
        _gameStateContent = File.ReadAllText(GamestatePath);
        
        // Ensure the file was loaded
        Assert.IsFalse(string.IsNullOrEmpty(_gameStateContent), "Test file should not be empty");
        
        TestContext.WriteLine($"Loaded test file: {GamestatePath}");
        TestContext.WriteLine($"File size: {_gameStateContent.Length:N0} bytes");
    }

    [TestMethod]
    public void RunParserComparison()
    {
        // We can run the comparison manually here to verify both parsers work with the file
        TestContext.WriteLine("Testing v1 Parser (Parser.cs)...");
        var v1Start = DateTime.Now;
        var v1Result = Parser.Parse(_gameStateContent);
        var v1Duration = DateTime.Now - v1Start;
        TestContext.WriteLine($"V1 Parser took: {v1Duration.TotalMilliseconds:N2}ms");
        TestContext.WriteLine($"V1 Object properties count: {v1Result.Properties.Count}");
        
        TestContext.WriteLine("\nTesting v2 Parser (PdxSaveReader.cs)...");
        var v2Start = DateTime.Now;
        var v2Result = PdxSaveReader.Read(_gameStateContent.AsMemory());
        var v2Duration = DateTime.Now - v2Start;
        TestContext.WriteLine($"V2 Parser took: {v2Duration.TotalMilliseconds:N2}ms");
        TestContext.WriteLine($"V2 Object properties count: {v2Result.Properties.Count}");
        
        // Compare property counts to verify they parse the same structure
        TestContext.WriteLine($"\nSpeed difference: {v1Duration.TotalMilliseconds / v2Duration.TotalMilliseconds:N2}x");

        // Optionally run the full benchmark suite if needed
        if (Environment.GetEnvironmentVariable("RUN_BENCHMARK") == "1")
        {
            TestContext.WriteLine("\nRunning full benchmark suite...");
            var summary = BenchmarkRunner.Run<ParserBenchmarkTests>();
            TestContext.WriteLine(summary.ToString());
        }
        else
        {
            TestContext.WriteLine("\nTo run the full benchmark with BenchmarkDotNet, set environment variable RUN_BENCHMARK=1");
        }
    }
}

/// <summary>
/// Benchmark class that follows BenchmarkDotNet conventions
/// </summary>
[MemoryDiagnoser]
// Using SimpleJob without specifying a runtime to use the current runtime (.NET 10.0 Preview)
[SimpleJob]
[RPlotExporter]
[HtmlExporter]
[Config(typeof(FastAndDirtyConfig))]
public class ParserBenchmarkTests
{
    // Path to the test gamestate file
    private string GamestatePath => Path.Combine(AppContext.BaseDirectory, "Stellaris", "TestData", "gamestate");

    // Keep content in memory to ensure fair comparison
    private string _gameStateContent = string.Empty;
    
    // For memory test with ReadOnlyMemory
    private ReadOnlyMemory<char> _gameStateMemory;
    
    [GlobalSetup]
    public void Setup()
    {
        // Load the file once for all benchmarks
        _gameStateContent = File.ReadAllText(GamestatePath);
        _gameStateMemory = _gameStateContent.AsMemory();
        
        // Verify file was loaded
        if (string.IsNullOrEmpty(_gameStateContent))
        {
            throw new InvalidOperationException($"Failed to load test file from {GamestatePath}");
        }
        
        // Warmup (optional)
        Parser.Parse(_gameStateContent[..100]);
        PdxSaveReader.Read(_gameStateMemory[..100]);
    }
    
    [Params(0.01, 0.1, 0.5, 1.0)]
    public double FileFraction { get; set; }
    
    [Benchmark(Baseline = true, Description = "V1 Parser (string)")]
    public SaveObject ParserV1()
    {
        return Parser.Parse(_gameStateContent);
    }
    
    [Benchmark(Description = "V2 Parser (ReadOnlyMemory)")]
    public PdxObject ParserV2()
    {
        return PdxSaveReader.Read(_gameStateContent.AsMemory());
    }
}

/// <summary>
/// Fast benchmark config for development testing
/// </summary>
public class FastAndDirtyConfig : ManualConfig
{
    public FastAndDirtyConfig()
    {
        // Add job with reduced measurement count for faster development iterations
        AddJob(Job.Default
            .WithWarmupCount(1)     // Reduced warmup
            .WithIterationCount(3)   // Fewer iterations
            .WithInvocationCount(1)  // Single invocation per iteration
        );
        
        // Still add the default exporters and diagnosers
        AddExporter(BenchmarkDotNet.Exporters.DefaultExporters.Html);
        AddDiagnoser(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);
    }
} 