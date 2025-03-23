using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

/// <summary>
/// Test model that includes a Position struct to test source generator's struct handling
/// </summary>
[SaveModel]
public partial class PositionTestModel
{
    [SaveScalar("name")]
    public string Name { get; set; } = string.Empty;
    
    [SaveObject("position")]
    public Position Position { get; set; }
}