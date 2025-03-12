using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents fleet statistics in the game state.
/// </summary>
public class FleetStats
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
    /// Loads fleet statistics from a SaveElement.
    /// </summary>
    /// <param name="clausewitzElement">The SaveElement containing the fleet statistics data.</param>
    /// <returns>A new FleetStats instance.</returns>
    public static FleetStats Load(SaveElement clausewitzElement)
    {
        var stats = new FleetStats();
        var statsObj = clausewitzElement as SaveObject;
        if (statsObj != null)
        {
            foreach (var property in statsObj.Properties)
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
        }

        return stats;
    }
}