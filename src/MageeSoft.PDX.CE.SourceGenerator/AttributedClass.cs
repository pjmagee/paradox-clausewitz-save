namespace MageeSoft.PDX.CE.SourceGenerator;

/// <summary>
/// Represents a class with the GameStateDocumentAttribute.
/// </summary>
public class AttributedClass
{
    /// <summary>
    /// The name of the class.
    /// </summary>
    public string ClassName { get; set; } = "";
    
    /// <summary>
    /// The namespace of the class.
    /// </summary>
    public string Namespace { get; set; } = "";
    
    /// <summary>
    /// The schema file name specified in the attribute.
    /// </summary>
    public string SchemaFileName { get; set; } = "";
}