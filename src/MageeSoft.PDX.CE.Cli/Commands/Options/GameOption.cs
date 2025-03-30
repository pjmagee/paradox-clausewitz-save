using System.CommandLine;

namespace MageeSoft.PDX.CE.Cli.Commands.Options;

/// <summary>
/// Option to specify which game to process
/// </summary>
public class GameOption : Option<string>
{
    public GameOption() 
        : base(
            aliases: ["--game", "-g"],
            description: "The game to process saves for (stellaris, eu4, hoi4, ck3, vic3)")
    {
        IsRequired = false;
    }
} 