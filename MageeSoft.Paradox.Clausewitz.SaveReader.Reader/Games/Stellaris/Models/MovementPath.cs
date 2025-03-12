using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a movement path in the game state.
/// </summary>
public class MovementPath
{
    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path points.
    /// </summary>
    public List<long> Points { get; set; } = new();

    /// <summary>
    /// Loads a movement path from a SaveElement.
    /// </summary>
    /// <param name="clausewitzElement">The SaveElement containing the movement path data.</param>
    /// <returns>A new MovementPath instance.</returns>
    public static MovementPath Load(SaveElement clausewitzElement)
    {
        var path = new MovementPath();
        var pathObj = clausewitzElement as SaveObject;
        if (pathObj != null)
        {
            var dateElement = pathObj.Properties.FirstOrDefault(p => p.Key == "date");
            if (dateElement.Key != null && dateElement.Value is Scalar<string> dateScalar)
            {
                path.Date = dateScalar.Value;
            }

            var pointsElement = pathObj.Properties.FirstOrDefault(p => p.Key == "points");
            if (pointsElement.Key != null && pointsElement.Value is SaveArray pointsArray)
            {
                foreach (var point in pointsArray.Items)
                {
                    if (point is Scalar<long> pointScalar)
                    {
                        path.Points.Add(pointScalar.Value);
                    }
                }
            }
        }

        return path;
    }
}