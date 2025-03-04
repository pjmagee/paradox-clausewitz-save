using System.Linq;

public abstract class Element
{
    public override string ToString()
    {
        if (this is Array array)
        {
            return $"Array: {string.Join(", ", array.Items.Select(i => i.ToString()))}";
        }

        if (this is Object obj)
        {
            return $"Object: {string.Join(", ", obj.Properties.Select(p => p.ToString()))}";
        }

        if (this is Scalar scalar)
        {
            return $"Scalar: {scalar.Value}";
        }

        return $"{GetType().Name}";
    }
}
