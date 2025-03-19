using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Models;

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
}

[SaveModel]
public partial class NestedModel
{
    [SaveScalar("name")]
    public string Name { get; set; } = string.Empty;
    
    [SaveScalar("value")]
    public int Value { get; set; }
}

[SaveModel]
public partial class ItemModel
{
    [SaveScalar("id")]
    public int Id { get; set; }
    
    [SaveScalar("description")]
    public string Description { get; set; } = string.Empty;
} 