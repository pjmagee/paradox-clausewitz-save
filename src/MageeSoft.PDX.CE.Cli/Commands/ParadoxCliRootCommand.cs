using System.CommandLine;

namespace MageeSoft.PDX.CE.Cli.Commands;

/// <summary>
/// Root command for the Paradox Clausewitz Save CLI
/// </summary>
public class ParadoxCliRootCommand : RootCommand
{
    public ParadoxCliRootCommand() : base("Paradox Clausewitz Save Parser CLI - A tool for working with Paradox Interactive save files")
    {
        AddCommand(new ListCommand());
        AddCommand(new SummaryCommand());
        AddCommand(new JsonCommand());
        AddCommand(new InfoCommand());
    }
} 