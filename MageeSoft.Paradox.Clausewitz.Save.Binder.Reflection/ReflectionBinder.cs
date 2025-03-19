using System.Collections;
using System.Collections.Immutable;
using System.Reflection;

using MageeSoft.Paradox.Clausewitz.Save.Parser;
using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Binder.Reflection;

/// <summary>
/// This does NOT support Native AOT compilation.
/// We should rely on the SourceGen generated Model binders instead.
/// </summary>
[Obsolete("Use the SourceGen generated Model binders instead")]
public static class ReflectionBinder
{
    static object? BindScalar(SaveElement element, Type targetType)
    {
        // For nullable types, get the underlying type
        Type nullableUnderlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (element is Scalar<string> stringScalar)
        {
            if (stringScalar.Value == "none")
                return null;
            if (nullableUnderlyingType == typeof(string))
                return stringScalar.Value;           
        }
        else if (element is Scalar<int> intScalar)
        {
            if (nullableUnderlyingType == typeof(int))
                return intScalar.Value;
            if (nullableUnderlyingType == typeof(float))
                return (float)intScalar.Value;
            if (nullableUnderlyingType == typeof(long))
                return (long)intScalar.Value;
        }
        else if (element is Scalar<long> longScalar)
        {
            if (nullableUnderlyingType == typeof(long))
                return longScalar.Value;
            if (nullableUnderlyingType == typeof(int) && longScalar.Value <= int.MaxValue)
                return (int)longScalar.Value;
            if (nullableUnderlyingType == typeof(float))
                return (float)longScalar.Value;
        }
        else if (element is Scalar<float> floatScalar)
        {
            if (nullableUnderlyingType == typeof(float))
                return floatScalar.Value;
            if (nullableUnderlyingType == typeof(int) && floatScalar.Value % 1 == 0)
                return (int)floatScalar.Value;
            if (nullableUnderlyingType == typeof(long) && floatScalar.Value % 1 == 0)
                return (long)floatScalar.Value;
        }
        else if (element is Scalar<bool> boolScalar)
        {
            if (nullableUnderlyingType == typeof(bool))
                return boolScalar.Value;
        }
        else if (element is Scalar<DateOnly> dateScalar)
        {
            if (nullableUnderlyingType == typeof(DateOnly))
                return dateScalar.Value;
        }
        else if (element is Scalar<Guid> guidScalar)
        {
            if (nullableUnderlyingType == typeof(Guid))
                return guidScalar.Value;
        }

        return null;
    }

    static object? BindScalar(SaveObject saveObject, string propertyName, Type targetType)
    {
        var prop = saveObject.Properties.FirstOrDefault(p => p.Key == propertyName);
        
        if (prop.Value is SaveElement element)
            return BindScalar(element, targetType);
        
        return null;
    }

    static object? BindObject(SaveObject saveObject, string propertyName, Type propertyType)
    {
        var prop = saveObject.Properties.FirstOrDefault(p => p.Key == propertyName);
        
        // Handle 'none' value
        if (prop.Value != null && prop.Value.ToString() == "none")
            return null;
        
        var obj = prop.Value as SaveObject;
        if (obj == null)
            return null;

        var bindMethod = typeof(ReflectionBinder).GetMethod(nameof(Bind))!;
        return bindMethod.MakeGenericMethod(propertyType).Invoke(null, [obj]);
    }

    static object? BindImmutableDictionary(SaveObject saveObject, Type keyType, Type valueType)
    {
        var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        var dictionary = (IDictionary)Activator.CreateInstance(dictionaryType)!;

        foreach (var prop in saveObject.Properties)
        {
            object? key = null;
            
            try
            {
                key = keyType == typeof(string) ? prop.Key : Convert.ChangeType(prop.Key, keyType);
            }
            catch
            {
                continue; // Skip if key cannot be converted
            }

            object? value = null;

            if (prop.Value is SaveObject obj)
            {
                var bindMethod = typeof(ReflectionBinder).GetMethod(nameof(Bind))!;
                value = bindMethod.MakeGenericMethod(valueType).Invoke(null, [obj]);
            }
            else if (prop.Value is SaveArray array)
            {
                value = BindArray(array, valueType);
            }
            else if (prop.Value is SaveElement scalar)
            {
                value = BindScalar(scalar, valueType);
            }

            if (key != null && value != null)
                dictionary.Add(key, value);
        }

