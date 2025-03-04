// Enum for the value types.
// Base class for parsed elements.
// Represents an object (with duplicate keys allowed).
// Represents a primitive value along with its type and parsed value.

// Provides a JsonDocument-like API for the parsed game state.
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
