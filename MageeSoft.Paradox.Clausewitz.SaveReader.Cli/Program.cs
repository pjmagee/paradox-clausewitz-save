using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Reflection;
using MageeSoft.Paradox.Clausewitz.SaveReader.Cli.Commands;
using MageeSoft.Paradox.Clausewitz.SaveReader.Cli.Services;
using MageeSoft.Paradox.Clausewitz.SaveReader.Cli.Services.Games.Stellaris;
using MageeSoft.Paradox.Clausewitz.SaveReader.Cli.Services.Games.CrusaderKings3;
using MageeSoft.Paradox.Clausewitz.SaveReader.Cli.Services.Games.HeartsOfIron4;
using MageeSoft.Paradox.Clausewitz.SaveReader.Cli.Services.Games.Victoria3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Create a root command with description
        var rootCommand = new RootCommand("Paradox Clausewitz Save Parser CLI - A tool for working with Paradox game save files");
        
        // Add commands
        rootCommand.AddCommand(new ListSavesCommand());
        rootCommand.AddCommand(new SummarizeCommand());
        
        // Create a version command
        var versionCommand = new Command("version", "Display the version of the tool");
        versionCommand.SetHandler(() =>
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"MageeSoft.Paradox.Clausewitz.SaveReader.Cli v{version}");
        });
        rootCommand.AddCommand(versionCommand);
        
        // Configure the command line parser
        var parser = new CommandLineBuilder(rootCommand)
            .UseHost(_ => Host.CreateDefaultBuilder(),
                host =>
                {
                    // Configure logging to not use console
                    host.ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddFilter("Microsoft", LogLevel.Warning);
                        logging.AddFilter("System", LogLevel.Warning);
                        // Add file logging if needed
                        // logging.AddFile("logs/paradox-parser-{Date}.log");
                    });
                    
                    host.ConfigureServices((context, services) =>
                    {
                        // Register game services
                        services.AddSingleton<IGameSaveService, StellarisSaveService>();
                        services.AddSingleton<IGameSaveService, CrusaderKings3SaveService>();
                        services.AddSingleton<IGameSaveService, HeartsOfIron4SaveService>();
                        services.AddSingleton<IGameSaveService, Victoria3SaveService>();
                    });
                })
            .UseDefaults()
            .Build();
        
        // Parse the command line arguments and execute the command
        return await parser.InvokeAsync(args);
    }
}