        var toImmutableMethod = typeof(ImmutableDictionary).GetMethods()
            .First(m => m.Name == "CreateRange" && m.IsGenericMethod)
            .MakeGenericMethod(keyType, valueType);

        return toImmutableMethod.Invoke(null, [dictionary]);
    }

    static object? BindArray(SaveArray saveArray, Type targetType)
    {
        var elementType = targetType.IsArray ? targetType.GetElementType()! : targetType.GetGenericArguments()[0];
        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
        
        // Check if this is a nullable reference type
        bool isNullableElementType = !elementType.IsValueType || Nullable.GetUnderlyingType(elementType) != null;

        foreach (var element in saveArray.Items)
        {
            if (element is SaveObject obj)
            {
                var bindMethod = typeof(ReflectionBinder).GetMethod(nameof(Bind))!;
                var boundObject = bindMethod.MakeGenericMethod(elementType).Invoke(null, [obj]);
                if (boundObject != null)
                    list.Add(boundObject);
                else if (isNullableElementType)
                    list.Add(null!); // Add null for nullable types
            }
            else if (element is SaveArray array)
            {
                var value = BindArray(array, elementType);
                if (value != null)
                {
                    if (value is IEnumerable enumerable && value.GetType() != elementType)
                    {
                        foreach (var item in enumerable)
                            list.Add(item);
                    }
                    else
                    {
                        list.Add(value);
                    }
                }
            }
            else if (element is SaveElement scalar)
            {
                if (scalar.ToString() == "none" && isNullableElementType)
                {
                    list.Add(null!); // Add null for 'none' with nullable element types
                }
                else if (scalar.ToString() != "none")
                {
                    var scalarValue = BindScalar(scalar, elementType);
                    if (scalarValue != null)
                        list.Add(scalarValue);
                    else if (isNullableElementType)
                        list.Add(null!);
                }
            }
        }

        if (targetType.IsArray)
        {
            var array = Array.CreateInstance(elementType, list.Count);
            list.CopyTo(array, 0);
            return array;
        }
        else if (targetType.IsGenericType)
        {
            var genericTypeDef = targetType.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(ImmutableArray<>))
            {
                var toImmutableMethod = typeof(ImmutableArray).GetMethods()
                    .First(m => m.Name == "CreateRange" && m.IsGenericMethod)
                    .MakeGenericMethod(elementType);
                return toImmutableMethod.Invoke(null, [list]);
            }
            else if (genericTypeDef == typeof(ImmutableList<>))
            {
                var toImmutableMethod = typeof(ImmutableList).GetMethods()
                    .First(m => m.Name == "CreateRange" && m.IsGenericMethod)
                    .MakeGenericMethod(elementType);
                return toImmutableMethod.Invoke(null, [list]);
            }
            else if (genericTypeDef == typeof(List<>))
            {
                return list;
            }
        }

