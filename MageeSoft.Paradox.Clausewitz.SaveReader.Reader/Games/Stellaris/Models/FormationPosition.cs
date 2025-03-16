using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;


public class FormationPosition
{

    [SaveProperty("x")]
    public float X { get; set; }

    [SaveProperty("y")]
    public float Y { get; set; }

    [SaveProperty("speed")]
    public float Speed { get; set; }

    [SaveProperty("rotation")]
    public  float Rotation { get; set; }

    [SaveProperty("forward_x")]
    public  float ForwardX { get; set; }

    [SaveProperty("forward_y")]
    public float ForwardY { get; set; }
}