using System.Text;

namespace MageeSoft.PDX.CE.SourceGenerator;

public class IndentedStringBuilder
{
    readonly StringBuilder _sb = new(1024 * 1024); // Set an initial 1MB capacity
    int _indentationLevel;
    const int IndentSize = 4;
    string _currentIndent = "";

    public void Indent()
    {
        _indentationLevel++;
        _currentIndent = new string(' ', _indentationLevel * IndentSize);
    }

    public void Unindent()
    {
        if (_indentationLevel > 0)
        {
            _indentationLevel--;
            _currentIndent = new string(' ', _indentationLevel * IndentSize);
        }
    }

    public void AppendLine(string line = "")
    {
        if (!string.IsNullOrEmpty(line))
        {
            _sb.Append(_currentIndent);
        }

        _sb.AppendLine(line);
    }

    public void OpenBrace()
    {
        AppendLine("{");
        Indent();
    }

    public void CloseBrace()
    {
        Unindent();
        AppendLine("}");
    }

    // Allows setting the initial indentation level
    public void SetIndentLevel(int level)
    {
        _indentationLevel = Math.Max(0, level);
        _currentIndent = new string(' ', _indentationLevel * IndentSize);
    }

    public override string ToString() => _sb.ToString();
}