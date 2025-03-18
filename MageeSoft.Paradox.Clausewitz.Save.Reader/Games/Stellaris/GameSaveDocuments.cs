namespace MageeSoft.Paradox.Clausewitz.Save.Reader.Games.Stellaris;

public class GameSaveDocuments(GameStateDocument metaDocument, GameStateDocument gameStateDocument)
{
    public GameStateDocument MetaDocument { get; init; } = metaDocument;
    public GameStateDocument GameStateDocument { get; init; } = gameStateDocument;
}