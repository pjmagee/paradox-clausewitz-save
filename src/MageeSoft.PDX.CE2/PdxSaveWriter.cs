using System.Buffers;
using System.Text;

namespace MageeSoft.PDX.CE2;

/// <summary>
/// Writes SaveElements back to the Paradox Clausewitz Engine save file format.
/// </summary>
public class PdxSaveWriter
{
    private readonly StringBuilder _builder = new();
    private int _indentLevel = 0;
    private const string IndentString = "\t";
    
    // Pre-allocated common strings to avoid allocations
    private static readonly string[] _numericStrings = Enumerable.Range(0, 1000)
        .Select(i => i.ToString())
        .ToArray();
    
    /// <summary>
    /// Writes a SaveElement to a string in Paradox save file format.
    /// </summary>
    /// <param name="element">The element to write.</param>
    /// <returns>A string representation of the element in Paradox save format.</returns>
    public string Write(PdxElement element)
    {
        _builder.Clear();
        _indentLevel = 0;
        SerializeElement(element);
        return _builder.ToString().TrimEnd();
    }

    /// <summary>
    /// Writes a SaveElement directly to the specified file in Paradox save file format.
    /// </summary>
    /// <param name="element">The element to write.</param>
    /// <param name="filePath">The path of the file to write to.</param>
    public void WriteToFile(PdxElement element, string filePath)
    {
        // First write to a string builder
        _builder.Clear();
        _indentLevel = 0;
        SerializeElement(element);
        
        // Then write directly to the file using a char buffer for better performance
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        writer.Write(_builder);
        writer.WriteLine(); // Add trailing newline
    }
    
    /// <summary>
    /// Writes a SaveElement directly to a TextWriter in Paradox save file format.
    /// </summary>
    /// <param name="element">The element to write.</param>
    /// <param name="writer">The TextWriter to write to.</param>
    public void WriteTo(PdxElement element, TextWriter writer)
    {
        _builder.Clear();
        _indentLevel = 0;
        SerializeElement(element);
        writer.Write(_builder);
    }

    private void SerializeElement(PdxElement element, string? key = null)
    {
        if (key != null)
        {
            WriteIndent();
            _builder.Append(key);
            _builder.Append('=');
        }
        else if (!(element is PdxObject || element is PdxArray))
        {
            WriteIndent();
        }

        switch (element)
        {
            case PdxObject obj:
                SerializeObject(obj);
                break;
                
            case PdxArray arr:
                SerializeArray(arr);
                break;
                
            case PdxScalar<string> str when str.Type == PdxType.String:
                _builder.Append('"');
                _builder.Append(str.Value);
                _builder.Append('"');
                break;
                
            case PdxScalar<string> str when str.Type == PdxType.Identifier:
                _builder.Append(str.Value);
                break;
                
            case PdxScalar<int> i:
                // Use pre-allocated string if available
                if (i.Value >= 0 && i.Value < _numericStrings.Length)
                    _builder.Append(_numericStrings[i.Value]);
                else
                    _builder.Append(i.Value);
                break;
                
            case PdxScalar<long> l:
                _builder.Append(l.Value);
                break;
                
            case PdxScalar<float> f:
                // Use invariant culture to ensure correct decimal point
                _builder.Append(f.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                break;
                
            case PdxScalar<bool> b:
                _builder.Append(b.Value ? "yes" : "no");
                break;
                
            case PdxScalar<DateTime> d:
                // Format according to Paradox format
                _builder.Append('"');
                _builder.Append(d.Value.ToString("yyyy.M.d", System.Globalization.CultureInfo.InvariantCulture));
                _builder.Append('"');
                break;
                
            case PdxScalar<Guid> g:
                _builder.Append('"');
                _builder.Append(g.Value.ToString());
                _builder.Append('"');
                break;
                
            default:
                throw new InvalidOperationException($"Unsupported element type: {element.GetType().Name}");
        }
    }

    private void SerializeObject(PdxObject obj)
    {
        if (!obj.Properties.Any())
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
            {
                _builder.AppendLine();
            }
            
            SerializeElement(property.Value, property.Key);
            isFirst = false;
        }

        _indentLevel--;
        _builder.AppendLine();
        WriteIndent();
        _builder.Append("}");
    }

    private void SerializeArray(PdxArray array)
    {
        if (!array.Items.Any())
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
            {
                _builder.AppendLine();
            }
            
            SerializeElement(item);
            isFirst = false;
        }

        _builder.AppendLine();
        _indentLevel--;
        WriteIndent();
        _builder.Append("}");
    }

    private void WriteIndent()
    {
        for (int i = 0; i < _indentLevel; i++)
        {
            _builder.Append(IndentString);
        }
    }
} 