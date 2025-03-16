using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;
using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Model;

public static class Binder
{
    private static MethodInfo GetCreateRangeMethod(Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "CreateRange" &&
                        m.GetParameters().Length == 1 &&
                        m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            );
    }

    private static object? BindScalar(SaveObject saveObject, string propertyName, Type propertyType)
    {
        var element = saveObject.Properties.FirstOrDefault(p => p.Key == propertyName).Value;
        if (element == null)
            return null;

        if (element is SaveObject obj)
        {
            var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
            return bindMethod.MakeGenericMethod(propertyType).Invoke(null, [obj]);
        }
        else if (element is SaveArray array)
        {
            return BindArray(array, propertyType);
        }
        else if (element is SaveElement scalar)
        {
            return BindScalar(scalar, propertyType);
        }

        return null;
    }

    private static object? BindObject(SaveObject saveObject, string propertyName, Type propertyType)
    {
        var obj = saveObject.Properties.FirstOrDefault(p => p.Key == propertyName).Value as SaveObject;
        if (obj == null)
            return null;

        var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
        return bindMethod.MakeGenericMethod(propertyType).Invoke(null, [obj]);
    }

    private static object? BindImmutableList(SaveObject saveObject, Type elementType)
    {
        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

        foreach (var prop in saveObject.Properties)
        {
            if (prop.Value is SaveObject itemObject)
            {
                var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
                var nestedItem = bindMethod.MakeGenericMethod(elementType).Invoke(null, [itemObject]);
                if (nestedItem != null)
                    list.Add(nestedItem);
            }
            else if (prop.Value is Scalar<string> strScalar && strScalar.Value == "none")
            {
                // Handle "none" value by creating an empty instance
                var emptyInstance = Activator.CreateInstance(elementType);
                if (emptyInstance != null)
                    list.Add(emptyInstance);
            }
        }

        var toImmutableMethod = GetCreateRangeMethod(typeof(ImmutableList))
            .MakeGenericMethod(elementType);

        return toImmutableMethod.Invoke(null, [list]);
    }

    private static object? BindImmutableDictionary(SaveObject saveObject, Type keyType, Type valueType)
    {
        var dictionary = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType))!;

        foreach (var prop in saveObject.Properties)
        {
            if (prop.Value is SaveObject itemObject)
            {
                try
                {
                    var key = Convert.ChangeType(prop.Key, keyType);
                    var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
                    var value = bindMethod.MakeGenericMethod(valueType).Invoke(null, [itemObject]);

                    if (key != null && value != null)
                        dictionary.Add(key, value);
                }
                catch (Exception)
                {
                    // Skip invalid conversions
                    continue;
                }
            }
            else if (prop.Value is Scalar<float> floatScalar)
            {
                try
                {
                    var key = Convert.ChangeType(prop.Key, keyType);
                    var value = Convert.ChangeType(floatScalar.Value, valueType);

                    if (key != null && value != null)
                        dictionary.Add(key, value);
                }
                catch (Exception)
                {
                    // Skip invalid conversions
                    continue;
                }
            }
            else if (prop.Value is Scalar<int> intScalar)
            {
                try
                {
                    var key = Convert.ChangeType(prop.Key, keyType);
                    var value = Convert.ChangeType(intScalar.Value, valueType);

                    if (key != null && value != null)
                        dictionary.Add(key, value);
                }
                catch (Exception)
                {
                    // Skip invalid conversions
                    continue;
                }
            }
            else if (prop.Value is Scalar<string> strScalar)
            {
                try
                {
                    var key = Convert.ChangeType(prop.Key, keyType);
                    var value = Convert.ChangeType(strScalar.Value, valueType);

                    if (key != null && value != null)
                        dictionary.Add(key, value);
                }
                catch (Exception)
                {
                    // Skip invalid conversions
                    continue;
                }
            }
        }

        var toImmutableMethod = GetCreateRangeMethod(typeof(ImmutableDictionary))
            .MakeGenericMethod(keyType, valueType);

        return toImmutableMethod.Invoke(null, [dictionary]);
    }

    private static object? BindScalar(SaveElement element, Type propertyType)
    {
        switch (element.Type)
        {
            case SaveType.String:
                if (element is Scalar<string> strScalar)
                {
                    var stringValue = strScalar.Value.Trim('"');
                    if (propertyType == typeof(Guid))
                    {
                        if (Guid.TryParse(stringValue, out var guidValue))
                            return guidValue;
                        throw new InvalidCastException($"Cannot convert '{stringValue}' to GUID.");
                    }
                    return stringValue;
                }
                break;
            case SaveType.Int32:
                if (element is Scalar<int> intScalar)
                    return intScalar.Value;

                break;
            case SaveType.Int64:
                if (element is Scalar<long> longScalar)
                    return longScalar.Value;

                break;
            case SaveType.Float:
                if (element is Scalar<float> floatScalar)
                    return floatScalar.Value;

                break;
            case SaveType.Bool:
                if (element is Scalar<bool> boolScalar)
                    return boolScalar.Value;

                break;
            case SaveType.Date:
                if (element is Scalar<DateOnly> dateScalar)
                    return dateScalar.Value;

                break;
                
            case SaveType.Guid:
                if (element is Scalar<Guid> guidScalar)
                    return guidScalar.Value;
                
                break;
        }

        return null;
    }

    private static object? BindArray(SaveArray array, Type propertyType)
    {
        var elementType = propertyType.IsArray ? propertyType.GetElementType()! :
            propertyType.IsGenericType ? propertyType.GetGenericArguments()[0] :
            propertyType;

        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

        foreach (var element in array.Elements())
        {
            if (element is SaveObject obj)
            {
                var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
                var boundObject = bindMethod.MakeGenericMethod(elementType).Invoke(null, [obj]);
                if (boundObject != null)
                    list.Add(boundObject);
            }
            else if (element is SaveArray nestedArray)
            {
                var arrayValue = BindArray(nestedArray, elementType);
                if (arrayValue != null)
                    list.Add(arrayValue);
            }
            else if (element is SaveElement scalar)
            {
                var scalarValue = BindScalar(scalar, elementType);
                if (scalarValue != null)
                    list.Add(scalarValue);
            }
        }

        if (propertyType.IsArray)
        {
            var arrayInstance = Array.CreateInstance(elementType, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                arrayInstance.SetValue(list[i], i);
            }

            return arrayInstance;
        }
        else if (propertyType.IsGenericType)
        {
            var genericTypeDef = propertyType.GetGenericTypeDefinition();

            if (genericTypeDef == typeof(ImmutableList<>))
            {
                var createRangeMethod = typeof(ImmutableList).GetMethods()
                    .First(m => m.Name == "CreateRange" && m.IsGenericMethod)
                    .MakeGenericMethod(elementType);

                var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(elementType);
                var castedItems = castMethod.Invoke(null, [
                        list,
                    ]
                );

                return createRangeMethod.Invoke(null, [
                        castedItems,
                    ]
                );
            }
            else if (genericTypeDef == typeof(ImmutableArray<>))
            {
                var createRangeMethod = typeof(ImmutableArray)
                    .GetMethods()
                    .First(m => m.Name == "CreateRange" && m.IsGenericMethod)
                    .MakeGenericMethod(elementType);

                var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(elementType);
                var castedItems = castMethod.Invoke(null, [list,]);
                return createRangeMethod.Invoke(null, [castedItems,]);
            }
            else if (genericTypeDef == typeof(ImmutableDictionary<,>))
            {
                var keyType = propertyType.GetGenericArguments()[0];
                var valueType = propertyType.GetGenericArguments()[1];
                var dictionary = new Dictionary<object, object>();

                foreach (var item in list)
                {
                    if (item is SaveObject itemObj)
                    {
                        var key = itemObj.Properties.FirstOrDefault(p => p.Key == "key").Value;
                        var value = itemObj.Properties.FirstOrDefault(p => p.Key == "value").Value;

                        if (key != null && value != null)
                        {
                            var keyValue = BindScalar(key, keyType);
                            var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
                            var boundValue = bindMethod.MakeGenericMethod(valueType).Invoke(null, [value]);

                            if (keyValue != null && boundValue != null)
                                dictionary[keyValue] = boundValue;
                        }
                    }
                }

                var toImmutableMethod = typeof(ImmutableDictionary).GetMethods()
                    .First(m => m.Name == "CreateRange" && m.IsGenericMethod)
                    .MakeGenericMethod(keyType, valueType);

                var castMethod = typeof(Enumerable)
                    .GetMethod(nameof(Enumerable.Cast))!
                    .MakeGenericMethod(typeof(KeyValuePair<,>)
                        .MakeGenericType(keyType, valueType));
                
                var castedItems = castMethod.Invoke(null, [dictionary,]);
                return toImmutableMethod.Invoke(null, [castedItems,]);
            }
        }

        return list;
    }

    private static object? BindArray(SaveObject saveObject, string propertyName, Type propertyType)
    {
        if (saveObject.Properties.FirstOrDefault(p => p.Key == propertyName).Value is not SaveArray array) return null;
        return BindArray(array, propertyType);
    }

    public static T Bind<T>(SaveObject saveObject)
    {
        if (typeof(T).IsGenericType)
        {
            var genericTypeDef = typeof(T).GetGenericTypeDefinition();

            if (genericTypeDef == typeof(ImmutableDictionary<,>))
            {
                var keyType = typeof(T).GetGenericArguments()[0];
                var valueType = typeof(T).GetGenericArguments()[1];
                var dictionary = new Dictionary<object, object>();

                foreach (var prop in saveObject.Properties)
                {
                    if (prop.Value is SaveObject obj)
                    {
                        var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
                        var boundValue = bindMethod.MakeGenericMethod(valueType).Invoke(null, [obj]);

                        if (boundValue != null)
                        {
                            var convertedKey = Convert.ChangeType(prop.Key, keyType);
                            dictionary[convertedKey] = boundValue;
                        }
                    }
                }

                var toImmutableMethod = typeof(ImmutableDictionary).GetMethods()
                    .First(m => m.Name == "CreateRange" && m.IsGenericMethod)
                    .MakeGenericMethod(keyType, valueType);

                var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType));
                var castedItems = castMethod.Invoke(null, [dictionary]);
                return (T)toImmutableMethod.Invoke(null, [castedItems])!;
            }
            else if (genericTypeDef == typeof(ImmutableList<>))
            {
                var elementType = typeof(T).GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

                foreach (var prop in saveObject.Properties)
                {
                    if (prop.Value is SaveObject obj)
                    {
                        var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
                        var boundObject = bindMethod.MakeGenericMethod(elementType).Invoke(null, [obj]);
                        if (boundObject != null)
                            list.Add(boundObject);
                    }
                    else if (prop.Value is SaveArray array)
                    {
                        var arrayValue = BindArray(array, elementType);
                        if (arrayValue != null)
                            list.Add(arrayValue);
                    }
                    else if (prop.Value is SaveElement scalar)
                    {
                        var scalarValue = BindScalar(scalar, elementType);
                        if (scalarValue != null)
                            list.Add(scalarValue);
                    }
                }

                var toImmutableMethod = typeof(ImmutableList)
                    .GetMethods()
                    .First(m => m.Name == "CreateRange" && m.IsGenericMethod)
                    .MakeGenericMethod(elementType);

                var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(elementType);
                var castedItems = castMethod.Invoke(null, [list]);

                return (T)toImmutableMethod.Invoke(null, [castedItems])!;
            }
            else if (genericTypeDef == typeof(ImmutableArray<>))
            {
                var elementType = typeof(T).GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

                foreach (var prop in saveObject.Properties)
                {
                    if (prop.Value is SaveObject obj)
                    {
                        var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
                        var boundObject = bindMethod.MakeGenericMethod(elementType).Invoke(null, [obj]);
                        if (boundObject != null)
                            list.Add(boundObject);
                    }
                    else if (prop.Value is SaveArray array)
                    {
                        var arrayValue = BindArray(array, elementType);
                        if (arrayValue != null)
                            list.Add(arrayValue);
                    }
                    else if (prop.Value is SaveElement scalar)
                    {
                        var scalarValue = BindScalar(scalar, elementType);
                        if (scalarValue != null)
                            list.Add(scalarValue);
                    }
                }

                var toImmutableMethod = typeof(ImmutableArray).GetMethods()
                    .First(m => m.Name == "CreateRange" && m.IsGenericMethod)
                    .MakeGenericMethod(elementType);

                var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(elementType);
                var castedItems = castMethod.Invoke(null, [list,]);
                return (T)toImmutableMethod.Invoke(null, [castedItems,])!;
            }
        }

        T instance = (T)Activator.CreateInstance(typeof(T))!;

        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var scalarAttr = prop.GetCustomAttribute<SaveScalarAttribute>();
            var objectAttr = prop.GetCustomAttribute<SaveObjectAttribute>();
            var arrayAttr = prop.GetCustomAttribute<SaveArrayAttribute>();

            if (scalarAttr != null)
            {
                var value = BindScalar(saveObject, scalarAttr.Name, prop.PropertyType);
                if (value != null)
                    prop.SetValue(instance, value);
            }
            else if (objectAttr != null)
            {
                var value = BindObject(saveObject, objectAttr.Name, prop.PropertyType);
                if (value != null)
                    prop.SetValue(instance, value);
            }
            else if (arrayAttr != null)
            {
                var value = BindArray(saveObject, arrayAttr.Name, prop.PropertyType);
                if (value != null)
                    prop.SetValue(instance, value);
            }

            if (prop.GetCustomAttribute<SaveIndexedDictionaryAttribute>() is SaveIndexedDictionaryAttribute indexedAttr)
            {
                var indexedObject = saveObject[indexedAttr.PropertyName] as SaveObject;
                if (indexedObject != null)
                {
                    var dictionaryType = prop.PropertyType;
                    var keyType = dictionaryType.GenericTypeArguments[0];
                    var valueType = dictionaryType.GenericTypeArguments[1];

                    var dictionary = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType)) as IDictionary;

                    foreach (var kvp in indexedObject.Properties)
                    {
                        var key = Convert.ChangeType(kvp.Key, keyType);
                        var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!.MakeGenericMethod(valueType);
                        var value = bindMethod.Invoke(null, new object[] { kvp.Value });
                        dictionary.Add(key, value);
                    }

                    prop.SetValue(instance, dictionaryType.IsAssignableTo(typeof(ImmutableDictionary<,>).MakeGenericType(keyType, valueType))
                        ? dictionaryType.GetMethod("ToImmutableDictionary", Type.EmptyTypes)!.Invoke(dictionary, null)
                        : dictionary);
                }
            }
        }

        return instance;
    }
}