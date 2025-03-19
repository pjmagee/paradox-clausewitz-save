

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a formation position in the game state.
/// </summary>  
[SaveModel]
public partial class FormationPosition
{

    [SaveScalar("x")]
    public float X { get; set; }

    [SaveScalar("y")]
    public float Y { get; set; }

    [SaveScalar("speed")]
    public float Speed { get; set; }

    [SaveScalar("rotation")]
    public  float Rotation { get; set; }

    [SaveScalar("forward_x")]
    public  float ForwardX { get; set; }

    [SaveScalar("forward_y")]
    public float ForwardY { get; set; }
}