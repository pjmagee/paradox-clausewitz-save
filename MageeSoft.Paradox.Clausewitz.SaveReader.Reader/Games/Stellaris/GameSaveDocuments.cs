using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;

public class GameSaveDocuments
{
    public GameStateDocument Meta { get; init; }
    public GameStateDocument GameState { get; init; }

    public GameSaveDocuments(GameStateDocument meta, GameStateDocument gameState)
    {
        Meta = meta;
        GameState = gameState;
    }

    public GameSaveDocuments(GameStateDocument gameState) : this(gameState, gameState)
    {
    }

    public GameSaveDocuments(SaveObject meta, SaveObject gameState)
    {
        Meta = new GameStateDocument(meta);
        GameState = new GameStateDocument(gameState);
    }

    public void Deconstruct(out GameStateDocument meta, out GameStateDocument gameState)
    {
        meta = Meta;
        gameState = GameState;
    }
}