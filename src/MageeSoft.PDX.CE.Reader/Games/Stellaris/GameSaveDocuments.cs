namespace MageeSoft.PDX.CE.Reader.Games.Stellaris;

public class GameSaveDocuments(GameStateDocument metaDocument, GameStateDocument gameStateDocument)
{
    public GameStateDocument MetaDocument { get; set; } = metaDocument;
    public GameStateDocument GameStateDocument { get; set; } = gameStateDocument;
}