using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Commands;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Services;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Services.Games.Stellaris;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var parser = new CommandLineBuilder(new ParadoxCliRootCommand())
    .UseHost(_ => Host.CreateDefaultBuilder(), hostBuilder =>
        {
            hostBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton<GameServiceManager>();
                    services.AddSingleton<GamePathResolver>();
                    services.AddSingleton<IGameFilesProvider, StellarisSaveService>();
                    services.AddLogging(loggingBuilder => loggingBuilder.ClearProviders());
                }
            );
        }
    )
    .UseDefaults()
    .Build();

return await parser.InvokeAsync(args);