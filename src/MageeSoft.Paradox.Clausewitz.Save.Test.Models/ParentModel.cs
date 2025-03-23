using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

/// <summary>
/// A complex model with nested objects for testing cascading binding.
/// </summary>
[SaveModel]
public partial class ParentModel
{
    [SaveScalar("name")]
    public string Name { get; set; } = string.Empty;
    
    [SaveObject("nested_object")]
    public NestedModel NestedObject { get; set; } = new();
    
    [SaveArray("item_array")]
    public ItemModel[] ItemArray { get; set; } = Array.Empty<ItemModel>();
    
    [SaveScalar("value")]
    public int Value { get; set; }
}