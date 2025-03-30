using System.CommandLine;

namespace MageeSoft.PDX.CE.Cli.Commands.Options;

/// <summary>
/// Option to specify output file
/// </summary>
public class OutputOption : Option<FileInfo?>
{
    public OutputOption() 
        : base(
            aliases: ["--output", "-o"],
            description: "Output file path")
    {
    }
} 