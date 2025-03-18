using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Reader.Games.Stellaris;

public class Meta
{
    [SaveScalar("version")]
    public string Version { get; set; }

    [SaveScalar("version_control_revision")]
    public int VersionControlRevision { get; set; }
    
    [SaveScalar("name")]
    public string Name { get; set; }
    
    [SaveScalar("player_portrait")]
    public string PlayerPortrait { get; set; } = string.Empty;
    
	[SaveObject("flag")]
    public MetaFlag Flag { get; set; }
    
    [SaveScalar("meta_fleets")]
    public int MetaFleets { get; set; }
    
    [SaveScalar("meta_planets")]
    public int MetaPlanets { get; set; }
    
    [SaveScalar("ironman")]
    public bool IsIronman { get; set; }
}

public class MetaFlag
{
	[SaveObject("icon")]
	public MetaIcon Icon { get; set; }

	[SaveObject("background")]
	public MetaBackground Background { get; set; }

	[SaveArray("colors")]
	public List<string> Colors { get; set; } = new();
}

public class MetaBackground
{
	[SaveScalar("category")]
	public string Category { get; set; } = string.Empty;

	[SaveScalar("file")]
	public string File { get; set; } = string.Empty;
}
	
public class MetaIcon
{
	[SaveScalar("category")]
	public string Category { get; set; } = string.Empty;

	[SaveScalar("file")]
	public string File { get; set; } = string.Empty;
}