namespace MageeSoft.PDX.CE;

/// <summary>
/// Provides jq-like query and set functionality for Paradox Clausewitz Engine save data.
/// </summary>
public class PdxQuery(IPdxElement root)
{
    /// <summary>
    /// Selects values from the Paradox save data using a simple path expression.
    /// Supports dot-separated paths, wildcards, numeric indices, and array filters.
    /// </summary>
    public IEnumerable<IPdxElement> Select(string path)
    {
        var segments = path.Split('.');
        return SelectRecursive(root, segments, 0);
    }
    
    /// <summary>
    /// Returns all items at the given path as a list.
    /// </summary>
    public List<IPdxElement> GetList(string path) => Select(path).ToList();
    
    /// <summary>
    /// Replaces the array at the given path with a new array.
    /// </summary>
    public void SetArray(string path, IEnumerable<IPdxElement> newItems)
    {
        var segments = path.Split('.');

        if (segments.Length == 1)
        {
            // Direct child of root
            if (root is PdxObject rootObj)
            {
                for (int i = 0; i < rootObj.Properties.Count; i++)
                {
                    if (rootObj.Properties[i].Key.ToString() == segments[0] && rootObj.Properties[i].Value is PdxArray)
                    {
                        rootObj.Properties[i] = new KeyValuePair<IPdxScalar, IPdxElement>(rootObj.Properties[i].Key, new PdxArray(newItems.ToList()));
                        return;
                    }
                }
            }
        }
        else
        {
            // Traverse to parent
            var parentSegments = segments.Take(segments.Length - 1).ToArray();
            var key = segments.Last();
            var parent = TraverseToParent(root, parentSegments, 0);

            if (parent is PdxObject parentObj)
            {
                for (int i = 0; i < parentObj.Properties.Count; i++)
                {
                    if (parentObj.Properties[i].Key.ToString() == key && parentObj.Properties[i].Value is PdxArray)
                    {
                        parentObj.Properties[i] = new KeyValuePair<IPdxScalar, IPdxElement>(parentObj.Properties[i].Key, new PdxArray(newItems.ToList()));
                        return;
                    }
                }
            }
        }
    }
    
    private IEnumerable<IPdxElement> SelectRecursive(IPdxElement current, string[] segments, int index)
    {
        while (index < segments.Length)
        {
            var segment = segments[index];

            // Array index
            if (current is PdxArray arr && segment.StartsWith("[") && segment.EndsWith("]") && int.TryParse(segment.Substring(1, segment.Length - 2), out var idx))
            {
                if (idx < 0 || idx >= arr.Items.Count)
                    yield break;

                current = arr.Items[idx];
                index++;

                continue;
            }

            // Wildcard
            if (current is PdxArray arr2 && segment == "[*]")
            {
                for (int i = 0; i < arr2.Items.Count; i++)
                {
                    foreach (var result in SelectRecursive(arr2.Items[i], segments, index + 1))
                        yield return result;
                }

                yield break;
            }

            // Filter
            if (current is PdxArray arr3 && segment.StartsWith("[?") && segment.EndsWith("]"))
            {
                var filter = segment.Substring(2, segment.Length - 3); // e.g. owner==84
                var parts = filter.Split(new[]
                    {
                        "=="
                    }, StringSplitOptions.None
                );

                if (parts.Length == 2)
                {
                    var field = parts[0];
                    var value = parts[1];

                    for (int i = 0; i < arr3.Items.Count; i++)
                    {
                        var item = arr3.Items[i];

                        if (item is PdxObject itemObj && itemObj.Properties != null)
                        {
                            var match = itemObj.Properties.FirstOrDefault(p => p.Key.ToString() == field);

                            if (match.Key != null && ElementToString(match.Value) == value)
                            {
                                foreach (var result in SelectRecursive(item, segments, index + 1))
                                    yield return result;
                            }
                        }
                    }
                }

                yield break;
            }

            // Object property
            if (current is PdxObject obj)
            {
                bool found = false;

                for (int i = 0; i < obj.Properties.Count; i++)
                {
                    var key = obj.Properties[i].Key.ToString();

                    if (key == segment)
                    {
                        found = true;
                        foreach (var result in SelectRecursive(obj.Properties[i].Value, segments, index + 1))
                            yield return result;
                    }
                }

                if (found)
                    yield break;

                yield break;
            }

            // Fallback: if current is array but segment is not an index/wildcard/filter, try to descend into each item
            if (current is PdxArray arrFallback)
            {
                for (int i = 0; i < arrFallback.Items.Count; i++)
                {
                    foreach (var result in SelectRecursive(arrFallback.Items[i], segments, index))
                        yield return result;
                }

                yield break;
            }


            yield break;
        }

        yield return current;
    }

