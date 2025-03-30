using MageeSoft.PDX.CE.Reader.Games.Stellaris;

namespace MageeSoft.PDX.CE.Reader;

public class GameStateDocument
{
    public SaveObject Root { get; }
    
    private GameStateDocument(SaveObject root)
    {
        //ArgumentNullException.ThrowIfNull(root, nameof(root));
        Root = root;
    }
    
    public static GameStateDocument Parse(string input)
    {
        //ArgumentException.ThrowIfNullOrWhiteSpace(input, nameof(input));
        var root = Parser.Parse(input);
        return new GameStateDocument(root);
    }

    /// <summary>
    /// This should be used for parsing the meta or gamestate files in the .sav file
    /// Alternatively, use GameSaveZip to parse the entire .sav file.
    /// Or you can load a txt file with the same format as the meta or gamestate files
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns>
    ///  The parsed GameStateDocument.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the file does not exist.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the file is a .sav file
    /// </exception>
    public static GameStateDocument Parse(FileInfo fileInfo)
    {
        //ArgumentNullException.ThrowIfNull(fileInfo, nameof(fileInfo));
        
        if (!fileInfo.Exists)
            throw new FileNotFoundException("File not found", fileInfo.FullName);
        
        if (fileInfo.Extension.Equals(".sav", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Parsing .sav files is not supported. Use {nameof(GameSaveZip)} instead.");
        
        return Parse(File.ReadAllText(fileInfo.FullName));
    }

    public static GameStateDocument Parse(Stream stream)
    {
        //ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }
}
