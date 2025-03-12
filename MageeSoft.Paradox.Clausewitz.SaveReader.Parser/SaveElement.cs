using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

/// <summary>
/// Base class for elements in a save file.
/// </summary>
public abstract class SaveElement
{
    /// <summary>
    /// Gets the type of the element.
    /// </summary>
    public abstract ValueType Type { get; }

    /// <summary>
    /// Tries to get the value as a SaveObject.
    /// </summary>
    /// <param name="result">The SaveObject if successful, null otherwise.</param>
    /// <returns>True if the conversion was successful, false otherwise.</returns>
    public bool TryGetObject(out SaveObject? result)
    {
        result = this as SaveObject;
        return result != null;
    }

    /// <summary>
    /// Tries to get the value as a SaveArray.
    /// </summary>
    /// <param name="result">The SaveArray if successful, null otherwise.</param>
    /// <returns>True if the conversion was successful, false otherwise.</returns>
    public bool TryGetArray(out SaveArray? result)
    {
        result = this as SaveArray;
        return result != null;
    }

    /// <summary>
    /// Tries to get the value as a Scalar of type T.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value.</typeparam>
    /// <param name="result">The scalar value if successful, default otherwise.</param>
    /// <returns>True if the conversion was successful, false otherwise.</returns>
    public bool TryGetScalar<T>(out T result)
    {
        if (this is Scalar<T> scalar)
        {
            result = scalar.Value;
            return true;
        }

        result = default!;
        return false;
    }

    /// <summary>
    /// Gets the value as a SaveObject, throwing an exception if the conversion fails.
    /// </summary>
    /// <returns>The SaveObject.</returns>
    /// <exception cref="InvalidCastException">Thrown when the element is not a SaveObject.</exception>
    public SaveObject AsObject()
    {
        if (TryGetObject(out var result))
        {
            return result!;
        }

        throw new InvalidCastException($"Cannot convert {GetType().Name} to SaveObject");
    }

    /// <summary>
    /// Gets the value as a SaveArray, throwing an exception if the conversion fails.
    /// </summary>
    /// <returns>The SaveArray.</returns>
    /// <exception cref="InvalidCastException">Thrown when the element is not a SaveArray.</exception>
    public SaveArray AsArray()
    {
        if (TryGetArray(out var result))
        {
            return result!;
        }

        throw new InvalidCastException($"Cannot convert {GetType().Name} to SaveArray");
    }

    /// <summary>
    /// Gets the value as a Scalar of type T, throwing an exception if the conversion fails.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value.</typeparam>
    /// <returns>The scalar value.</returns>
    /// <exception cref="InvalidCastException">Thrown when the element is not a Scalar of type T.</exception>
    public T AsScalar<T>()
    {
        if (TryGetScalar<T>(out var result))
        {
            return result!;
        }

        throw new InvalidCastException($"Cannot convert {GetType().Name} to Scalar<{typeof(T).Name}>");
    }

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