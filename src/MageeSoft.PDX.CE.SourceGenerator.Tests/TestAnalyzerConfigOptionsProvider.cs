using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests;

class TestAnalyzerConfigOptionsProvider(AnalyzerConfigOptions globalOptions) : AnalyzerConfigOptionsProvider
{
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => globalOptions;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => globalOptions;

    public override AnalyzerConfigOptions GlobalOptions => globalOptions;
}
