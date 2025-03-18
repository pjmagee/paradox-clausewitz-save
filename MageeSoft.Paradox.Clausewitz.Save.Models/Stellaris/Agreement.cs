using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

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
    /// Gets or sets the owner country ID.
    /// </summary>
    [SaveScalar("owner")]
    public long Owner { get; init; }

    /// <summary>
    /// Gets or sets the target country ID.
    /// </summary>
    [SaveScalar("target")]
    public long Target { get; init; }

    /// <summary>
    /// Gets or sets the active status.
    /// </summary>
    [SaveScalar("active_status")]
    public string ActiveStatus { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the date added.
    /// </summary>
    [SaveScalar("date_added")]
    public DateOnly DateAdded { get; init; }

    /// <summary>
    /// Gets or sets the date changed.
    /// </summary>
    [SaveScalar("date_changed")]
    public DateOnly DateChanged { get; init; }

    /// <summary>
    /// Gets or sets the agreement terms.
    /// </summary>
    [SaveObject("term_data")]
    public AgreementTerms Terms { get; init; }

    /// <summary>
    /// Gets or sets the subject specialization.
    /// </summary>
    [SaveObject("subject_specialization")]
    public SubjectSpecialization Specialization { get; init; }
}






