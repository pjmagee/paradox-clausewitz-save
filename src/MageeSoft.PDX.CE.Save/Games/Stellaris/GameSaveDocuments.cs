namespace MageeSoft.PDX.CE.Save.Games.Stellaris;

public class GameSaveDocuments(GameStateDocument metaDocument, GameStateDocument gameStateDocument)
{
    /// <summary>
    ///  A stellaris meta document that is used to store the meta information of a game save.
    /// This is the smallest document and contains the least data but contains important information.
    /// </summary>
    public GameStateDocument MetaDocument { get; set; } = metaDocument;
    
    /// <summary>
    /// The stellaris gamestate document that is used to store the gamestate information of a game save.
    /// This is the largest document and contains the most information.
    /// </summary>
    public GameStateDocument GameStateDocument { get; set; } = gameStateDocument;
}