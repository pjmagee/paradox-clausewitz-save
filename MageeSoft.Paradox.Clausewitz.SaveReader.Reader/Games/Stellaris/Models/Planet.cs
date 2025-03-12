using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a planet in the game state.
/// </summary>
public class Planet
{
    /// <summary>
    /// Gets or sets the planet ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the planet.
    /// </summary>
    public LocalizedText Name { get; set; } = new();

    /// <summary>
    /// Gets or sets the planet class.
    /// </summary>
    public string PlanetClass { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the coordinate of the planet.
    /// </summary>
    public Coordinate Coordinate { get; set; } = new();

    /// <summary>
    /// Gets or sets the orbit value.
    /// </summary>
    public float Orbit { get; set; }

    /// <summary>
    /// Gets or sets the size of the planet.
    /// </summary>
    public int PlanetSize { get; set; }

    /// <summary>
    /// Gets or sets the bombardment damage.
    /// </summary>
    public int BombardmentDamage { get; set; }

    /// <summary>
    /// Gets or sets the last bombardment date.
    /// </summary>
    public string LastBombardment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the planet has automated development.
    /// </summary>
    public bool AutomatedDevelopment { get; set; }

    /// <summary>
    /// Gets or sets the owner of the planet.
    /// </summary>
    public long Owner { get; set; }

    /// <summary>
    /// Gets or sets the original owner of the planet.
    /// </summary>
    public long OriginalOwner { get; set; }

    /// <summary>
    /// Gets or sets the controller of the planet.
    /// </summary>
    public long Controller { get; set; }

    /// <summary>
    /// Gets or sets the kill pop date.
    /// </summary>
    public string KillPop { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the build queue.
    /// </summary>
    public int BuildQueue { get; set; }

    /// <summary>
    /// Gets or sets the army build queue.
    /// </summary>
    public int ArmyBuildQueue { get; set; }

    /// <summary>
    /// Gets or sets the planet orbitals.
    /// </summary>
    public Dictionary<string, SaveElement> PlanetOrbitals { get; set; } = new();

    /// <summary>
    /// Gets or sets the shipclass orbital station.
    /// </summary>
    public long ShipclassOrbitalStation { get; set; }

    /// <summary>
    /// Gets or sets the orbital defence.
    /// </summary>
    public int OrbitalDefence { get; set; }

    /// <summary>
    /// Gets or sets the flags.
    /// </summary>
    public Dictionary<string, long> Flags { get; set; } = new();

    /// <summary>
    /// Gets or sets the entity.
    /// </summary>
    public int Entity { get; set; }

    /// <summary>
    /// Gets or sets the entity name.
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the entity is explicit.
    /// </summary>
    public bool ExplicitEntity { get; set; }

    /// <summary>
    /// Gets or sets whether to prevent anomaly.
    /// </summary>
    public bool PreventAnomaly { get; set; }

    /// <summary>
    /// Gets or sets the atmosphere color.
    /// </summary>
    public List<float> AtmosphereColor { get; set; } = new();

    /// <summary>
    /// Gets or sets the atmosphere intensity.
    /// </summary>
    public float AtmosphereIntensity { get; set; }

    /// <summary>
    /// Gets or sets the atmosphere width.
    /// </summary>
    public float AtmosphereWidth { get; set; }

    /// <summary>
    /// Gets or sets the deposits.
    /// </summary>
    public List<int> Deposits { get; set; } = new();

    /// <summary>
    /// Gets or sets the favorite jobs.
    /// </summary>
    public Dictionary<string, SaveElement> FavoriteJobs { get; set; } = new();

    /// <summary>
    /// Gets or sets the stability.
    /// </summary>
    public int Stability { get; set; }

    /// <summary>
    /// Gets or sets the migration value.
    /// </summary>
    public float Migration { get; set; }

    /// <summary>
    /// Gets or sets the crime value.
    /// </summary>
    public float Crime { get; set; }

    /// <summary>
    /// Gets or sets the amenities value.
    /// </summary>
    public int Amenities { get; set; }

    /// <summary>
    /// Gets or sets the amenities usage.
    /// </summary>
    public int AmenitiesUsage { get; set; }

    /// <summary>
    /// Gets or sets the free amenities.
    /// </summary>
    public int FreeAmenities { get; set; }

    /// <summary>
    /// Gets or sets the free housing.
    /// </summary>
    public int FreeHousing { get; set; }

    /// <summary>
    /// Gets or sets the total housing.
    /// </summary>
    public int TotalHousing { get; set; }

    /// <summary>
    /// Gets or sets the housing usage.
    /// </summary>
    public int HousingUsage { get; set; }

    /// <summary>
    /// Gets or sets the employable pops.
    /// </summary>
    public int EmployablePops { get; set; }

    /// <summary>
    /// Gets or sets the number of sapient pops.
    /// </summary>
    public int NumSapientPops { get; set; }

    /// <summary>
    /// Gets or sets whether to recalculate pops.
    /// </summary>
    public bool RecalcPops { get; set; }

    /// <summary>
    /// Gets or sets the manual designation changed date.
    /// </summary>
    public string ManualDesignationChangedDate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the final designation.
    /// </summary>
    public string FinalDesignation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ascension tier.
    /// </summary>
    public int AscensionTier { get; set; }

    /// <summary>
    /// Gets or sets the auto slots taken.
    /// </summary>
    public List<bool> AutoSlotsTaken { get; set; } = new();

    /// <summary>
    /// Gets or sets the last auto mod index.
    /// </summary>
    public int LastAutoModIndex { get; set; }

    /// <summary>
    /// Gets or sets the species refs.
    /// </summary>
    public List<int> SpeciesRefs { get; set; } = new();

    /// <summary>
    /// Gets or sets the species information.
    /// </summary>
    public Dictionary<string, Dictionary<string, int>> SpeciesInformation { get; set; } = new();

    /// <summary>
    /// Gets or sets the moon of ID.
    /// </summary>
    public int? MoonOf { get; set; }

    /// <summary>
    /// Gets or sets the colonize date.
    /// </summary>
    public DateOnly? ColonizeDate { get; set; }

    /// <summary>
    /// Gets or sets whether the planet is a moon.
    /// </summary>
    public bool IsMoon { get; set; }

    /// <summary>
    /// Gets or sets whether this planet has a ring.
    /// </summary>
    public bool HasRing { get; set; }

    /// <summary>
    /// Gets or sets the list of moons.
    /// </summary>
    public List<int> Moons { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of pops.
    /// </summary>
    public List<int> Pops { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of buildings.
    /// </summary>
    public List<int> Buildings { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of districts.
    /// </summary>
    public List<string> Districts { get; set; } = new();

    /// <summary>
    /// Gets or sets the last building changed.
    /// </summary>
    public string LastBuildingChanged { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last district changed.
    /// </summary>
    public string LastDistrictChanged { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the planet modifier.
    /// </summary>
    public string PlanetModifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timed modifiers.
    /// </summary>
    public Dictionary<string, SaveElement> TimedModifiers { get; set; } = new();

    /// <summary>
    /// Gets or sets the army list.
    /// </summary>
    public List<int> Army { get; set; } = new();

    /// <summary>
    /// Gets or sets the pop to kill from devastation.
    /// </summary>
    public int PopToKillFromDevastation { get; set; }

    /// <summary>
    /// Gets or sets the planet automation settings.
    /// </summary>
    public List<string> PlanetAutomationSettings { get; set; } = new();

    /// <summary>
    /// Gets or sets the job priorities.
    /// </summary>
    public Dictionary<string, int> JobPriorities { get; set; } = new();

    /// <summary>
    /// Gets or sets the jobs cache.
    /// </summary>
    public List<JobCache> JobsCache { get; set; } = new();

    /// <summary>
    /// Loads all planets from the game state.
    /// </summary>
    /// <param name="root">The game state root object to load from.</param>
    /// <returns>An immutable array of planets.</returns>
    public static ImmutableArray<Planet> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Planet>();
        var planetsElement = root.Properties
            .FirstOrDefault(p => p.Key == "planets");

        var planetsObj = planetsElement.Value as SaveObject;
        if (planetsObj != null)
        {
            foreach (var planetElement in planetsObj.Properties)
            {
                if (long.TryParse(planetElement.Key, out var planetId))
                {
                    var obj = planetElement.Value as SaveObject;
                    if (obj == null)
                    {
                        continue;
                    }

                    var planet = new Planet { Id = planetId };

                    foreach (var property in obj.Properties)
                    {
                        switch (property.Key)
                        {
                            case "name" when property.Value is SaveObject nameObj:
                                planet.Name = LocalizedText.Load(nameObj);
                                break;
                            case "planet_class" when property.Value?.TryGetScalar<string>(out var value) == true:
                                planet.PlanetClass = value;
                                break;
                            case "coordinate" when property.Value is SaveObject coordObj:
                                planet.Coordinate = Coordinate.Load(coordObj);
                                break;
                            case "orbit" when property.Value?.TryGetScalar<float>(out var value) == true:
                                planet.Orbit = value;
                                break;
                            case "orbit" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.Orbit = value;
                                break;
                            case "planet_size" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.PlanetSize = value;
                                break;
                            case "bombardment_damage" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.BombardmentDamage = value;
                                break;
                            case "last_bombardment" when property.Value?.TryGetScalar<string>(out var value) == true:
                                planet.LastBombardment = value;
                                break;
                            case "automated_development" when property.Value?.TryGetScalar<bool>(out var value) == true:
                                planet.AutomatedDevelopment = value;
                                break;
                            case "owner" when property.Value?.TryGetScalar<long>(out var value) == true:
                                planet.Owner = value;
                                break;
                            case "original_owner" when property.Value?.TryGetScalar<long>(out var value) == true:
                                planet.OriginalOwner = value;
                                break;
                            case "controller" when property.Value?.TryGetScalar<long>(out var value) == true:
                                planet.Controller = value;
                                break;
                            case "kill_pop" when property.Value?.TryGetScalar<string>(out var value) == true:
                                planet.KillPop = value;
                                break;
                            case "build_queue" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.BuildQueue = value;
                                break;
                            case "army_build_queue" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.ArmyBuildQueue = value;
                                break;
                            case "planet_orbitals" when property.Value is SaveObject orbitalsObj:
                                foreach (var orbital in orbitalsObj.Properties)
                                {
                                    planet.PlanetOrbitals[orbital.Key] = orbital.Value;
                                }
                                break;
                            case "shipclass_orbital_station" when property.Value?.TryGetScalar<long>(out var value) == true:
                                planet.ShipclassOrbitalStation = value;
                                break;
                            case "orbital_defence" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.OrbitalDefence = value;
                                break;
                            case "flags" when property.Value is SaveObject flagsObj:
                                foreach (var flag in flagsObj.Properties)
                                {
                                    if (flag.Value?.TryGetScalar<long>(out var value) == true)
                                    {
                                        planet.Flags[flag.Key] = value;
                                    }
                                }
                                break;
                            case "entity" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.Entity = value;
                                break;
                            case "entity_name" when property.Value?.TryGetScalar<string>(out var value) == true:
                                planet.EntityName = value;
                                break;
                            case "explicit_entity" when property.Value?.TryGetScalar<bool>(out var value) == true:
                                planet.ExplicitEntity = value;
                                break;
                            case "prevent_anomaly" when property.Value?.TryGetScalar<bool>(out var value) == true:
                                planet.PreventAnomaly = value;
                                break;
                            case "atmosphere_color" when property.Value is SaveArray colorArray:
                                foreach (var element in colorArray.Items)
                                {
                                    if (element.TryGetScalar<float>(out var value))
                                    {
                                        planet.AtmosphereColor.Add(value);
                                    }
                                }
                                break;
                            case "atmosphere_intensity" when property.Value?.TryGetScalar<float>(out var value) == true:
                                planet.AtmosphereIntensity = value;
                                break;
                            case "atmosphere_width" when property.Value?.TryGetScalar<float>(out var value) == true:
                                planet.AtmosphereWidth = value;
                                break;
                            case "deposits" when property.Value is SaveArray depositsArray:
                                foreach (var element in depositsArray.Items)
                                {
                                    if (element.TryGetScalar<int>(out var value))
                                    {
                                        planet.Deposits.Add(value);
                                    }
                                }
                                break;
                            case "favorite_jobs" when property.Value is SaveObject jobsObj:
                                foreach (var job in jobsObj.Properties)
                                {
                                    planet.FavoriteJobs[job.Key] = job.Value;
                                }
                                break;
                            case "stability" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.Stability = value;
                                break;
                            case "migration" when property.Value?.TryGetScalar<float>(out var value) == true:
                                planet.Migration = value;
                                break;
                            case "crime" when property.Value?.TryGetScalar<float>(out var value) == true:
                                planet.Crime = value;
                                break;
                            case "amenities" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.Amenities = value;
                                break;
                            case "amenities_usage" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.AmenitiesUsage = value;
                                break;
                            case "free_amenities" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.FreeAmenities = value;
                                break;
                            case "free_housing" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.FreeHousing = value;
                                break;
                            case "total_housing" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.TotalHousing = value;
                                break;
                            case "housing_usage" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.HousingUsage = value;
                                break;
                            case "employable_pops" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.EmployablePops = value;
                                break;
                            case "num_sapient_pops" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.NumSapientPops = value;
                                break;
                            case "recalc_pops" when property.Value?.TryGetScalar<bool>(out var value) == true:
                                planet.RecalcPops = value;
                                break;
                            case "manual_designation_changed_date" when property.Value?.TryGetScalar<string>(out var value) == true:
                                planet.ManualDesignationChangedDate = value;
                                break;
                            case "final_designation" when property.Value?.TryGetScalar<string>(out var value) == true:
                                planet.FinalDesignation = value;
                                break;
                            case "ascension_tier" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.AscensionTier = value;
                                break;
                            case "auto_slots_taken" when property.Value is SaveArray slotsArray:
                                foreach (var element in slotsArray.Items)
                                {
                                    if (element.TryGetScalar<bool>(out var value))
                                    {
                                        planet.AutoSlotsTaken.Add(value);
                                    }
                                }
                                break;
                            case "last_auto_mod_index" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.LastAutoModIndex = value;
                                break;
                            case "species_refs" when property.Value is SaveArray refsArray:
                                foreach (var element in refsArray.Items)
                                {
                                    if (element.TryGetScalar<int>(out var value))
                                    {
                                        planet.SpeciesRefs.Add(value);
                                    }
                                }
                                break;
                            case "species_information" when property.Value is SaveObject infoObj:
                                foreach (var info in infoObj.Properties)
                                {
                                    if (info.Value is SaveObject speciesObj)
                                    {
                                        var speciesInfo = new Dictionary<string, int>();
                                        foreach (var speciesProperty in speciesObj.Properties)
                                        {
                                            if (speciesProperty.Value?.TryGetScalar<int>(out var value) == true)
                                            {
                                                speciesInfo[speciesProperty.Key] = value;
                                            }
                                        }
                                        planet.SpeciesInformation[info.Key] = speciesInfo;
                                    }
                                }
                                break;
                            case "moon_of" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.MoonOf = value;
                                break;
                            case "colonize_date" when property.Value?.TryGetScalar<DateOnly>(out var value) == true:
                                planet.ColonizeDate = value;
                                break;
                            case "is_moon" when property.Value?.TryGetScalar<bool>(out var value) == true:
                                planet.IsMoon = value;
                                break;
                            case "has_ring" when property.Value?.TryGetScalar<bool>(out var value) == true:
                                planet.HasRing = value;
                                break;
                            case "moons" when property.Value is SaveArray moonsArray:
                                foreach (var element in moonsArray.Items)
                                {
                                    if (element.TryGetScalar<int>(out var value))
                                    {
                                        planet.Moons.Add(value);
                                    }
                                }
                                break;
                            case "pops" when property.Value is SaveArray popsArray:
                                foreach (var element in popsArray.Items)
                                {
                                    if (element.TryGetScalar<int>(out var value))
                                    {
                                        planet.Pops.Add(value);
                                    }
                                }
                                break;
                            case "buildings" when property.Value is SaveArray buildingsArray:
                                foreach (var element in buildingsArray.Items)
                                {
                                    if (element.TryGetScalar<int>(out var value))
                                    {
                                        planet.Buildings.Add(value);
                                    }
                                }
                                break;
                            case "district" when property.Value?.TryGetScalar<string>(out var value) == true:
                                planet.Districts.Add(value);
                                break;
                            case "last_building_changed" when property.Value?.TryGetScalar<string>(out var value) == true:
                                planet.LastBuildingChanged = value;
                                break;
                            case "last_district_changed" when property.Value?.TryGetScalar<string>(out var value) == true:
                                planet.LastDistrictChanged = value;
                                break;
                            case "planet_modifier" when property.Value?.TryGetScalar<string>(out var value) == true:
                                planet.PlanetModifier = value;
                                break;
                            case "timed_modifiers" when property.Value is SaveObject modifiersObj:
                                foreach (var modifier in modifiersObj.Properties)
                                {
                                    planet.TimedModifiers[modifier.Key] = modifier.Value;
                                }
                                break;
                            case "army" when property.Value is SaveArray armyArray:
                                foreach (var element in armyArray.Items)
                                {
                                    if (element.TryGetScalar<int>(out var value))
                                    {
                                        planet.Army.Add(value);
                                    }
                                }
                                break;
                            case "pop_to_kill_from_devastation" when property.Value?.TryGetScalar<int>(out var value) == true:
                                planet.PopToKillFromDevastation = value;
                                break;
                            case "planet_automation_settings" when property.Value is SaveArray settingsArray:
                                foreach (var element in settingsArray.Items)
                                {
                                    if (element.TryGetScalar<string>(out var value))
                                    {
                                        planet.PlanetAutomationSettings.Add(value);
                                    }
                                }
                                break;
                            case "job_priorities" when property.Value is SaveObject prioritiesObj:
                                foreach (var priority in prioritiesObj.Properties)
                                {
                                    if (priority.Value?.TryGetScalar<int>(out var value) == true)
                                    {
                                        planet.JobPriorities[priority.Key] = value;
                                    }
                                }
                                break;
                            case "jobs_cache" when property.Value is SaveArray cacheArray:
                                foreach (var element in cacheArray.Items)
                                {
                                    if (element is SaveObject cacheObj)
                                    {
                                        var cache = new JobCache();
                                        foreach (var cacheProperty in cacheObj.Properties)
                                        {
                                            switch (cacheProperty.Key)
                                            {
                                                case "job" when cacheProperty.Value?.TryGetScalar<string>(out var value) == true:
                                                    cache.Job = value;
                                                    break;
                                                case "count" when cacheProperty.Value?.TryGetScalar<int>(out var value) == true:
                                                    cache.Count = value;
                                                    break;
                                            }
                                        }
                                        planet.JobsCache.Add(cache);
                                    }
                                }
                                break;
                        }
                    }

                    builder.Add(planet);
                }
            }
        }

        return builder.ToImmutable();
    }
}

public class JobCache
{
    public string Job { get; set; }
    public int Count { get; set; }
} 