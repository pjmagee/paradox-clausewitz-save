using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents the terms of an agreement in the game state.
/// </summary>
[SaveModel]
public partial class AgreementTerms
{
    /// <summary>
    /// Gets or sets the discrete terms.
    /// </summary>
    public Dictionary<string, string>? DiscreteTerms { get;set; }

    /// <summary>
    /// Gets or sets the resource terms.
    /// </summary>
    public Dictionary<string, float>? ResourceTerms { get;set; }

    /// <summary>
    /// Gets or sets whether the subject can be integrated.
    /// </summary>
    [SaveScalar("can_subject_be_integrated")]
    public bool? CanSubjectBeIntegrated { get;set; }

    /// <summary>
    /// Gets or sets whether the subject can do diplomacy.
    /// </summary>
    [SaveScalar("can_subject_do_diplomacy")]
    public bool? CanSubjectDoDiplomacy { get;set; }

    /// <summary>
    /// Gets or sets whether the subject can vote.
    /// </summary>
    [SaveScalar("can_subject_vote")]
    public bool? CanSubjectVote { get;set; }

    /// <summary>
    /// Gets or sets whether there is a cooldown on first renegotiation.
    /// </summary>
    [SaveScalar("has_cooldown_on_first_renegotiation")]
    public bool? HasCooldownOnFirstRenegotiation { get;set; }

    /// <summary>
    /// Gets or sets whether the agreement has access.
    /// </summary>
    [SaveScalar("has_access")]
    public bool? HasAccess { get;set; }

    /// <summary>
    /// Gets or sets whether the agreement has sensors.
    /// </summary>
    [SaveScalar("has_sensors")]
    public bool? HasSensors { get;set; }

    /// <summary>
    /// Gets or sets how the subject joins overlord wars.
    /// </summary>
    [SaveScalar("joins_overlord_wars")]
    public string? JoinsOverlordWars { get;set; }

    /// <summary>
    /// Gets or sets how the subject calls overlord to war.
    /// </summary>
    [SaveScalar("calls_overlord_to_war")]
    public string? CallsOverlordToWar { get;set; }

    /// <summary>
    /// Gets or sets the subject expansion type.
    /// </summary>
    [SaveScalar("subject_expansion_type")]
    public string? SubjectExpansionType { get;set; }

    /// <summary>
    /// Gets or sets the agreement preset.
    /// </summary>
    [SaveScalar("agreement_preset")]
    public string? AgreementPreset { get;set; }

    /// <summary>
    /// Gets or sets the forced initial loyalty.
    /// </summary>
    [SaveScalar("forced_initial_loyalty")]
    public int? ForcedInitialLoyalty { get;set; }
} 






