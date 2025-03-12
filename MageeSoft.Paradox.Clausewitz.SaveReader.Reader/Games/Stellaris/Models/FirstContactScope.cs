using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a first contact scope.
/// </summary>
public class FirstContactScope
{
    /// <summary>
    /// Gets or sets the scope type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scope ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the opener ID.
    /// </summary>
    public long OpenerId { get; set; }

    /// <summary>
    /// Gets or sets whether random is allowed.
    /// </summary>
    public bool RandomAllowed { get; set; }

    /// <summary>
    /// Gets or sets the random values.
    /// </summary>
    public List<long> Random { get; set; } = new List<long>();

    /// <summary>
    /// Gets or sets the root scope.
    /// </summary>
    public FirstContactScope? Root { get; set; }

    /// <summary>
    /// Gets or sets the from scope.
    /// </summary>
    public FirstContactScope? From { get; set; }

    /// <summary>
    /// Loads a first contact scope from a ClausewitzObject.
    /// </summary>
    /// <param name="element">The ClausewitzObject containing the scope data.</param>
    /// <returns>A new FirstContactScope instance.</returns>
    public static FirstContactScope Load(SaveObject element)
    {
        var scope = new FirstContactScope();

        foreach (var property in element.Properties)
        {
            switch (property.Key)
            {
                case "type" when property.Value is Scalar<string> typeScalar:
                    scope.Type = typeScalar.Value;
                    break;
                case "id" when property.Value is Scalar<long> idScalar:
                    scope.Id = idScalar.Value;
                    break;
                case "opener_id" when property.Value is Scalar<long> openerIdScalar:
                    scope.OpenerId = openerIdScalar.Value;
                    break;
                case "random" when property.Value is SaveArray randomArray:
                    var randomValues = new List<long>();
                    foreach (var item in randomArray.Items)
                    {
                        if (item is Scalar<long> longScalar)
                        {
                            randomValues.Add(longScalar.Value);
                        }
                        else if (item is Scalar<string> strScalar && long.TryParse(strScalar.Value, out var value))
                        {
                            randomValues.Add(value);
                        }
                        else if (item is SaveObject obj)
                        {
                            var objValue = SaveObjectHelper.GetLongValue(obj, "value", 0);
                            if (objValue != 0)
                            {
                                randomValues.Add(objValue);
                            }
                        }
                        else
                        {
                            var str = item.ToString();
                            if (str.StartsWith("Scalar: ") && long.TryParse(str.Substring(8), out var parsed))
                            {
                                randomValues.Add(parsed);
                            }
                        }
                    }
                    scope.Random = randomValues;
                    break;
                case "random_allowed" when property.Value is Scalar<string> randomAllowedScalar:
                    scope.RandomAllowed = randomAllowedScalar.Value == "yes";
                    break;
                case "root" when property.Value is SaveObject rootObj:
                    scope.Root = Load(rootObj);
                    break;
                case "from" when property.Value is SaveObject fromObj:
                    scope.From = Load(fromObj);
                    break;
            }
        }

        return scope;
    }
}