using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class ItemModel
{
    [SaveScalar("id")]
    public int? Id { get; set; }
    
    [SaveScalar("description")]
    public string? Description { get; set; }
}

[SaveModel]
public partial class ModelWithDictionaryOfKeyValues
{
    [SaveIndexedDictionary("items")]
    public Dictionary<string, int>? Items { get; set; }
}