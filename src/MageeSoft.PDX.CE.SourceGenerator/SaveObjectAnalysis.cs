using Microsoft.CodeAnalysis;

namespace MageeSoft.PDX.CE.SourceGenerator;

public class SaveObjectAnalysis
{
    public string RootName { get; }
    public Dictionary<string, ClassDefinition> ClassDefinitions { get; internal set; } = new();
    public Exception? Error { get; private set; }
    public List<DiagnosticInfo> Diagnostics { get; } = new();

    public SaveObjectAnalysis(string rootName)
    {
        RootName = rootName;
    }

    public void SetError(Exception ex)
    {
        Error = ex;
    }

    public void AddDiagnostic(string id, string title, string message, DiagnosticSeverity severity)
    {
        Diagnostics.Add(new DiagnosticInfo(id, title, message, severity));
    }
}