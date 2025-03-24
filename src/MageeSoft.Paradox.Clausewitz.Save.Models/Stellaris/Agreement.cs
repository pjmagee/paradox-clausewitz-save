namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents an agreement in the game state.
/// </summary>
[SaveModel]
public partial class Agreement
{
    /// <summary>
    /// Gets or sets the agreement ID.
    /// </summary>
    public long? Id { get;set; }

    /// <summary>
    /// Gets or sets the owner country ID.
    /// </summary>
    [SaveScalar("owner")]
    public long? Owner { get;set; }

    /// <summary>
    /// Gets or sets the target country ID.
    /// </summary>
    [SaveScalar("target")]
    public long? Target { get;set; }

    /// <summary>
    /// Gets or sets the active status.
    /// </summary>
    [SaveScalar("active_status")]
    public string? ActiveStatus { get;set; }

    /// <summary>
    /// Gets or sets the date added.
    /// </summary>
    [SaveScalar("date_added")]
    public DateOnly? DateAdded { get;set; }

    /// <summary>
    /// Gets or sets the date changed.
    /// </summary>
    [SaveScalar("date_changed")]
    public DateOnly? DateChanged { get;set; }

    /// <summary>
    /// Gets or sets the agreement terms.
    /// </summary>
    [SaveObject("term_data")]
    public AgreementTerms? Terms { get;set; }

    /// <summary>
    /// Gets or sets the subject specialization.
    /// </summary>
    [SaveObject("subject_specialization")]
    public SubjectSpecialization? Specialization { get;set; }
}






