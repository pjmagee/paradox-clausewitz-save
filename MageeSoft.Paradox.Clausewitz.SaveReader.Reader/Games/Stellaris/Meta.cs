using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;

public class Meta
{
    [SaveProperty("version")]
    public string Version { get; set; }

    [SaveProperty("version_control_revision")]
    public int VersionControlRevision { get; set; }
    
    [SaveProperty("name")]
    public string Name { get; set; }
    
    [SaveProperty("player_portrait")]
    public string PlayerPortrait { get; set; } = string.Empty;
    
	[SaveProperty("flag")]
    public MetaFlag Flag { get; set; }
    
    [SaveProperty("meta_fleets")]
    public int MetaFleets { get; set; }
    
    [SaveProperty("meta_planets")]
    public int MetaPlanets { get; set; }
    
    [SaveProperty("ironman")]
    public bool IsIronman { get; set; }
}

public class MetaFlag
{
	[SaveProperty("icon")]
	public MetaIcon Icon { get; set; }

	[SaveProperty("background")]
	public MetaBackground Background { get; set; }

	[SaveProperty("colors")]
	public List<string> Colors { get; set; } = new();
}

public class MetaBackground
{
	[SaveProperty("category")]
	public string Category { get; set; } = string.Empty;

	[SaveProperty("file")]
	public string File { get; set; } = string.Empty;
}
	
public class MetaIcon
{
	[SaveProperty("category")]
	public string Category { get; set; } = string.Empty;

	[SaveProperty("file")]
	public string File { get; set; } = string.Empty;
}