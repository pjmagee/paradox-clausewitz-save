namespace MageeSoft.PDX.CE;

public class SaveArray(List<SaveElement> items) : SaveElement
{
    public List<SaveElement> Items { get; } = items;

    public override SaveType Type => SaveType.Array;
}