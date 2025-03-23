using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class NestedModel
{
    [SaveScalar("energy")]
    public int Energy { get;set; }

    [SaveScalar("minerals")]
    public int Minerals { get;set; }

    [SaveScalar("name")]
    public string Name { get;set; }

    [SaveScalar("efficiency")]
    public float Efficiency { get;set; }
    
    [SaveScalar("value")]
    public int Value { get; set; }
}