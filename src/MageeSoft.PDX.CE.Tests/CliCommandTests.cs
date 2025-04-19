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
        
        Assert.IsTrue(
            condition: !string.IsNullOrWhiteSpace(output) && output.Contains("Version"),
            message: $"Output was: {output}"
        );
    }
    
    [TestMethod]
    [DataRow("galactic_object", "galactic_object[0]: {")]
    [DataRow("required_dlcs", "Ancient Relics Story Pack")]
    [DataRow("nebula.galactic_object", "nebula.galactic_object[4]")]
    public void Query_Key(string key, string expected)
    {
        var ironmanPath = Path.Combine("Stellaris", "TestData", "ironman.sav");
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
                });
            })
            .UseDefaults()
            .Build();

        TestConsole console = new();
        parser.Invoke($"query -g=stellaris -s={ironmanPath} -q={key} --show-paths", console: console);
        var output = console.Out.ToString();
        TestContext.WriteLine(output);
        StringAssert.Contains(value: output, substring: expected, StringComparison.Ordinal);
        Assert.IsTrue(!string.IsNullOrWhiteSpace(output), $"Should find {key} somewhere in the save");
    }

    [TestMethod]
    [DataRow("player.[*].name", "player.[*].name[0]: \"Delegate\"")]
    [DataRow("player.[*].country", "player.[*].country[0]: 0")]
    public void Query_Wildcard(string query, string expected)
    {
        var ironmanPath = Path.Combine("Stellaris", "TestData", "ironman.sav");
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
                });
            })
            .UseDefaults()
            .Build();

        TestConsole console = new();
        parser.Invoke($"""query -g=stellaris -s="{ironmanPath}" -q="{query}" --show-paths""", console: console);
        var output = console.Out.ToString();
        TestContext.WriteLine(output);
        StringAssert.Contains(value: output, substring: expected, StringComparison.Ordinal);
        Assert.IsTrue(!string.IsNullOrWhiteSpace(output), $"Should find at least one value for {query} in the save");
    }

    [TestMethod]
    [DataRow(".. | .energy?", new[] {
            "country.0.budget.current_month.income.country_base.energy: 20",
            "country.0.budget.current_month.balance.starbase_stations.energy: -20"
        }
    )]
    [DataRow(".. | .alloys?", new[]
    {
        "country.0.budget.current_month.income.planet_metallurgists.alloys: 39.26",
        "country.1.budget.last_month.balance.ship_components.alloys: -4.8365"
    })]
    [DataRow(".. | .trait?", new[]
    {
        "species_db.1.traits.trait: \"trait_adaptive\"",
        "species_db.36.traits.trait: \"trait_decadent\""
    })]
    public void Query_RecursiveKeySearch(string query, string[] results)
    {
        var ironmanPath = Path.Combine("Stellaris", "TestData", "ironman.sav");
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
                });
            })
            .UseDefaults()
            .Build();

        TestConsole console = new();
        parser.Invoke($"""query -g=stellaris -s="{ironmanPath}" -q="{query}" --show-paths""", console: console);
        var output = console.Out.ToString();
        TestContext.WriteLine(output);
        
        CollectionAssert.IsSubsetOf(results, output.Split('\n'));
        Assert.IsTrue(!string.IsNullOrWhiteSpace(output), $"Should find at least one value for {query} in the save");
    }

    [TestMethod]
    [DataRow(".. | select(. == yes)")]
    [DataRow(".. | select(. == no)")]
    [DataRow(".. | select(. == none)")]
    public void Query_RecursiveValueSearch(string query)
    {
        var ironmanPath = Path.Combine("Stellaris", "TestData", "ironman.sav");
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
                });
            })
            .UseDefaults()
            .Build();

        TestConsole console = new();
        parser.Invoke($"""query -g=stellaris -s="{ironmanPath}" -q="{query}" --show-paths""", console: console);
        var output = console.Out.ToString();
        TestContext.WriteLine(output);
        Assert.IsTrue(!string.IsNullOrEmpty(output), $"Should find at least one value for {query} in the save");
    }

    [TestMethod]
    [DataRow(".. | select(contains(22))")]
    public void Query_RecursiveSubStringSearch(string query)
    {
        var ironmanPath = Path.Combine("Stellaris", "TestData", "ironman.sav");
        
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
                });
            })
            .UseDefaults()
            .Build();

        TestConsole console = new();
        parser.Invoke($"""query -g=stellaris -s="{ironmanPath}" -q="{query}" --show-paths""", console: console);
        var output = console.Out.ToString();
        TestContext.WriteLine(output);
        Assert.IsTrue(!string.IsNullOrWhiteSpace(output), $"Should find at least one value for {query} in the save");
        
    }
}