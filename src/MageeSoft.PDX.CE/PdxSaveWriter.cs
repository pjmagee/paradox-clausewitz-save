using System.Globalization;
using System.Text;

namespace MageeSoft.PDX.CE;

/// <summary>
/// Writes Paradox save elements to text.
/// </summary>
public sealed class PdxSaveWriter
{
    private readonly StringBuilder _builder = new();
    private int _indentLevel = 0;
    private const string IndentString = "\t";
    
    /// <summary>
    /// Writes an element to a string.
    /// </summary>
    public string Write(IPdxElement element)
    {
        _builder.Clear();
        _indentLevel = 0;
        SerializeElement(element);
        return _builder.ToString().TrimEnd();
    }
    
    /// <summary>
    /// Writes an element to a file.
    /// </summary>
    public void WriteToFile(IPdxElement element, string filePath)
    {
        _builder.Clear();
        _indentLevel = 0;
        SerializeElement(element);
        
        File.WriteAllText(filePath, _builder.ToString(), Encoding.UTF8);
    }
    
    /// <summary>
    /// Writes an element to a text writer.
    /// </summary>
    public void WriteTo(IPdxElement element, TextWriter writer)
    {
        _builder.Clear();
        _indentLevel = 0;
        SerializeElement(element);
        writer.Write(_builder);
    }
    
    /// <summary>
    /// Serializes an element to the internal builder.
    /// </summary>
    private void SerializeElement(IPdxElement element, PdxString? key = null)
    {
        // Write key if provided
        if (key != null)
        {
            WriteIndent();
            if (key.Value.WasQuoted || NeedsQuotes(key.Value.Value))
                _builder.Append('"').Append(key.Value.Value).Append('"');
            else
                _builder.Append(key.Value.Value);
                
            _builder.Append('=');
        }
        else if (element is not (PdxObject or PdxArray))
        {
            WriteIndent();
        }
        
        // Write value based on type
        switch (element)
        {
            case PdxObject obj:
                SerializeObject(obj, key == null);
                break;
                
            case PdxArray arr:
                SerializeArray(arr, key == null);
                break;
                
            case PdxString str:
                if (str.WasQuoted || NeedsQuotes(str.Value))
                    _builder.Append('"').Append(str.Value).Append('"');
                else
                    _builder.Append(str.Value);
                break;
                
            case PdxBool b:
                _builder.Append(b.Value ? "yes" : "no");
                break;
                
            case PdxInt i:
                _builder.Append(i.Value);
                break;
                
            case PdxLong l:
                _builder.Append(l.Value);
                break;
                
            case PdxFloat f:
                _builder.Append(f.Value.ToString(CultureInfo.InvariantCulture));
                break;
                
            case PdxDate d:
                _builder.Append('"').Append(d.Value.ToString("yyyy.M.d", CultureInfo.InvariantCulture)).Append('"');
                break;
                
            case PdxGuid g:
                _builder.Append('"').Append(g.Value.ToString()).Append('"');
                break;
                
            default:
                _builder.Append(element.ToString());
                break;
        }
    }
    
    /// <summary>
    /// Serializes an object to the internal builder.
    /// </summary>
    private void SerializeObject(PdxObject obj, bool needsIndent)
    {
        if (needsIndent)
            WriteIndent();
            
        if (obj.Properties.Length == 0)
        {
            _builder.Append("{ }");
            return;
        }
        
        _builder.AppendLine("{");
        _indentLevel++;
        
        bool isFirst = true;
        foreach (var property in obj.Properties)
        {
            if (!isFirst)
                _builder.AppendLine();
                
            SerializeElement(property.Value, property.Key);
            isFirst = false;
        }
        
        _indentLevel--;
        _builder.AppendLine();
        WriteIndent();
        _builder.Append("}");
    }
    
    /// <summary>
    /// Serializes an array to the internal builder.
    /// </summary>
    private void SerializeArray(PdxArray array, bool needsIndent)
    {
        if (needsIndent)
            WriteIndent();
            
        if (array.Items.Length == 0)
        {
            _builder.Append("{}");
            return;
        }
        
        _builder.AppendLine("{");
        _indentLevel++;
        
        bool isFirst = true;
        foreach (var item in array.Items)
        {
            if (!isFirst)
                _builder.AppendLine();
                
            SerializeElement(item);
            isFirst = false;
        }
        
        _indentLevel--;
        _builder.AppendLine();
        WriteIndent();
        _builder.Append("}");
    }
    
    /// <summary>
    /// Writes indentation to the internal builder.
    /// </summary>
    private void WriteIndent()
    {
        for (int i = 0; i < _indentLevel; i++)
            _builder.Append(IndentString);
    }
    
    /// <summary>
    /// Determines whether a string needs quotes.
    /// </summary>
    private static bool NeedsQuotes(string value)
    {
        if (string.IsNullOrEmpty(value))
            return true;
            
        // Check if it contains any characters that would require quotes
        foreach (char c in value)
        {
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != '.')
                return true;
        }
        
        // Check if it's a reserved keyword
        return IsReservedKeyword(value);
    }
    
    /// <summary>
    /// Determines whether a string is a reserved keyword.
    /// </summary>
    private static bool IsReservedKeyword(string value)
    {
        return value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("no", StringComparison.OrdinalIgnoreCase);
    }
} 