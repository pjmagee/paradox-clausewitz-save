using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an agreement in the game state.
/// </summary>
public class Agreement
{
    /// <summary>
    /// Gets or sets the agreement ID.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets or sets the first party ID.
    /// </summary>
    public long First { get; init; }

    /// <summary>
    /// Gets or sets the second party ID.
    /// </summary>
    public long Second { get; init; }

    /// <summary>
    /// Gets or sets the agreement type.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    public DateOnly StartDate { get; init; }

    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    public DateOnly EndDate { get; init; }

    /// <summary>
    /// Gets or sets the agreement terms.
    /// </summary>
    public AgreementTerms Terms { get; init; } = new();

    /// <summary>
    /// Gets or sets the subject specialization.
    /// </summary>
    public SubjectSpecialization Specialization { get; init; } = new();

    /// <summary>
    /// Loads agreements from the game state.
    /// </summary>
    /// <param name="gameState">The game state root object to load from.</param>
    /// <returns>An immutable array of agreements.</returns>
    public static ImmutableArray<Agreement> Load(SaveObject gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState, nameof(gameState));
        
        var builder = ImmutableArray.CreateBuilder<Agreement>();

        var agreementsElement = gameState.Properties.FirstOrDefault(p => p.Key == "agreements").Value as SaveObject;
        var agreementsData = agreementsElement?.Properties.FirstOrDefault(p => p.Key == "agreements").Value as SaveObject;

        if (agreementsData != null)
        {
            foreach (var agreementEntry in agreementsData.Properties)
            {
                if (agreementEntry.Value is SaveObject agreementObj)
                {
                    var agreement = new Agreement
                    {
                        Id = long.Parse(agreementEntry.Key),
                        First = GetScalarLong(agreementObj, "owner"),
                        Second = GetScalarLong(agreementObj, "target"),
                        Type = GetScalarString(agreementObj, "active_status"),
                        StartDate = GetScalarDateOnly(agreementObj, "date_added"),
                        EndDate = GetScalarDateOnly(agreementObj, "date_changed"),
                        Terms = LoadTerms(agreementObj),
                        Specialization = LoadSpecialization(agreementObj)
                    };

                    builder.Add(agreement);
                }
            }
        }

        return builder.ToImmutable();
    }

    private static AgreementTerms LoadTerms(SaveObject obj)
    {
        var termData = GetObject(obj, "term_data");
        if (termData == null)
        {
            return new AgreementTerms();
        }

        var firstResources = ImmutableDictionary.CreateBuilder<string, float>();
        var secondResources = ImmutableDictionary.CreateBuilder<string, float>();
        var firstModifiers = ImmutableDictionary.CreateBuilder<string, float>();
        var secondModifiers = ImmutableDictionary.CreateBuilder<string, float>();

        var resourceTerms = GetObject(termData, "resource_terms");
        if (resourceTerms != null)
        {
            foreach (var term in resourceTerms.Properties)
            {
                if (term.Value is SaveObject termObj)
                {
                    var key = GetScalarString(termObj, "key");
                    var value = GetScalarFloat(termObj, "value");

                    if (!string.IsNullOrEmpty(key) && key.StartsWith("resource_subsidies_"))
                    {
                        var resourceKey = key.Replace("resource_subsidies_", "");
                        firstResources[resourceKey] = value;
                        secondResources[resourceKey] = value;
                    }
                }
            }
        }

        firstModifiers.Add("subject_power", 0f);
        firstModifiers.Add("overlord_power", 0f);
        secondModifiers.Add("subject_power", 0f);
        secondModifiers.Add("overlord_power", 0f);

        return new AgreementTerms
        {
            Type = GetScalarString(termData, "agreement_preset"),
            Level = GetScalarInt(termData, "forced_initial_loyalty"),
            Length = GetScalarInt(termData, "length"),
            First = GetScalarLong(obj, "owner"),
            Second = GetScalarLong(obj, "target"),
            FirstResources = firstResources.ToImmutable(),
            SecondResources = secondResources.ToImmutable(),
            FirstModifiers = firstModifiers.ToImmutable(),
            SecondModifiers = secondModifiers.ToImmutable()
        };
    }

    private static SubjectSpecialization LoadSpecialization(SaveObject obj)
    {
        var specializationObj = GetObject(obj, "subject_specialization");
        if (specializationObj == null)
        {
            return new SubjectSpecialization();
        }

        var conversionObj = GetObject(specializationObj, "subject_conversion_process");
        if (conversionObj == null)
        {
            return new SubjectSpecialization
            {
                Level = GetScalarFloat(specializationObj, "level"),
                Experience = GetScalarFloat(specializationObj, "experience")
            };
        }

        return new SubjectSpecialization
        {
            Level = GetScalarFloat(specializationObj, "level"),
            Experience = GetScalarFloat(specializationObj, "experience"),
            ConversionProcess = new SubjectConversionProcess
            {
                Progress = GetScalarFloat(conversionObj, "progress"),
                InProgress = GetScalarBoolean(conversionObj, "in_progress"),
                Done = GetScalarBoolean(conversionObj, "done"),
                Ignore = GetScalarBoolean(conversionObj, "ignore")
            }
        };
    }
}

/// <summary>
/// Represents the terms of an agreement.
/// </summary>
public class AgreementTerms
{
    /// <summary>
    /// Gets or sets the agreement type.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the agreement level.
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    /// Gets or sets the agreement length.
    /// </summary>
    public int Length { get; init; }

    /// <summary>
    /// Gets or sets the first party ID.
    /// </summary>
    public long First { get; init; }

    /// <summary>
    /// Gets or sets the second party ID.
    /// </summary>
    public long Second { get; init; }

    /// <summary>
    /// Gets or sets the first party resources.
    /// </summary>
    public ImmutableDictionary<string, float> FirstResources { get; init; } = ImmutableDictionary<string, float>.Empty;

    /// <summary>
    /// Gets or sets the second party resources.
    /// </summary>
    public ImmutableDictionary<string, float> SecondResources { get; init; } = ImmutableDictionary<string, float>.Empty;

    /// <summary>
    /// Gets or sets the first party modifiers.
    /// </summary>
    public ImmutableDictionary<string, float> FirstModifiers { get; init; } = ImmutableDictionary<string, float>.Empty;

    /// <summary>
    /// Gets or sets the second party modifiers.
    /// </summary>
    public ImmutableDictionary<string, float> SecondModifiers { get; init; } = ImmutableDictionary<string, float>.Empty;
}