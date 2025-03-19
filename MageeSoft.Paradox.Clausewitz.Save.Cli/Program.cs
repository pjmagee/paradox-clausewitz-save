using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Reflection;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Commands;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Services;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Services.Games.Stellaris;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var rootCommand = new RootCommand("Paradox Clausewitz Save Parser CLI - A tool for working with Stellaris save files");
rootCommand.AddCommand(new ListSavesCommand());
rootCommand.AddCommand(new SummarizeCommand());
        
var versionCommand = new Command("version", "Display the version of the tool");
versionCommand.SetHandler(() =>
{
    var assembly = Assembly.GetExecutingAssembly();
    var productName = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "MageeSoft.Paradox.Clausewitz.Save.Cli";
    var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";
    var buildConfig = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "Unknown";
    var isAot = System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported ? "No" : "Yes";
    
    // Parse git info from version string (if available)
    string gitInfo = string.Empty;
    if (version.Contains('+'))
    {
        var parts = version.Split('+');
        if (parts.Length > 1)
        {
            gitInfo = parts[1];
        }
    }

    Console.WriteLine($"{productName} v{version}");
    Console.WriteLine($"Build: {buildConfig}");
    Console.WriteLine($"Native AOT: {isAot}");
    
    if (!string.IsNullOrEmpty(gitInfo))
    {
        Console.WriteLine($"Git: {gitInfo}");
    }
});

rootCommand.AddCommand(versionCommand);

var parser = new CommandLineBuilder(rootCommand)
    .UseHost(_ => Host.CreateDefaultBuilder(),
        host =>
        {
            host.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddFilter("Microsoft", LogLevel.Warning);
                    logging.AddFilter("System", LogLevel.Warning);
                    // logging.AddFile("logs/paradox-parser-{Date}.log");
                }
            );

            host.ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IGameSaveService, StellarisSaveService>();
                    services.AddSingleton<StellarisSaveService>();
                }
            );
        }
    )
    .UseDefaults()
    .Build();
        
return await parser.InvokeAsync(args);