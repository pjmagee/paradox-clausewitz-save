using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a localized text entry with variables.
/// </summary>
public class LocalizedText
{
    /// <summary>
    /// Gets or sets the localization key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the variables used in the localized text.
    /// </summary>
    public Dictionary<string, LocalizedTextVariable> Variables { get; set; } = new();

    /// <summary>
    /// Loads localized text from a ClausewitzObject.
    /// </summary>
    /// <param name="element">The ClausewitzObject containing the text data.</param>
    /// <returns>A new LocalizedText instance.</returns>
    public static LocalizedText Load(SaveObject element)
    {
        var text = new LocalizedText();

        // Handle the case where we're already in the text object
        foreach (var property in element.Properties)
        {
            switch (property.Key)
            {
                case "key" when property.Value is Scalar<string> keyScalar:
                    text.Key = keyScalar.Value;
                    break;
                case "variables" when property.Value is SaveObject variablesObj:
                    var variables = new Dictionary<string, LocalizedTextVariable>();
                    foreach (var varObj in variablesObj.Properties.Select(p => p.Value).OfType<SaveObject>())
                    {
                        var keyElement = varObj.Properties.FirstOrDefault(p => p.Key == "key");
                        var valueElement = varObj.Properties.FirstOrDefault(p => p.Key == "value");

                        if (keyElement.Key != null && valueElement.Key != null)
                        {
                            var variable = new LocalizedTextVariable();
                            if (keyElement.Value is Scalar<string> keyScalar)
                            {
                                variable.Key = keyScalar.Value;
                            }

                            if (valueElement.Value is SaveObject valueObjInner)
                            {
                                var keyProperty = valueObjInner.Properties.FirstOrDefault(p => p.Key == "key");
                                if (keyProperty.Key != null && keyProperty.Value is Scalar<string> valueKeyScalar)
                                {
                                    variable.Value = new LocalizedText { Key = valueKeyScalar.Value };
                                    
                                    // Check for nested variables
                                    var variablesProperty = valueObjInner.Properties.FirstOrDefault(p => p.Key == "variables");
                                    if (variablesProperty.Key != null && variablesProperty.Value is SaveObject nestedVariablesObj)
                                    {
                                        var nestedText = new LocalizedText { Key = valueKeyScalar.Value };
                                        foreach (var nestedVarObj in nestedVariablesObj.Properties.Select(p => p.Value).OfType<SaveObject>())
                                        {
                                            var nestedKeyElement = nestedVarObj.Properties.FirstOrDefault(p => p.Key == "key");
                                            var nestedValueElement = nestedVarObj.Properties.FirstOrDefault(p => p.Key == "value");

                                            if (nestedKeyElement.Key != null && nestedValueElement.Key != null)
                                            {
                                                var nestedVariable = new LocalizedTextVariable();
                                                if (nestedKeyElement.Value is Scalar<string> nestedKeyScalar)
                                                {
                                                    nestedVariable.Key = nestedKeyScalar.Value;
                                                }

                                                if (nestedValueElement.Value is SaveObject nestedValueObjInner)
                                                {
                                                    var nestedKeyProperty = nestedValueObjInner.Properties.FirstOrDefault(p => p.Key == "key");
                                                    if (nestedKeyProperty.Key != null && nestedKeyProperty.Value is Scalar<string> nestedValueKeyScalar)
                                                    {
                                                        nestedVariable.Value = new LocalizedText { Key = nestedValueKeyScalar.Value };
                                                    }
                                                }
                                                else if (nestedValueElement.Value is Scalar<string> nestedValueScalar)
                                                {
                                                    nestedVariable.Value = new LocalizedText { Key = nestedValueScalar.Value };
                                                }

                                                if (!string.IsNullOrEmpty(nestedVariable.Key))
                                                {
                                                    nestedText.Variables[nestedVariable.Key] = nestedVariable;
                                                }
                                            }
                                        }
                                        variable.Value = nestedText;
                                    }
                                }
                            }
                            else if (valueElement.Value is Scalar<string> valueScalar)
                            {
                                variable.Value = new LocalizedText { Key = valueScalar.Value };
                            }

                            if (!string.IsNullOrEmpty(variable.Key))
                            {
                                variables[variable.Key] = variable;
                            }
                        }
                    }
                    text.Variables = variables;
                    break;
            }
        }
        return text;
    }

    /// <summary>
    /// Loads localized text from a ClausewitzObject with a context key.
    /// </summary>
    /// <param name="obj">The ClausewitzObject containing the text data.</param>
    /// <param name="context">The context key for the text.</param>
    /// <returns>A new LocalizedText instance.</returns>
    public static LocalizedText Load(SaveObject obj, string context)
    {
        // Handle the case where the text object is passed directly
        if (obj.Properties.Any(p => p.Key == context))
        {
            var textElement = obj.Properties.FirstOrDefault(p => p.Key == context);
            if (textElement.Key != null && textElement.Value is SaveObject textObj)
            {
                return Load(textObj);
            }
        }

        return Load(obj);
    }
}