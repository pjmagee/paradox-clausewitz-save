using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents combat statistics in the game state.
/// </summary>
public class CombatStats
{
    /// <summary>
    /// Gets or sets the damage dealt.
    /// </summary>
    public float DamageDealt { get; set; }

    /// <summary>
    /// Gets or sets the damage taken.
    /// </summary>
    public float DamageTaken { get; set; }

    /// <summary>
    /// Gets or sets the ships lost.
    /// </summary>
    public int ShipsLost { get; set; }

    /// <summary>
    /// Gets or sets the armies lost.
    /// </summary>
    public int ArmiesLost { get; set; }

    /// <summary>
    /// Loads combat statistics from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the combat statistics data.</param>
    /// <returns>A new CombatStats instance.</returns>
    public static CombatStats Load(SaveObject saveObject)
    {
        var stats = new CombatStats();

        foreach (var property in saveObject.Properties)
        {
            switch (property.Key)
            {
                case "damage_dealt" when property.Value is Scalar<float> damageDealtScalar:
                    stats.DamageDealt = damageDealtScalar.Value;
                    break;
                case "damage_taken" when property.Value is Scalar<float> damageTakenScalar:
                    stats.DamageTaken = damageTakenScalar.Value;
                    break;
                case "ships_lost" when property.Value is Scalar<int> shipsLostScalar:
                    stats.ShipsLost = shipsLostScalar.Value;
                    break;
                case "armies_lost" when property.Value is Scalar<int> armiesLostScalar:
                    stats.ArmiesLost = armiesLostScalar.Value;
                    break;
            }
        }

        return stats;
    }
}