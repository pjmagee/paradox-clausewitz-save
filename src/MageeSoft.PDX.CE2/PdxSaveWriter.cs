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
    /// Writes a SaveElement to a file in Paradox save file format.
    /// </summary>
    /// <param name="element">The element to write.</param>
    /// <param name="filePath">The path of the file to write to.</param>
    public void WriteToFile(PdxElement element, string filePath)
    {
        var content = Write(element);
        File.WriteAllText(filePath, content + "\n");
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
                _builder.Append($"\"{str.Value}\"");
                break;
                
            case PdxScalar<string> str when str.Type == PdxType.Identifier:
                _builder.Append(str.Value);
                break;
                
            case PdxScalar<int> i:
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
                _builder.Append($"\"{d.Value:yyyy.M.d}\"");
                break;
                
            case PdxScalar<Guid> g:
                _builder.Append($"\"{g.Value}\"");
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
                if (!(item is PdxObject || item is PdxArray))
                {
                    _builder.AppendLine();
                }
                else
                {
                    _builder.AppendLine();
                }
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