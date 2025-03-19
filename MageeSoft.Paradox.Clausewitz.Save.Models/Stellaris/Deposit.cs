namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a deposit in the game state.
/// </summary>  
[SaveModel]
public partial class Deposit
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string Type { get;set; }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public int Amount { get;set; }

    /// <summary>
    /// Gets or sets whether the deposit is infinite.
    /// </summary>
    public bool Infinite { get;set; }

    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    public Position? Position { get;set; }
    
} 






