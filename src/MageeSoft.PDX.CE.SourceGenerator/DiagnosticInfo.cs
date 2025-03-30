using Microsoft.CodeAnalysis;

namespace MageeSoft.PDX.CE.SourceGenerator;

public readonly struct DiagnosticInfo
{
    public string Id { get; }
    public string Title { get; }
    public string Message { get; }
    public DiagnosticSeverity Severity { get; }

    public DiagnosticInfo(string id, string title, string message, DiagnosticSeverity severity)
    {
        Id = id;
        Title = title;
        Message = message;
        Severity = severity;
    }
}