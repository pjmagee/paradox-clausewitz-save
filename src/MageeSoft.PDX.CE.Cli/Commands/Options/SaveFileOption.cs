using System.CommandLine;

namespace MageeSoft.PDX.CE.Cli.Commands.Options;

/// <summary>
/// Option to specify a direct path to a save file
/// </summary>
public class SaveFileOption : Option<FileInfo?>
{
    public SaveFileOption() : base(
        aliases: ["--save-file", "-s"],
        description: "Path to a Paradox Clausewitz save file to query or modify directly")
    {
        IsRequired = false;
    }
} 