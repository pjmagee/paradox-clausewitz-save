namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

[SaveModel]
public partial class Country
{
     [SaveObject("flag")]
     public CountryFlag ? Flag { get; set; }

     [SaveScalar("color_index")] 
     public int? ColorIndex { get; set; }
     
     [SaveObject("budget")]
     public CountryBudget ? Budget { get; set; }
     
     [SaveObject("events")]
     public CountryEvents? Events { get; set; }
     
     [SaveScalar("track_all_situations")]
     public bool? TrackAllSituations { get; set; }
     
     [SaveObject("modules")]
     public CountryModules? Modules { get; set; }
}


public partial class CountryEvents
{
    
}

public partial class CountryBudget
{
    
}

[SaveModel]
public partial class CountryModules
{
    [SaveObject("standard_event_module")] 
    public StandardEventModule? StandardEventModule { get; set; }
    
    [SaveObject("standard_economy_module")]
    public StandardEconomyModule? StandardEconomyModule { get; set; }
}

[SaveModel]
public partial class StandardEconomyModule
{
    [SaveObject("resources")]
    public CountryResources? Resources { get; set; }
}

[SaveModel]
public partial class CountryResources
{
    [SaveScalar("energy")]
    public int ? Energy { get; set; }
    
    [SaveScalar("minerals")]
    public int ? Minerals { get; set; }
    
    [SaveScalar("food")]
    public int ? Food { get; set; }
    
    [SaveScalar("physics_research")]
    public float ? PhysicsResearch { get; set; }
    
    [SaveScalar("society_research")]
    public float ? SocietyResearch { get; set; }
    
    [SaveScalar("engineering_research")]
    public float ? EngineeringResearch { get; set; }
    
    [SaveScalar("influence")]
    public int ? Influence { get; set; }
    
    [SaveScalar("unity")]
    public float ? Unity { get; set; }
    
    [SaveScalar("consumer_goods")]
    public float ? ConsumerGoods { get; set; }
    
    [SaveScalar("alloys")]
    public float ? Alloys { get; set; }
    
    [SaveScalar("exotic_gasses")]
    public float ? ExoticGasses { get; set; }
    
    [SaveScalar("minor_artifacts")]
    public int ? MinorArtifacts { get; set; }
}

[SaveModel]
public partial class StandardEventModule
{
    [SaveObject("delayed_event")]
    public DelayedEvent? DelayedEvent { get; set; }
}

[SaveModel]
public partial class DelayedEvent
{
    [SaveScalar("event")]
    public string? EventName { get; set; }
    
    [SaveScalar("days")]
    public int? Days { get; set; }
}

[SaveModel]
public partial class EventScope
{
    [SaveScalar("type")]
    public string? Type { get; set; }
    [SaveScalar("id")]
    public int? Id { get; set; }
    
    [SaveScalar("opener_id")]
    public int? OpenerId { get; set; }
}

[SaveModel]
public partial class CountryFlag
{
    [SaveObject("icon")]
    public FlagIcon? Icon { get; set; }
    
    [SaveObject("background")]
    public FlagBackground? Background { get; set; }
    
    [SaveArray("colors")]
    public string[]? Colors { get; set; }
}

[SaveModel]
public partial class FlagBackground
{
    [SaveScalar("category")]
    public string? Category { get; set; }
    
    [SaveScalar("file")]
    public string? File { get; set; }
}

[SaveModel]
public partial class FlagIcon
{
    [SaveScalar("category")]
    public string? Category { get; set; }
    
    [SaveScalar("file")]
    public string? File { get; set; }
}