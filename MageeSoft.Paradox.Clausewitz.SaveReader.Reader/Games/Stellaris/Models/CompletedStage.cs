using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a completed stage in first contact.
/// </summary>
public class CompletedStage
{
    /// <summary>
    /// Gets or sets the date when the stage was completed.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stage name.
    /// </summary>
    public string Stage { get; set; } = string.Empty;

    /// <summary>
    /// Loads a completed stage from a ClausewitzObject.
    /// </summary>
    /// <param name="element">The ClausewitzObject containing the completed stage data.</param>
    /// <returns>A new CompletedStage instance.</returns>
    public static CompletedStage Load(SaveObject element)
    {
        var stage = new CompletedStage();

        foreach (var property in element.Properties)
        {
            switch (property.Key)
            {
                case "date" when property.Value is Scalar<string> dateScalar:
                    stage.Date = dateScalar.RawText;
                    break;
                case "stage" when property.Value is Scalar<string> stageScalar:
                    stage.Stage = stageScalar.Value;
                    break;
            }
        }

        // Set a default date if it's empty
        if (string.IsNullOrEmpty(stage.Date))
        {
            stage.Date = "2200.01.01";
        }

        return stage;
    }
}