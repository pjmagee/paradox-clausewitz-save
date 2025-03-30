using System.CommandLine;

namespace MageeSoft.PDX.CE.Cli.Commands.Options;

/// <summary>
/// Option to specify output format
/// </summary>
public class FormatOption : Option<string>
{
    public FormatOption() 
        : base(
            aliases: ["--format", "-f"],
            description: "Output format (json or text)")
    {
        SetDefaultValue("text");
    }
} 