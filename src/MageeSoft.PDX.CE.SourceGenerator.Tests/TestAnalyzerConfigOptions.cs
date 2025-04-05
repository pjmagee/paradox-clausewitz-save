using Microsoft.CodeAnalysis.Diagnostics;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests;

class TestAnalyzerConfigOptions(Dictionary<string, string> options) : AnalyzerConfigOptions
{
    public override bool TryGetValue(string key, out string value) => options.TryGetValue(key, value: out value!);
}
