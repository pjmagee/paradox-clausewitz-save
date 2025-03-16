using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

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
    /// Gets or sets the owner country ID.
    /// </summary>
    [SaveName("owner")]
    public long Owner { get; init; }

    /// <summary>
    /// Gets or sets the target country ID.
    /// </summary>
    [SaveName("target")]
    public long Target { get; init; }

    /// <summary>
    /// Gets or sets the active status.
    /// </summary>
    [SaveName("active_status")]
    public string ActiveStatus { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the date added.
    /// </summary>
    [SaveName("date_added")]
    public DateOnly DateAdded { get; init; }

    /// <summary>
    /// Gets or sets the date changed.
    /// </summary>
    [SaveName("date_changed")]
    public DateOnly DateChanged { get; init; }

    /// <summary>
    /// Gets or sets the agreement terms.
    /// </summary>
    [SaveName("term_data")]
    public AgreementTerms Terms { get; init; }

    /// <summary>
    /// Gets or sets the subject specialization.
    /// </summary>
    [SaveName("subject_specialization")]
    public SubjectSpecialization Specialization { get; init; } = SubjectSpecialization.Default;
}






