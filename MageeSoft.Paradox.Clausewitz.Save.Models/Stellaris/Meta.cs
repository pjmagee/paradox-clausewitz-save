namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

[SaveModel]
public partial class Meta
{
    [SaveScalar("version")]
    public string Version { get; set; } = string.Empty;
    
    [SaveScalar("version_control_revision")]
    public int VersionControlRevision { get; set; }

    [SaveScalar("name")]
    public string Name { get; set; } = string.Empty;

    [SaveScalar("date")]
    public DateOnly Date { get; set; }
    
    [SaveArray("required_dlcs")]
    public List<string> RequiredDlcs { get; set; } = new();
    
    [SaveScalar("ironman")]
    public bool Ironman { get; set; }
    
    [SaveScalar("player_portrait")]
    public string PlayerPortrait { get; set; } = string.Empty;
    
    [SaveObject("flag")]
    public MetaFlag Flag { get; set; }
    
    [SaveScalar("meta_fleets")]
    public int MetaFleets { get; set; }
    
    [SaveScalar("meta_planets")]
    public int MetaPlanets { get; set; }
}

[SaveModel]
public partial class Player
{
    [SaveScalar("country")]
    public int Country { get; set; }
}

[SaveModel]
public partial class IronmanManager
{
    [SaveScalar("checksum")]
    public string Checksum { get; set; } = string.Empty;

    [SaveScalar("date")]
    public DateOnly Date { get; set; }
}

[SaveModel]
public partial class OuterText
{
    [SaveScalar("tag")]
    public string Tag { get; set; } = string.Empty;

    [SaveScalar("date")]
    public DateOnly Date { get; set; }

    [SaveScalar("format_version")]
    public int FormatVersion { get; set; }
}

[SaveModel]
public partial class MetaFlag
{
    [SaveObject("icon")]
    public MetaIcon Icon { get; set; }

    [SaveObject("background")]
    public MetaBackground Background { get; set; }

    [SaveArray("colors")]
    public List<string> Colors { get; set; }
}

[SaveModel]
public partial class MetaBackground
{
    [SaveScalar("category")]
    public string Category { get; set; } = string.Empty;

    [SaveScalar("file")]
    public string File { get; set; } = string.Empty;
}
    
[SaveModel]
public partial class MetaIcon
{
    [SaveScalar("category")]
    public string Category { get; set; } = string.Empty;

    [SaveScalar("file")]
    public string File { get; set; } = string.Empty;
} 