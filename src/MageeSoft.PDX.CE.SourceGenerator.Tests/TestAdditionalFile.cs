using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MageeSoft.PDX.CE.SourceGenerator.Tests;

public class TestAdditionalFile(string path, string text) : AdditionalText
{
    private readonly SourceText _text = SourceText.From(text);

    public override SourceText GetText(CancellationToken cancellationToken = new()) => _text;

    public override string Path { get; } = path;
}
