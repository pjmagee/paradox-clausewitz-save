using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an FTL jump in the game state.
/// </summary>
public record FtlJump
{
    /// <summary>
    /// Gets or sets the origin coordinate.
    /// </summary>
    public required Coordinate From { get; init; }

    /// <summary>
    /// Gets or sets the destination system ID.
    /// </summary>
    public long? To { get; init; }

    /// <summary>
    /// Gets or sets the fleet ID.
    /// </summary>
    public required long Fleet { get; init; }

    /// <summary>
    /// Gets or sets the jump method.
    /// </summary>
    public required string JumpMethod { get; init; }

    /// <summary>
    /// Gets or sets the bypass from ID.
    /// </summary>
    public required long BypassFrom { get; init; }

    /// <summary>
    /// Gets or sets the bypass to ID.
    /// </summary>
    public required long BypassTo { get; init; }

    /// <summary>
    /// Creates a new instance of FtlJump with default values.
    /// </summary>
    public FtlJump()
    {
        From = Coordinate.Default;
        To = null;
        Fleet = 4294967295;
        JumpMethod = "jump_count";
        BypassFrom = 4294967295;
        BypassTo = 4294967295;
    }

    /// <summary>
    /// Default instance of FtlJump.
    /// </summary>
    public static FtlJump Default { get; } = new()
    {
        From = Coordinate.Default,
        To = null,
        Fleet = 4294967295,
        JumpMethod = "jump_count",
        BypassFrom = 4294967295,
        BypassTo = 4294967295
    };

    /// <summary>
    /// Loads an FTL jump from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the FTL jump data.</param>
    /// <returns>A new FtlJump instance.</returns>
    public static FtlJump? Load(SaveObject saveObject)
    {
        SaveObject? fromObj;
        if (!saveObject.TryGetSaveObject("from", out fromObj) || fromObj == null)
        {
            return null;
        }

        var from = Coordinate.Load(fromObj);
        if (from == null)
        {
            return null;
        }

        if (!saveObject.TryGetLong("fleet", out var fleet))
        {
            return null;
        }

        if (!saveObject.TryGetString("jump_method", out var jumpMethod))
        {
            return null;
        }

        return new FtlJump
        {
            From = from,
            To = saveObject.TryGetLong("to", out var to) ? to : null,
            Fleet = fleet,
            JumpMethod = jumpMethod,
            BypassFrom = saveObject.TryGetLong("bypass_from", out var bypassFrom) ? bypassFrom : 4294967295,
            BypassTo = saveObject.TryGetLong("bypass_to", out var bypassTo) ? bypassTo : 4294967295
        };
    }
}






