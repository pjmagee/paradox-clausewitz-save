using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

/// <summary>
/// Base class for elements in a save file.
/// </summary>
public abstract class SaveElement
{
    public abstract SaveType Type { get; }

    public override string ToString()
    {
        if (this is SaveArray array)
        {
            return $"Array: {string.Join(", ", array.Items.Select(i => i.ToString()))}";
        }

        if (this is SaveObject obj)
        {
            return $"Object: {string.Join(", ", obj.Properties.Select(p => p.ToString()))}";
        }
        
        switch(this)
        {
            case Scalar<string> scalar:
                return $"Scalar<{scalar.Type}>: {scalar.Value}";
            case Scalar<int> scalar:
                return $"Scalar<{scalar.Type}>: {scalar.Value}";
            case Scalar<float> scalar:
                return $"Scalar<{scalar.Type}>: {scalar.Value}";
            case Scalar<bool> scalar:
                return $"Scalar<{scalar.Type}>: {scalar.Value}";
            case Scalar<long> scalar:
                return $"Scalar<{scalar.Type}>: {scalar.Value}";
            case Scalar<DateOnly> scalar:
                return $"Scalar<{scalar.Type}>: {scalar.Value.ToString("yyyy.MM.dd", System.Globalization.CultureInfo.InvariantCulture)}";
            case Scalar<Guid> scalar:
                return $"Scalar<{scalar.Type}>: {scalar.Value}";    
        }

        return $"{GetType().Name}";
    }
}