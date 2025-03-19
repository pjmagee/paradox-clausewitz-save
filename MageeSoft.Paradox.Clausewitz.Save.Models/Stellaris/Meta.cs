namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

[SaveModel]
public partial class Meta
{
    [SaveScalar("version")]
    public string Version { get; set; } = string.Empty;

    [SaveScalar("name")]
    public string Name { get; set; } = string.Empty;

    [SaveScalar("date")]
    public DateOnly Date { get; set; }

    [SaveScalar("save_game_name")]
    public string SaveGameName { get; set; } = string.Empty;

    [SaveObject("player")]
    public Player Player { get; set; } = new();

    [SaveScalar("empire_name")]
    public string EmpireName { get; set; } = string.Empty;

    [SaveScalar("ironman")]
    public bool Ironman { get; set; }

    [SaveScalar("ironman_manager")]
    public IronmanManager IronmanManager { get; set; } = new();

    [SaveScalar("cloud_save")]
    public bool CloudSave { get; set; }

    [SaveObject("outer_text")]
    public OuterText OuterText { get; set; } = new();

    [SaveArray("achievements")]
    public int[] Achievements { get; set; } = Array.Empty<int>();

    [SaveScalar("version_control_revision")]
    public int VersionControlRevision { get; set; }
    
    [SaveScalar("player_portrait")]
    public string PlayerPortrait { get; set; } = string.Empty;
    
    [SaveObject("flag")]
    public MetaFlag Flag { get; set; } = new();
    
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
    public MetaIcon Icon { get; set; } = new();

    [SaveObject("background")]
    public MetaBackground Background { get; set; } = new();

    [SaveArray("colors")]
    public List<string> Colors { get; set; } = new();
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