using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StellarisSaveParser.Cli.Commands;
using StellarisSaveParser.Cli.Services;

namespace StellarisSaveParser.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Create a root command with description
        var rootCommand = new RootCommand("Stellaris Save Parser CLI - A tool for working with Stellaris save files");
        
        // Add commands
        rootCommand.AddCommand(new ListSavesCommand());
        rootCommand.AddCommand(new SummarizeCommand());
        
        // Create a version command
        var versionCommand = new Command("version", "Display the version of the tool");
        versionCommand.SetHandler(() =>
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"MageeSoft.StellarisSaveParser.Cli v{version}");
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
                        // logging.AddFile("logs/stellaris-parser-{Date}.log");
                    });
                    
                    host.ConfigureServices((context, services) =>
                    {
                        // Register services
                        services.AddSingleton<StellarisSaveService>();
                    });
                })
            .UseDefaults()
            .Build();
        
        // Parse the command line arguments and execute the command
        return await parser.InvokeAsync(args);
    }
}
