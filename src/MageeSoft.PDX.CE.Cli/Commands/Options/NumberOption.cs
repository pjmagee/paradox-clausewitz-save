using System.CommandLine;

namespace MageeSoft.PDX.CE.Cli.Commands.Options;

/// <summary>
/// Option to specify save file by index number
/// </summary>
public class NumberOption : Option<int>
{
    public NumberOption() 
        : base(
            aliases: ["--number", "-n"],
            description: "The index number of the save file from the list command")
    {
    }
} 