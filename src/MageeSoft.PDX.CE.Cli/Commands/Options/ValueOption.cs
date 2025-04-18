using System.CommandLine;

namespace MageeSoft.PDX.CE.Cli.Commands.Options;

/// <summary>
/// Option to specify the value to set in the save file
/// </summary>
public class ValueOption : Option<string>
{
    public ValueOption() : base(
        aliases: ["--value", "-v"],
        description: "Value to set (e.g. 42 or { x=1 })")
    {
        IsRequired = true;
    }
} 