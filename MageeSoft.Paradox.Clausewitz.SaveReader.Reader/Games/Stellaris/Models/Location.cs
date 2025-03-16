namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record Location
{
    public required int Type { get; init; }
    public required long Id { get; init; }
}