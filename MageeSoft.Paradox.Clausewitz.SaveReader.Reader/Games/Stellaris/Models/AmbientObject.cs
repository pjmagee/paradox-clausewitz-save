using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an ambient object in the game state.
/// </summary>
public class AmbientObject
{
    /// <summary>
    /// Gets or sets the ambient object ID.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets or sets the type of the ambient object.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the ambient object is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the coordinate of the ambient object.
    /// </summary>
    public Coordinate Coordinate { get; init; } = new();

    /// <summary>
    /// Gets or sets the data type of the ambient object.
    /// </summary>
    public string Data { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the properties of the ambient object.
    /// </summary>
    public AmbientObjectProperties Properties { get; init; } = new();

    /// <summary>
    /// Loads all ambient objects from the game state.
    /// </summary>
    /// <param name="gameState">The game state root object to load from.</param>
    /// <returns>An immutable array of ambient objects.</returns>
    public static ImmutableArray<AmbientObject> Load(SaveObject gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState, nameof(gameState));
        
        var builder = ImmutableArray.CreateBuilder<AmbientObject>();

        var ambientObjectsElement = gameState.Properties.FirstOrDefault(p => p.Key == "ambient_object");
        if (ambientObjectsElement.Value is not SaveObject ambientObjectsObj)
        {
            return builder.ToImmutable();
        }

        foreach (var ambientObjectElement in ambientObjectsObj.Properties)
        {
            if (long.TryParse(ambientObjectElement.Key, out var ambientObjectId))
            {
                if (ambientObjectElement.Value is SaveObject properties)
                {
                    var ambientObject = new AmbientObject 
                    { 
                        Id = ambientObjectId,
                        Type = GetScalarString(properties, "type"),
                        IsActive = GetScalarBoolean(properties, "is_active"),
                        Coordinate = properties.Properties.FirstOrDefault(p => p.Key == "coordinate").Value is SaveObject coordObj ? Coordinate.Load(coordObj) : new(),
                        Data = GetScalarString(properties, "data"),
                        Properties = properties.Properties.FirstOrDefault(p => p.Key == "properties").Value is SaveObject propsObj ? LoadProperties(propsObj) : new()
                    };

                    builder.Add(ambientObject);
                }
            }
        }

        return builder.ToImmutable();
    }

    private static AmbientObjectProperties LoadProperties(SaveObject propsObj)
    {
        var properties = new AmbientObjectProperties
        {
            Coordinate = propsObj.Properties.FirstOrDefault(p => p.Key == "coordinate").Value is SaveObject coordObj ? Coordinate.Load(coordObj) : new(),
            Attach = propsObj.Properties.FirstOrDefault(p => p.Key == "attach").Value is SaveObject attachObj ? LoadAttachInfo(attachObj) : new(),
            Scale = GetScalarFloat(propsObj, "scale"),
            EntityFaceObject = propsObj.Properties.FirstOrDefault(p => p.Key == "entity_face_object").Value is SaveObject faceObj ? LoadAttachInfo(faceObj) : new(),
            AppearState = GetScalarString(propsObj, "appear_state")
        };

        if (propsObj.Properties.FirstOrDefault(p => p.Key == "offset").Value is SaveArray offsetArray)
        {
            var offsetBuilder = ImmutableArray.CreateBuilder<float>();
            foreach (var element in offsetArray.Items)
            {
                if (element.TryGetScalar<float>(out var value))
                {
                    offsetBuilder.Add(value);
                }
            }
            // properties.Offset = offsetBuilder.ToImmutable();
        }

        return properties;
    }

    private static AttachInfo LoadAttachInfo(SaveObject attachObj)
    {
        return new AttachInfo
        {
            Type = GetScalarInt(attachObj, "type"),
            Id = GetScalarLong(attachObj, "id")
        };
    }

    private static string GetScalarString(SaveObject obj, string key)
    {
        var property = obj.Properties.FirstOrDefault(p => p.Key == key);
        return property.Value?.TryGetScalar<string>(out var value) == true ? value : string.Empty;
    }

    private static bool GetScalarBoolean(SaveObject obj, string key)
    {
        var property = obj.Properties.FirstOrDefault(p => p.Key == key);
        return property.Value?.TryGetScalar<bool>(out var value) == true && value;
    }

    private static int GetScalarInt(SaveObject obj, string key)
    {
        var property = obj.Properties.FirstOrDefault(p => p.Key == key);
        return property.Value?.TryGetScalar<int>(out var value) == true ? value : 0;
    }

    private static long GetScalarLong(SaveObject obj, string key)
    {
        var property = obj.Properties.FirstOrDefault(p => p.Key == key);
        return property.Value?.TryGetScalar<long>(out var value) == true ? value : 0;
    }

    private static float GetScalarFloat(SaveObject obj, string key)
    {
        var property = obj.Properties.FirstOrDefault(p => p.Key == key);
        return property.Value?.TryGetScalar<float>(out var value) == true ? value : 0f;
    }
}

/// <summary>
/// Represents the properties of an ambient object.
/// </summary>
public class AmbientObjectProperties
{
    /// <summary>
    /// Gets or sets the coordinate of the properties.
    /// </summary>
    public Coordinate Coordinate { get; init; } = new();

    /// <summary>
    /// Gets or sets the attach information.
    /// </summary>
    public AttachInfo Attach { get; init; } = new();

    /// <summary>
    /// Gets or sets the offset values.
    /// </summary>
    public ImmutableArray<float> Offset { get; init; } = ImmutableArray<float>.Empty;

    /// <summary>
    /// Gets or sets the scale value.
    /// </summary>
    public float Scale { get; init; }

    /// <summary>
    /// Gets or sets the entity face object information.
    /// </summary>
    public AttachInfo EntityFaceObject { get; init; } = new();

    /// <summary>
    /// Gets or sets the appear state.
    /// </summary>
    public string AppearState { get; init; } = string.Empty;
}

/// <summary>
/// Represents attach information for an ambient object.
/// </summary>
public class AttachInfo
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public int Type { get; init; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public long Id { get; init; }
} 