using MageeSoft.PDX.CE.Reader.Games.Stellaris;
using MageeSoft.PDX.CE;

namespace MageeSoft.PDX.CE.Reader;

/// <summary>
///  Represents a parsed game state document which is a file found within a compressed save file.
/// </summary>
public class GameStateDocument
{
    /// <summary>
    ///  The root object of the parsed document.
    ///  This is the top-level object that contains all other properties
    /// </summary>
    public PdxObject Root { get; }
    
    /// <summary>
    ///  Creates a new instance of the GameStateDocument class with the specified root object.
    /// </summary>
    /// <param name="root">
    ///  The root object of the parsed document.
    /// </param>
    /// <exception cref="ArgumentException">
    ///  Thrown when the root object is null.
    /// </exception>
    private GameStateDocument(PdxObject root)
    {
        Root = root ?? throw new ArgumentException("Root object cannot be null", nameof(root));
    }
    
    public static GameStateDocument Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        
        return new GameStateDocument(PdxSaveReader.Read(input));
    }

    /// <summary>
    /// This should be used for parsing the meta or gamestate files in the .sav file
    /// Alternatively, use GameSaveZip to parse the entire .sav file.
    /// Or you can load a txt file with the same format as the meta or gamestate files
    /// </summary>
    /// <param name="fileInfo">
    /// The compressed save file to decompress and parse.
    /// </param>
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
        if (fileInfo is null)
            throw new ArgumentNullException(nameof(fileInfo), "FileInfo cannot be null");
        
        if (!fileInfo.Exists)
            throw new FileNotFoundException("File not found", fileInfo.FullName);
        
        if (fileInfo.Extension.Equals(".sav", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Parsing .sav files is not supported. Use {nameof(GameSaveZip)} instead.");
        
        return Parse(File.ReadAllText(fileInfo.FullName));
    }

    public static GameStateDocument Parse(Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream), "Stream cannot be null");

        var reader = new StreamReader(stream);

        try
        {
            return Parse(reader.ReadToEnd());
        }
        finally
        {
            reader.Dispose();
        }
    }
}
