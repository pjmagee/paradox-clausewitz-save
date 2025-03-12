using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public class SubjectSpecialization
{
    public float Level { get; set; }
    public float Experience { get; set; }
    public SubjectConversionProcess ConversionProcess { get; set; } = new();

    /// <summary>
    /// Loads subject specialization from a ClausewitzObject.
    /// </summary>
    /// <param name="agreementObj">The ClausewitzObject containing the agreement data.</param>
    /// <returns>A new SubjectSpecialization instance.</returns>
    public static SubjectSpecialization Load(SaveObject agreementObj)
    {
        var specialization = new SubjectSpecialization();
        var specializationElement = agreementObj.Properties
            .FirstOrDefault(p => p.Key == "subject_specialization");

        if (specializationElement.Key != null && specializationElement.Value is SaveObject specializationObj)
        {
            specialization.Level = SaveObjectHelper.GetScalarFloat(specializationObj, "level");
            specialization.Experience = SaveObjectHelper.GetScalarFloat(specializationObj, "experience");

            var conversionElement = specializationObj.Properties
                .FirstOrDefault(p => p.Key == "subject_conversion_process");
            if (conversionElement.Key != null && conversionElement.Value is SaveObject conversionObj)
            {
                specialization.ConversionProcess = SubjectConversionProcess.Load(conversionObj);
            }
        }

        return specialization;
    }
}