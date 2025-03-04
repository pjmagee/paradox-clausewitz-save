using System.IO;

public class GameStateDocument
{
    public Element RootElement { get; }
    private GameStateDocument(Element rootElement)
    {
        RootElement = rootElement;
    }
    public static GameStateDocument Parse(string input)
    {
        var parser = new Parser(input);
        var root = parser.Parse();
        return new GameStateDocument(root);
    }
    public static GameStateDocument Parse(FileInfo fileInfo)
    {
        return Parse(File.ReadAllText(fileInfo.FullName));
    }
}