    private IPdxElement? TraverseToParent(IPdxElement current, string[] segments, int index)
    {
        if (index >= segments.Length) return current;
        var segment = segments[index];

        if (current is PdxObject obj)
        {
            for (int i = 0; i < obj.Properties.Count; i++)
            {
                var key = obj.Properties[i].Key.ToString();

                if (key == segment)
                {
                    return TraverseToParent(obj.Properties[i].Value, segments, index + 1);
                }
            }
        }
        else if (current is PdxArray arr)
        {
            if (segment.StartsWith("[") && segment.EndsWith("]") && int.TryParse(segment.Substring(1, segment.Length - 2), out var idx))
            {
                if (idx >= 0 && idx < arr.Items.Count)
                {
                    return TraverseToParent(arr.Items[idx], segments, index + 1);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns all arrays of arrays at the given path.
    /// </summary>
    public IEnumerable<PdxArray> GetArraysOfArrays(string path)
    {
        return Select(path).OfType<PdxArray>().Where(a => a.Items.All(i => i is PdxArray));
    }

    /// <summary>
    /// Finds all properties with a specific key name and returns their values.
    /// </summary>
    public IEnumerable<IPdxElement> FindAllByKey(string keyName)
    {
        return FindAllByKeyRecursive(root, keyName);
    }

    private IEnumerable<IPdxElement> FindAllByKeyRecursive(IPdxElement element, string keyName)
    {
        if (element is PdxObject obj)
        {
            foreach (var kvp in obj.Properties)
            {
                if (kvp.Key.ToString() == keyName)
                    yield return kvp.Value;

                foreach (var found in FindAllByKeyRecursive(kvp.Value, keyName))
                    yield return found;
            }
        }
        else if (element is PdxArray arr)
        {
            foreach (var item in arr.Items)
            foreach (var found in FindAllByKeyRecursive(item, keyName))
                yield return found;
        }
    }

    /// <summary>
    /// Sets the value at the specified path, supporting dot-separated paths and array indices (e.g. foo.0.bar).
    /// Overwrites the value at the target location with the provided newValue.
    /// </summary>
    public bool SetValueByPath(string path, IPdxElement newValue)
    {
        var segments = path.Split('.');
        return SetValueByPathRecursive(root, segments, 0, newValue);
    }

    private bool SetValueByPathRecursive(IPdxElement current, string[] segments, int index, IPdxElement newValue)
    {
        if (index >= segments.Length)
            return false;

        var segment = segments[index];

        // Array index: e.g. [0]
        if (current is PdxArray arr && int.TryParse(segment, out int arrIdx))
        {
            if (arrIdx < 0 || arrIdx >= arr.Items.Count)
                return false;
            if (index == segments.Length - 1)
            {
                arr.Items[arrIdx] = newValue;
                return true;
            }
            return SetValueByPathRecursive(arr.Items[arrIdx], segments, index + 1, newValue);
        }

        // Object property
        if (current is PdxObject obj)
        {
            for (int i = 0; i < obj.Properties.Count; i++)
            {
                var key = obj.Properties[i].Key.ToString();
                if (key == segment)
                {
                    if (index == segments.Length - 1)
                    {
                        obj.Properties[i] = new KeyValuePair<IPdxScalar, IPdxElement>(obj.Properties[i].Key, newValue);
                        return true;
                    }
                    return SetValueByPathRecursive(obj.Properties[i].Value, segments, index + 1, newValue);
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Replaces the array at the given path with a new array.
    /// </summary>
    public bool SetArrayByPath(string path, IEnumerable<IPdxElement> newItems)
    {
        var segments = path.Split('.');
        return SetArrayByPathRecursive(root, segments, 0, newItems);
    }

    private bool SetArrayByPathRecursive(IPdxElement current, string[] segments, int index, IEnumerable<IPdxElement> newItems)
    {
        if (index >= segments.Length)
            return false;
        var segment = segments[index];
        if (current is PdxObject obj)
        {
            for (int i = 0; i < obj.Properties.Count; i++)
            {
                var key = obj.Properties[i].Key.ToString();
                if (key == segment)
                {
                    if (index == segments.Length - 1 && obj.Properties[i].Value is PdxArray)
                    {
                        obj.Properties[i] = new KeyValuePair<IPdxScalar, IPdxElement>(obj.Properties[i].Key, new PdxArray(newItems.ToList()));
                        return true;
                    }
                    return SetArrayByPathRecursive(obj.Properties[i].Value, segments, index + 1, newItems);
                }
            }
        }
        else if (current is PdxArray arr && int.TryParse(segment, out int arrIdx))
        {
            if (arrIdx < 0 || arrIdx >= arr.Items.Count)
                return false;
            if (index == segments.Length - 1 && arr.Items[arrIdx] is PdxArray)
            {
                arr.Items[arrIdx] = new PdxArray(newItems.ToList());
                return true;
            }
            return SetArrayByPathRecursive(arr.Items[arrIdx], segments, index + 1, newItems);
        }
        return false;
    }

    /// <summary>
    /// Helper to print an IPdxElement to string (for test/demo purposes).
    /// </summary>
    public static string? ElementToString(IPdxElement element)
    {
        return element switch
        {
            IPdxScalar scalar => scalar.ToString(),
            PdxObject obj => obj.ToString(),
            PdxArray arr => arr.ToString(),
            _ => throw new InvalidOperationException("Unexpected element type.")
        };
    }

    /// <summary>
    /// Parses user input into the correct IPdxElement type.
    /// </summary>
    public static IPdxElement ParseUserInput(string input)
    {
        input = input.Trim();
        if (input.StartsWith("{") && input.EndsWith("}")) return PdxSaveReader.ParseValue(input);
        if (int.TryParse(input, out var i)) return new PdxInt(i);
        if (long.TryParse(input, out var l)) return new PdxLong(l);
        if (float.TryParse(input, out var f)) return new PdxFloat(f);
        if (input == "yes") return new PdxBool(true);
        if (input == "no") return new PdxBool(false);
        if (Guid.TryParse(input, out var g)) return new PdxGuid(g);
        if (DateOnly.TryParse(input, out var d)) return new PdxDate(d);
        return new PdxString(input);
    }

    /// <summary>
    /// Finds all properties with a specific key name and returns their values and paths.
    /// </summary>
    public IEnumerable<(string Path, IPdxElement Value)> FindAllByKeyWithPath(string keyName)
    {
        return FindAllByKeyWithPathRecursive(root, keyName, "");
    }

    private IEnumerable<(string Path, IPdxElement Value)> FindAllByKeyWithPathRecursive(IPdxElement element, string keyName, string currentPath)
    {
        if (element is PdxObject obj)
        {
            foreach (var kvp in obj.Properties)
            {
                var keyStr = kvp.Key.ToString();
                var newPath = string.IsNullOrEmpty(currentPath) ? keyStr : $"{currentPath}.{keyStr}";
                if (keyStr == keyName)
                    yield return (newPath, kvp.Value);

                foreach (var found in FindAllByKeyWithPathRecursive(kvp.Value, keyName, newPath))
                    yield return found;
            }
        }
        else if (element is PdxArray arr)
        {
            for (int i = 0; i < arr.Items.Count; i++)
            {
                var newPath = $"{currentPath}.[{i}]";
                
                foreach (var found in FindAllByKeyWithPathRecursive(arr.Items[i], keyName, newPath))
                    yield return found;
            }
        }
    }
}