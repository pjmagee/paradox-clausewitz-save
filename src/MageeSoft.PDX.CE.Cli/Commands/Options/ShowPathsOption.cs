using System.CommandLine;

namespace MageeSoft.PDX.CE.Cli.Commands.Options;

public class ShowPathsOption : Option<bool>
{
    public ShowPathsOption() : base(
        aliases: ["--show-paths", "-p"],
        description: "Show the full path of each node in the result")
    {
        SetDefaultValue(false);
    }
}