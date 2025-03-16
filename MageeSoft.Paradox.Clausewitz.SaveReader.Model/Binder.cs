using System.Collections;
using System.Reflection;
using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;
using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Model;

public static class Binder
{
    public static T Bind<T>(SaveObject saveObject) where T : new()
    {
        T instance = new();

        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            SavePropertyAttribute? savePropertyAttribute = prop.GetCustomAttribute<SavePropertyAttribute>();
            SaveArrayAttribute? saveArrayAttribute = prop.GetCustomAttribute<SaveArrayAttribute>();
            
            if(savePropertyAttribute != null)
            {
                string propertyName = savePropertyAttribute.Name ?? prop.Name;
                
                // Check if the property type is a collection
                bool isCollection = prop.PropertyType != typeof(string) && 
                    (prop.PropertyType.IsArray || 
                     (prop.PropertyType.IsGenericType && 
                      (prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
                       prop.PropertyType.GetGenericTypeDefinition() == typeof(IList<>) ||
                       prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                       prop.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))));

                if (isCollection)
                {
                    // Get all matching properties with the same name
                    var matchingProperties = saveObject.Properties
                        .Where(kvp => kvp.Key == propertyName)
                        .Select(kvp => kvp.Value)
                        .ToList();

                    var elementType = prop.PropertyType.IsArray
                        ? prop.PropertyType.GetElementType()
                        : prop.PropertyType.GetGenericArguments().FirstOrDefault();

                    if (elementType != null)
                    {
                        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

                        foreach (var item in matchingProperties)
                        {
                            if (item is SaveObject itemObject)
                            {
                                var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
                                var nestedItem = bindMethod.MakeGenericMethod(elementType).Invoke(null, [itemObject]);
                                list.Add(nestedItem);
                            }
                            else if (item is Scalar<string> strScalar && strScalar.Value == "none")
                            {
                                // Handle "none" value by creating an empty instance
                                var emptyInstance = Activator.CreateInstance(elementType);
                                list.Add(emptyInstance);
                            }
                        }

                        if (prop.PropertyType.IsArray)
                            prop.SetValue(instance, list.GetType().GetMethod("ToArray")!.Invoke(list, null));
                        else
                            prop.SetValue(instance, list);
                    }
                    continue;
                }

                if (saveObject.TryGetSaveObject(propertyName, out var nestedObject))
                {
                    if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                    {
                        if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        {
                            var keyType = prop.PropertyType.GetGenericArguments()[0];
                            var valueType = prop.PropertyType.GetGenericArguments()[1];
                            
                            var dictionary = (IDictionary)Activator.CreateInstance(prop.PropertyType)!;
                            
                            foreach (var kvp in nestedObject.Properties)
                            {
                                if (kvp.Value is SaveObject itemObject)
                                {
                                    var key = Convert.ChangeType(kvp.Key, keyType);
                                    var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
                                    var value = bindMethod.MakeGenericMethod(valueType).Invoke(null, [itemObject]);
                                        
                                    dictionary.Add(key, value);
                                }
                            }
                            
                            prop.SetValue(instance, dictionary);
                        }
                        else
                        {
                            var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
                            var nestedValue = bindMethod.MakeGenericMethod(prop.PropertyType).Invoke(null, [nestedObject]);
                            prop.SetValue(instance, nestedValue);
                        }
                        continue;
                    }
                }

                if (prop.PropertyType == typeof(string))
                {
                    if (saveObject.TryGetString(propertyName, out string value))
                        prop.SetValue(instance, value);
                }
                else if (prop.PropertyType == typeof(int))
                {
                    if (saveObject.TryGetInt(propertyName, out int value))
                        prop.SetValue(instance, value);
                }
                else if (prop.PropertyType == typeof(long))
                {
                    if (saveObject.TryGetLong(propertyName, out long value))
                        prop.SetValue(instance, value);
                }
                else if (prop.PropertyType == typeof(float))
                {
                    if (saveObject.TryGetFloat(propertyName, out float value))
                        prop.SetValue(instance, value);
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    if (saveObject.TryGetBool(propertyName, out bool value))
                        prop.SetValue(instance, value);
                }
                else if (prop.PropertyType == typeof(DateOnly))
                {
                    if (saveObject.TryGetDateOnly(propertyName, out DateOnly value))
                        prop.SetValue(instance, value);
                }
                else if (prop.PropertyType == typeof(Guid))
                {
                    if (saveObject.TryGetGuid(propertyName, out Guid value))
                        prop.SetValue(instance, value);
                }
            }

            if (saveArrayAttribute != null)
            {
                string arrayName = saveArrayAttribute.Name;
                var elementType = prop.PropertyType.IsArray
                    ? prop.PropertyType.GetElementType()
                    : prop.PropertyType.GetGenericArguments().FirstOrDefault();

                if (elementType != null)
                {
                    var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

                    // First try to get it as a SaveArray
                    if (saveObject.TryGetSaveArray(arrayName, out var array))
                    {
                        foreach (var item in array.Items)
                        {
                            if (item is Scalar<int> intScalar)
                            {
                                list.Add(Convert.ChangeType(intScalar.Value, elementType));
                            }
                            else if (item is Scalar<long> longScalar)
                            {
                                list.Add(Convert.ChangeType(longScalar.Value, elementType));
                            }
                            else if (item is Scalar<float> floatScalar)
                            {
                                list.Add(Convert.ChangeType(floatScalar.Value, elementType));
                            }
                            else if (item is Scalar<string> strScalar)
                            {
                                list.Add(Convert.ChangeType(strScalar.Value, elementType));
                            }
                            else if (item is SaveObject objItem)
                            {
                                var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
                                var nestedItem = bindMethod.MakeGenericMethod(elementType).Invoke(null, [objItem]);
                                list.Add(nestedItem);
                            }
                        }
                    }
                    // If not found as SaveArray, try to get multiple properties with the same name
                    else
                    {
                        var matchingProperties = saveObject.Properties
                            .Where(kvp => kvp.Key == arrayName)
                            .Select(kvp => kvp.Value)
                            .ToList();

                        foreach (var item in matchingProperties)
                        {
                            if (item is SaveObject objItem)
                            {
                                var bindMethod = typeof(Binder).GetMethod(nameof(Bind))!;
                                var nestedItem = bindMethod.MakeGenericMethod(elementType).Invoke(null, [objItem]);
                                list.Add(nestedItem);
                            }
                            else if (item is Scalar<string> strScalar && strScalar.Value == "none")
                            {
                                // Handle "none" value by creating an empty instance
                                var emptyInstance = Activator.CreateInstance(elementType);
                                list.Add(emptyInstance);
                            }
                        }
                    }

                    if (prop.PropertyType.IsArray)
                        prop.SetValue(instance, list.GetType().GetMethod("ToArray")!.Invoke(list, null));
                    else
                        prop.SetValue(instance, list);
                }
            }
        }

        return instance;
    }
}