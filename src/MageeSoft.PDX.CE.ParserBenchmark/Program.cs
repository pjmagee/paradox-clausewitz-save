using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MageeSoft.PDX.CE;
using MageeSoft.PDX.CE2;

namespace MageeSoft.PDX.CE.ParserBenchmark;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Paradox Parser Benchmark Tool ===");
        Console.WriteLine("Running parser benchmarks...");
        
        var summary = BenchmarkRunner.Run<ParserBenchmarks>();
        Console.WriteLine(summary);
    }
}

/// <summary>
/// Benchmark class that follows BenchmarkDotNet conventions
/// </summary>
[MemoryDiagnoser]
[RyuJitX64Job]
public class ParserBenchmarks
{
    // Keep content in memory to ensure fair comparison
    private string _gameStateContent = string.Empty;
    
    // For memory test with ReadOnlyMemory
    private ReadOnlyMemory<char> _gameStateMemory;
    
    [GlobalSetup]
    public void Setup()
    {
        string testDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");
        string targetFile = Path.Combine(testDataDir, "gamestate");
        _gameStateContent = File.ReadAllText(targetFile);
        _gameStateMemory = _gameStateContent.AsMemory();
        Console.WriteLine($"Test file size: {_gameStateContent.Length:N0} bytes");
    }
    
    [Benchmark(Baseline = true, Description = "V1 Parser (string)")]
    public SaveObject ParserV1()
    {
        return Parser.Parse(_gameStateContent);
    }
    
    [Benchmark(Description = "V2 Parser (ReadOnlyMemory)")]
    public PdxObject ParserV2()
    {
        return PdxSaveReader.Read(_gameStateMemory);
    }
}
