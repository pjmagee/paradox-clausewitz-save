namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;

public class GameSaveDocuments(GameStateDocument metaDocument, GameStateDocument gameStateDocument)
{
    public GameStateDocument MetaDocument { get; init; } = metaDocument;
    public GameStateDocument GameStateDocument { get; init; } = gameStateDocument;
}