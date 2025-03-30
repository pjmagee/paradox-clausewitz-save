using Microsoft.CodeAnalysis;

namespace MageeSoft.PDX.CE.SourceGenerator;

public class SaveObjectAnalysis
{
    public string RootName { get; }
    
    // Make setter private or internal if only analyzer should set it
    public Dictionary<string, ClassDefinition> ClassDefinitions { get; internal set; } = new();
    public Exception? Error { get; private set; }
    public List<DiagnosticInfo> Diagnostics { get; } = new(); // Store diagnostics

    public SaveObjectAnalysis(string rootName)
    {
        RootName = rootName;
    }

    public void SetError(Exception ex)
    {
        Error = ex;
    }

    // Method to add diagnostic info
    public void AddDiagnostic(string id, string title, string message, DiagnosticSeverity severity)
    {
        Diagnostics.Add(new DiagnosticInfo(id, title, message, severity));
    }
}