        return null;
    }

    static object? BindArray(SaveObject saveObject, string propertyName, Type propertyType)
    {
        var elementType = propertyType.IsArray ? propertyType.GetElementType()! : propertyType.GetGenericArguments()[0];
        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
        
        // Check if this is a nullable reference type
        bool isNullableElementType = !elementType.IsValueType || Nullable.GetUnderlyingType(elementType) != null;

        foreach (var prop in saveObject.Properties.Where(p => p.Key == propertyName))
        {
            if (prop.Value is SaveObject obj)
            {
                var bindMethod = typeof(ReflectionBinder).GetMethod(nameof(Bind))!;
                var boundObject = bindMethod.MakeGenericMethod(elementType).Invoke(null, [obj]);
                if (boundObject != null)
                    list.Add(boundObject);
                else if (isNullableElementType)
                    list.Add(null!); // Add null for nullable types
            }
            else if (prop.Value is SaveArray array)
            {
                var value = BindArray(array, propertyType);
                if (value != null)
                {
                    if (value is IEnumerable enumerable && value.GetType() != elementType)
                    {
                        foreach (var item in enumerable)
                            list.Add(item);
                    }
                    else
                    {
                        list.Add(value);
                    }
                }
            }
            else if (prop.Value is SaveElement scalar)
            {
                if (scalar.ToString() == "none" && isNullableElementType)
                {
                    list.Add(null!); // Add null for 'none' with nullable element types
                }
                else if (scalar.ToString() != "none")
                {
                    var scalarValue = BindScalar(scalar, elementType);
                    if (scalarValue != null)
                        list.Add(scalarValue);
                    else if (isNullableElementType)
                        list.Add(null!);
                }
            }
        }

        if (propertyType.IsArray)
        {
            var array = Array.CreateInstance(elementType, list.Count);
            list.CopyTo(array, 0);
            return array;
        }
        else if (propertyType.IsGenericType)
        {
            var genericTypeDef = propertyType.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(ImmutableArray<>))
            {                
                var toImmutableMethod = typeof(ImmutableArray).GetMethods()
                    .First(m => m.Name == "CreateRange" && m.IsGenericMethod)
                    .MakeGenericMethod(elementType);
                return toImmutableMethod.Invoke(null, [list]);
            }
            else if (genericTypeDef == typeof(ImmutableList<>))
            {
                var toImmutableMethod = typeof(ImmutableList).GetMethods()
                    .First(m => m.Name == "CreateRange" && m.IsGenericMethod)
                    .MakeGenericMethod(elementType);
                return toImmutableMethod.Invoke(null, [list]);
            }
            else if (genericTypeDef == typeof(List<>))
            {
                return list;
            }
        }

        return null;
    }

    public static T? Bind<T>(SaveObject saveObject) where T : new()
    {
        var result = new T();
        var type = typeof(T);
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            var scalarAttribute = property.GetCustomAttribute<SaveScalarAttribute>();
            var arrayAttribute = property.GetCustomAttribute<SaveArrayAttribute>();
            var objectAttribute = property.GetCustomAttribute<SaveObjectAttribute>();
            var indexedDictionaryAttribute = property.GetCustomAttribute<SaveIndexedDictionaryAttribute>();

            try
            {
                if (scalarAttribute != null)
                {
                    var propertyName = scalarAttribute.Name;
                    var value = BindScalar(saveObject, propertyName, property.PropertyType);
                    if (value != null)
                        property.SetValue(result, value);
                }
                else if (arrayAttribute != null)
                {
                    var propertyName = arrayAttribute.Name;
                    var value = BindArray(saveObject, propertyName, property.PropertyType);
                    if (value != null)
                        property.SetValue(result, value);
                    else if (property.PropertyType.IsArray)
                    {
                        // Initialize empty arrays to avoid null references
                        var elementType = property.PropertyType.GetElementType()!;
                        var emptyArray = Array.CreateInstance(elementType, 0);
                        property.SetValue(result, emptyArray);
                    }
                }
                else if (objectAttribute != null)
                {
                    var propertyName = objectAttribute.Name;
                    var value = BindObject(saveObject, propertyName, property.PropertyType);
                    if (value != null)
                        property.SetValue(result, value);
                }
                else if (indexedDictionaryAttribute != null)
                {
                    var propertyName = indexedDictionaryAttribute.Name;                    
                    var obj = saveObject.Properties.FirstOrDefault(p => p.Key == propertyName).Value as SaveObject;
                    
                    if (obj != null)
                    {
                        var genArgs = property.PropertyType.GetGenericArguments();
                        var keyType = genArgs[0];
                        var valueType = genArgs[1];
                        var value = BindImmutableDictionary(obj, keyType, valueType);
                        if (value != null)
                            property.SetValue(result, value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error binding property {property.Name}: {ex.Message}");
            }
        }

        return result;
    }
}