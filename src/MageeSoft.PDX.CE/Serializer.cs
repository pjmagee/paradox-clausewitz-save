using System.Text;

namespace MageeSoft.PDX.CE;

/// <summary>
/// Serializes SaveElements back to the Paradox save file format.
/// </summary>
public class Serializer
{
    private readonly StringBuilder _builder;
    private int _indentLevel;
    private const string IndentString = "\t";
    private bool _isFirstElement = true;

    public Serializer()
    {
        _builder = new StringBuilder();
        _indentLevel = 0;
        _isFirstElement = true;
    }

    /// <summary>
    /// Serializes a SaveElement to a string in Paradox save file format.
    /// </summary>
    public string Serialize(SaveElement element)
    {
        _builder.Clear();
        _indentLevel = 0;
        _isFirstElement = true;
        SerializeElement(element);
        return _builder.ToString().TrimEnd();
    }

    /// <summary>
    /// Serializes a SaveElement to a file in Paradox save file format.
    /// </summary>
    public void SerializeToFile(SaveElement element, string filePath)
    {
        var content = Serialize(element);
        File.WriteAllText(filePath, content + "\n");
    }

    private void SerializeElement(SaveElement element, string? key = null)
    {
        // if (!_isFirstElement && key != null)
        // {
        //     _builder.Append('\n');
        // }
        
        if (key != null)
        {
            WriteIndent();
            _builder.Append(key);
            _builder.Append('=');
        }
        else if (!(element is SaveObject || element is SaveArray))
        {
            WriteIndent();
        }

        switch (element)
        {
            case SaveObject obj:
                SerializeObject(obj);
                break;
            case SaveArray arr:
                SerializeArray(arr);
                break;
            case Scalar<string> str when str.Type == SaveType.String:
                _builder.Append($"\"{str.RawText}\"");
                break;
            case Scalar<string> str when str.Type == SaveType.Identifier:
                _builder.Append(str.RawText);
                break;
            case Scalar<int> i:
                _builder.Append(i.RawText);
                break;
            case Scalar<long> l:
                _builder.Append(l.RawText);
                break;
            case Scalar<float> f:
                _builder.Append(f.RawText);
                break;
            case Scalar<bool> b:
                _builder.Append(b.Value ? "yes" : "no");
                break;
            case Scalar<DateTime> d:
                _builder.Append($"\"{d.RawText}\"");
                break;
            case Scalar<Guid> g:
                _builder.Append($"\"{g.RawText}\"");
                break;
            default:
                throw new ArgumentException($"Unsupported element type: {element.GetType().Name}");
        }

        _isFirstElement = false;
    }

    private void SerializeObject(SaveObject obj)
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
                _builder.Append('\n');
            }
            
            SerializeElement(property.Value, property.Key);
            isFirst = false;
        }

        _indentLevel--;
        _builder.AppendLine();
        WriteIndent();
        _builder.Append("}");
    }

    private void SerializeArray(SaveArray array)
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
            if (!isFirst && !(item is SaveObject || item is SaveArray))
            {
                _builder.AppendLine();
            }
            SerializeElement(item);
            isFirst = false;
        }

        if (!isFirst)
        {
            _builder.AppendLine();
        }
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