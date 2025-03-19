namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a situation in the game state.
/// </summary>
[SaveModel]
public partial class Situation
{
    /// <summary>
    /// Gets or sets the situation ID.
    /// </summary>
    public long Id { get;set; }

    /// <summary>
    /// Gets or sets the type of the situation.
    /// </summary>
    public string Type { get;set; }

    /// <summary>
    /// Gets or sets the country ID.
    /// </summary>
    public int Country { get;set; }

    /// <summary>
    /// Gets or sets the progress value.
    /// </summary>
    public double Progress { get;set; }

    /// <summary>
    /// Gets or sets the last month progress value.
    /// </summary>
    public double LastMonthProgress { get;set; }

    /// <summary>
    /// Gets or sets the approach value.
    /// </summary>
    public string Approach { get;set; }
} 






