using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using MageeSoft.PDX.CE;

namespace MageeSoft.PDX.CE.Models;

/// <summary>
/// Gamestate model for Paradox game state data.
/// All nested classes and binding logic will be generated from the schema file.
/// </summary>
[GameStateDocument("gamestate.csf")]
public partial class Gamestate
{
	// // The object this model is bound from
 //    public SaveObject SaveObject { get; set; } = null!;
 //    
 //    // version="Circinus v3.14.15926"
 //    public string? Version { get; set; }
 //    
 //    // version_control_revision=19
 //    public int? VersionControlRevision { get; set; }
 //    
 //    // name="United Nations of Earth"
 //    public string? Name { get; set; }
 //    
 //    // date="2250.11.15"      
 //    public DateTime? Date { get; set; }
 //    
 //    // required_dlcs= {  "Ancient Relics Story Pack" "Anniversary Portraits" }
 //    public List<string>? RequiredDlcs { get; set; }
 //    
 //    // player= { { name="player 1" country=0 } { name="Player 2" country=1 } }
 //    public List<GamestatePlayer>? Player { get; set; }
 //    
 //    /*
 //      species_db=
	//    {
	//    		0={
	// 			...
	// 		}
	// 		1={
	// 			...
	// 			extra_trait_points=0
	// 			home_planet=3
	// 			gender=not_set
	// 		}
	// 		2=
	// 		{
	// 			...
	// 		}
	// 	}
	// 	* /
 //     */
 //    public Dictionary<int, GamestatePlayerSpeciesDb>? SpeciesDb { get; set; }
 //    
 //    /*
 //     * traits=
 //    {
 //        trait="trait_hive_mind"
 //        trait="trait_pc_arid_preference"
 //        trait="trait_traditional"
 //        trait="trait_quick_learners"
 //        trait="trait_solitary"
 //        trait="trait_natural_physicists"
 //    }
 //     */
 //
 //
 //    /*
 //     * save_array = {
 //     *   {
 //     *       12345 // first item in array = ID of the following item
 //     *       { example_property = "example value" } // 2nd item in array = SaveObject with properties
 //     *   }
 //     *   {
 //     *		67890 // first item in array = ID of the following item
 //     *		{ example_property = "another example value" }
 //     *	 }
 //     *
 //     *   ...
 //     * }
 //     */
 //    public Dictionary<int, GameStateObject>? SaveArray { get; set; }
 //    
 //    public class GameStateObject
 //    {
	//     public string? ExampleProperty { get; set; }
	//     
	//     public static GameStateObject Load(SaveObject saveObject)
	//     {
	// 	    var model = new GameStateObject();
	// 	    if (saveObject.TryGetString("example_property", out string? exampleProperty)) model.ExampleProperty = exampleProperty;
	// 	    return model;
	//     }
 //    }
 //    
 //    public GamestateTraits? Traits { get; set; }
 //    
 //    public class GamestateTraits
	// {
	// 	public List<string>? Trait { get; set; }
	// 	
	// 	public static GamestateTraits Bind(SaveObject saveObject)
	// 	{
	// 		var model = new GamestateTraits();
	// 		model.Trait = new List<string>();
 //
	// 		foreach (var item in saveObject.Properties)
	// 		{
	// 			if (item.Key == "trait" && item.Value is Scalar<string> scalar)
	// 			{
	// 				model.Trait.Add(scalar.Value);
	// 			}
	// 		}
	// 		
	// 		return model;
	// 	}
	// }
 //
 //    public static Gamestate Load(SaveObject saveObject)
 //    {
 //        var model = new Gamestate();
 //        model.SaveObject = saveObject;
 //        
 //        if (saveObject.TryGetString("version", out string? version)) 
	//         model.Version = version;
 //        
 //        if (saveObject.TryGetString("name", out string? name)) 
	//         model.Name = name;
 //        
 //        if (saveObject.TryGetDateTime("date", out DateTime? date)) 
	//         model.Date = date;
 //        
 //        if (saveObject.TryGetInt("version_control_revision", out int? versionControlRevision)) 
	//         model.VersionControlRevision = versionControlRevision;
 //        
 //        // List because each item was a string: required_dlcs= {  "Ancient Relics Story Pack" "Anniversary Portraits" }
 //        if (saveObject.TryGetStrings("required_dlcs", out List<string>? requiredDlcs)) 
	//         model.RequiredDlcs = requiredDlcs!;
 //        
 //        // List because each item was a player object: player= { { name="player 1" country=0 } { name="Player 2" country=1 } }
 //        if (saveObject.TryGetSaveArray("player", out SaveArray player))
	//         model.Player = new List<GamestatePlayer>(player.Items.OfType<SaveObject>().Select(GamestatePlayer.Load));
 //        
 //        // Dictionary because each item key was an int: species_db={ 0={} 1={} 2={} }
 //        if (saveObject.TryGetSaveObject("species_db", out SaveObject? speciesDb))
	//         model.SpeciesDb = speciesDb!.Properties.ToDictionary(
	// 	        keySelector: kv => int.Parse(kv.Key),
	// 	        elementSelector: kv => GamestatePlayerSpeciesDb.Load((SaveObject)kv.Value)
	//         );
 //        
	//     // Object, because it's a single object of duplicate keys: traits={ trait="trait_hive_mind" trait="trait_pc_arid_preference" trait="trait_traditional" trait="trait_quick_learners" trait="trait_solitary" trait="trait_natural_physicists" }
	//     if (saveObject.TryGetSaveObject("traits", out SaveObject? traits))
	// 	    model.Traits = GamestateTraits.Bind(traits!);
	// 	
	//     // Unique ID/Object pair of array items: save_array = { { 12345 { example_property = "example value" } } { 67890 { example_property = "another example value" } } }
	//     if (saveObject.TryGetSaveArray("save_array", out SaveArray saveArray))
	// 	    model.SaveArray = saveArray.Items.OfType<SaveArray>().ToDictionary(
	// 		    keySelector: sa => ((Scalar<int>)sa.Items[0]).Value,
	// 		    elementSelector: sa => GameStateObject.Load((SaveObject)sa.Items[1])
	// 	    );
 //
 //        return model;
 //    }
 //    
 //    // player= { { p1 ... } { p2 ... } { p3 ... } }
 //    public class GamestatePlayer
 //    {
 //        public string? Name { get; set; }
 //        public int? Country { get; set; }
 //
 //        public static GamestatePlayer Load(SaveObject saveObject)
 //        {
 //            var model = new GamestatePlayer();
 //            if (saveObject.TryGetString("name", out string? name)) model.Name = name;
 //            if (saveObject.TryGetInt("country", out int? country)) model.Country = country;
 //            return model;
 //        }
 //    }
 //
 //    // [parent-prefix]species_db
 //    // species_db={ 0={ ... } 1={ ... } 2={ ... } }
 //    public class GamestatePlayerSpeciesDb
 //    {	
	//     /* others fields ommitted for brevity of example
	//      
	//     extra_trait_points=0
	//     home_planet=3
	//     gender=not_set
	//     */
	//     
	//     public int? ExtraTraitPoints { get; set; }
	//     public int? HomePlanet { get; set; }
	//     public string? Gender { get; set; }
 //
	// 	public static GamestatePlayerSpeciesDb Load(SaveObject saveObject)
	// 	{
	// 		var model = new GamestatePlayerSpeciesDb();
	// 		if (saveObject.TryGetInt("extra_trait_points", out int? extraTraitPoints)) model.ExtraTraitPoints = extraTraitPoints;
	// 		if (saveObject.TryGetInt("home_planet", out int? homePlanet)) model.HomePlanet = homePlanet;
	// 		if (saveObject.TryGetString("gender", out string? gender)) model.Gender = gender;
	// 		return model;
	// 	}
	//     
 //    }
}