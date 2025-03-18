using MageeSoft.Paradox.Clausewitz.Save.Parser;

namespace MageeSoft.Paradox.Clausewitz.Save.Reader;

public class GameStateDocument
{
    public SaveObject Root { get; }
    
    public GameStateDocument(SaveObject root)
    {
        ArgumentNullException.ThrowIfNull(root, nameof(root));
        Root = root;
    }
    
    public static GameStateDocument Parse(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input, nameof(input));
        
        var parser = new Save.Parser.Parser(input);
        var root = parser.Parse();
        return new GameStateDocument(root);
    }

    public static GameStateDocument Parse(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo, nameof(fileInfo));
        return Parse(File.ReadAllText(fileInfo.FullName));
    }

    public static GameStateDocument Parse(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }
}
