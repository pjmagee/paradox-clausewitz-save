namespace MageeSoft.PDX.CE;

public class Token(TokenType type, string text)
{
    public TokenType Type { get; set; } = type;

    public string Text { get; set; } = text;

    public override string ToString() => $"Token({Type}, '{Text}')";

}