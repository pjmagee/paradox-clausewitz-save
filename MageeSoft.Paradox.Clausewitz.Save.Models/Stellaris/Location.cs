namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

public record Location
{
    public required int Type { get; init; }
    public required long Id { get; init; }
}