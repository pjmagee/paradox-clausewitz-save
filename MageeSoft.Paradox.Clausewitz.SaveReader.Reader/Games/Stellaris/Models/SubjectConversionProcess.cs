using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public class SubjectConversionProcess
{
    public float Progress { get; set; }
    public bool InProgress { get; set; }
    public bool Done { get; set; }
    public bool Ignore { get; set; }

    /// <summary>
    /// Loads subject conversion process from a ClausewitzObject.
    /// </summary>
    /// <param name="conversionObj">The ClausewitzObject containing the conversion process data.</param>
    /// <returns>A new SubjectConversionProcess instance.</returns>
    public static SubjectConversionProcess Load(SaveObject conversionObj)
    {
        var process = new SubjectConversionProcess
        {
            Progress = SaveObjectHelper.GetScalarFloat(conversionObj, "progress"),
            InProgress = SaveObjectHelper.GetScalarBoolean(conversionObj, "in_progress"),
            Done = SaveObjectHelper.GetScalarBoolean(conversionObj, "done"),
            Ignore = SaveObjectHelper.GetScalarBoolean(conversionObj, "ignore")
        };

        return process;
    }
}