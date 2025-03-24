using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

/// <summary>
/// Simple position struct (xyz coordinates) that doesn't have its own Bind method
/// </summary>
[SaveModel]
public partial class Position
{
    [SaveScalar("x")]
    public float? X { get; set; }
    
    [SaveScalar("y")]
    public float? Y { get; set; }
    
    [SaveScalar("z")]
    public float? Z { get; set; }
}