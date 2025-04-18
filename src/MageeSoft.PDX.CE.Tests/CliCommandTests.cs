using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using MageeSoft.PDX.CE.Cli.Commands;
using MageeSoft.PDX.CE.Cli.Services;
using MageeSoft.PDX.CE.Cli.Services.Games.Stellaris;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MageeSoft.PDX.CE.Tests;

[TestClass]
public class CliCommandTests
{
    public TestContext TestContext { get; set; } = null!;
    
    [TestMethod]
    public void Info_Command()
    {
        var parser = new CommandLineBuilder(new PdxRootCommand())
            .UseDefaults()
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
    
        // info command
        TestConsole console = new();
        parser.Invoke("info", console: console);
        
        var output = console.Out.ToString();
        Assert.IsTrue(output != null && output.Contains("Version"), $"Output was: {output}");
    }

    [TestMethod]
    public void Query_Command()
    {
        var parser = new CommandLineBuilder(new PdxRootCommand())
            .UseDefaults()
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
            
        TestConsole console = new();
        parser.Invoke($"query --game stellaris --number 1 --query version", console: console);
        var output = console.Out.ToString();
            
        Assert.IsTrue(output != null && output.Contains("Phoenix"), $"Output was: {output}");
    }
}