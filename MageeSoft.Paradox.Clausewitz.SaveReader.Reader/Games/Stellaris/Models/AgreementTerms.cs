using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents the terms of an agreement in the game state.
/// </summary>
public class AgreementTerms
{
    /// <summary>
    /// Gets or sets the discrete terms.
    /// </summary>
    public ImmutableDictionary<string, string> DiscreteTerms { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets or sets the resource terms.
    /// </summary>
    public ImmutableDictionary<string, float> ResourceTerms { get; init; } = ImmutableDictionary<string, float>.Empty;

    /// <summary>
    /// Gets or sets whether the subject can be integrated.
    /// </summary>
    [SaveName("can_subject_be_integrated")]
    public bool CanSubjectBeIntegrated { get; init; }

    /// <summary>
    /// Gets or sets whether the subject can do diplomacy.
    /// </summary>
    [SaveName("can_subject_do_diplomacy")]
    public bool CanSubjectDoDiplomacy { get; init; }

    /// <summary>
    /// Gets or sets whether the subject can vote.
    /// </summary>
    [SaveName("can_subject_vote")]
    public bool CanSubjectVote { get; init; }

    /// <summary>
    /// Gets or sets whether there is a cooldown on first renegotiation.
    /// </summary>
    [SaveName("has_cooldown_on_first_renegotiation")]
    public bool HasCooldownOnFirstRenegotiation { get; init; }

    /// <summary>
    /// Gets or sets whether the agreement has access.
    /// </summary>
    [SaveName("has_access")]
    public bool HasAccess { get; init; }

    /// <summary>
    /// Gets or sets whether the agreement has sensors.
    /// </summary>
    [SaveName("has_sensors")]
    public bool HasSensors { get; init; }

    /// <summary>
    /// Gets or sets how the subject joins overlord wars.
    /// </summary>
    [SaveName("joins_overlord_wars")]
    public string JoinsOverlordWars { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets how the subject calls overlord to war.
    /// </summary>
    [SaveName("calls_overlord_to_war")]
    public string CallsOverlordToWar { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the subject expansion type.
    /// </summary>
    [SaveName("subject_expansion_type")]
    public string SubjectExpansionType { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the agreement preset.
    /// </summary>
    [SaveName("agreement_preset")]
    public string AgreementPreset { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the forced initial loyalty.
    /// </summary>
    [SaveName("forced_initial_loyalty")]
    public int ForcedInitialLoyalty { get; init; }
} 






