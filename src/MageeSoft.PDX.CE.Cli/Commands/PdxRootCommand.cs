using System.CommandLine;

namespace MageeSoft.PDX.CE.Cli.Commands;

/// <summary>
/// Root command for the Paradox Clausewitz Save CLI
/// </summary>
public class PdxRootCommand : RootCommand
{
    public PdxRootCommand() : base("A cli tool for interacting with PDX Clausewitz engine game saves")
    {
        AddCommand(new ListCommand());
        AddCommand(new SummaryCommand());
        AddCommand(new JsonCommand());
        AddCommand(new InfoCommand());
        AddCommand(new SetCommand());
        AddCommand(new QueryCommand());
    }
} 