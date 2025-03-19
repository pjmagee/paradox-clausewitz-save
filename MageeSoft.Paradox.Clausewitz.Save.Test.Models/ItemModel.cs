using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class ItemModel
{
    [SaveScalar("id")]
    public int Id { get; set; }
    
    [SaveScalar("description")]
    public string Description { get; set; } = string.Empty;
}