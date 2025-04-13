using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MageeSoft.PDX.CE;

BenchmarkRunner.Run<ParserBenchmarks>(args: args);

/// <summary>
/// Benchmark class that follows BenchmarkDotNet conventions
/// </summary>
[MemoryDiagnoser]
public class ParserBenchmarks
{
    // Keep content in memory to ensure fair comparison
    string _gameStateContent = string.Empty;
    
    // For memory test with ReadOnlyMemory
    ReadOnlyMemory<char> _gameStateMemory;
    
    [GlobalSetup]
    public void Setup()
    {
        string testDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");
        string targetFile = Path.Combine(testDataDir, "gamestate");
        _gameStateContent = File.ReadAllText(targetFile);
        _gameStateMemory = _gameStateContent.AsMemory();
        Console.WriteLine($"Test file size: {_gameStateContent.Length:N0} bytes");
    }
    
    [Benchmark(Description = "V2 Parser (ReadOnlyMemory)")]
    public void ParserV2()
    {
        _ = PdxSaveReader.Read(_gameStateMemory);
    }
}