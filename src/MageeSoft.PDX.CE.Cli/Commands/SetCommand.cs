using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using MageeSoft.PDX.CE.Cli.Commands.Options;
using MageeSoft.PDX.CE.Save;
using MageeSoft.PDX.CE.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MageeSoft.PDX.CE.Cli.Commands
{
    public class SetCommand : BaseCommand
    {
        public SetCommand() : base("set", "Set a value in a save file")
        {
            AddOption(new GameOption());
            AddOption(new NumberOption());
            AddOption(new SaveFileOption());
            AddOption(new QueryOption());
            AddOption(new OutputOption());
            AddOption(new ValueOption());
            AddOption(new Option<bool>(
                new[] { "--in-place", "-i" },
                "Modify the original save file in place (like sed -i)") { IsRequired = false });

            Handler = CommandHandler.Create<IHost, IConsole, string, int?, FileInfo?, string, string, FileInfo?, bool>(HandleCommand);
        }

        private void HandleCommand(IHost host, IConsole console, string game, int? number, FileInfo? saveFile, string query, string value, FileInfo? output, bool inPlace)
        {
            // Validate that exactly one of number or saveFile is provided
            if ((number.HasValue && saveFile != null) || (!number.HasValue && saveFile == null))
            {
                console.Error.WriteLine("You must provide either --number/-n or --save-file/-s, but not both.");
                return;
            }

            FileInfo file;
            IGameFilesProvider? provider = null;
            if (saveFile != null)
            {
                file = saveFile;
                // Try to infer provider from extension or fallback to Stellaris for now
                provider = host.Services.GetRequiredService<GameServiceManager>().GetProviderByName(game) ??
                           host.Services.GetRequiredService<GameServiceManager>().GetProviderByName("stellaris");
            }
            else
            {
                var gameServiceManager = host.Services.GetRequiredService<GameServiceManager>();
                var saveFileResult = GetSaveFileWithProvider(gameServiceManager, number!.Value, game);
                if (saveFileResult == null) return;
                (file, provider) = saveFileResult.Value;
            }

            if (provider == null || provider.GameType != GameType.Stellaris)
            {
                console.Error.WriteLine($"Set is only supported for Stellaris saves at this time.");
                return;
            }

            var stellarisSave = StellarisSave.FromSave(file);
            var pdxQuery = new PdxQuery(stellarisSave.GameState);
            var newValue = PdxQuery.ParseUserInput(value);
            bool changed;
            if (newValue is PdxArray arr)
            {
                changed = pdxQuery.SetArrayByPath(query, arr.Items);
            }
            else
            {
                changed = pdxQuery.SetValueByPath(query, newValue);
            }

            if (!changed)
            {
                console.Error.WriteLine($"Key or path '{query}' not found in save file.");
                return;
            }

            if (output != null || inPlace)
            {
                var outPath = output?.FullName ?? file.FullName;
                stellarisSave.WriteTo(outPath);
                console.Out.WriteLine($"Successfully wrote modified save to {outPath}");
            }
            else
            {
                // Dry-run: show what would change
                console.Out.WriteLine($"[Dry-run] Would set '{query}' to '{value}' in {file.FullName}.");
            }
        }

        private (FileInfo, IGameFilesProvider)? GetSaveFileWithProvider(GameServiceManager gameServiceManager, int number, string? gameName)
        {
            var cachePath = Path.Combine(Path.GetTempPath(), "paradox-save-parser-cache.txt");

            if (!File.Exists(cachePath))
            {
                Console.WriteLine("Error: No save file list cache found. Please run the 'list' command first.");
                return null;
            }

            try
            {
                var lines = File.ReadAllLines(cachePath);

                if (number <= 0 || number > lines.Length)
                {
                    Console.WriteLine($"Error: Invalid save file number: {number}. Valid range is 1-{lines.Length}.");
                    return null;
                }

                var parts = lines[number - 1].Split('|');
                if (parts.Length < 3)
                {
                    Console.WriteLine("Error: Invalid format in cache file. Please run the 'list' command again.");
                    return null;
                }

                var filePath = parts[1];
                var gameType = parts[2];

                // If game is specified, make sure it matches
                if (!string.IsNullOrEmpty(gameName) &&
                    !gameType.Equals(gameName, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Error: Save file #{number} is from game '{gameType}', but '{gameName}' was requested.");
                    return null;
                }

                var file = new FileInfo(filePath);

                if (!file.Exists)
                {
                    Console.WriteLine($"Error: Save file not found at {filePath}");
                    return null;
                }

                // Get the appropriate provider for this game
                var provider = gameServiceManager.GetProviderByName(gameType);
                if (provider == null)
                {
                    Console.WriteLine($"Error: No provider found for game type '{gameType}'.");
                    return null;
                }

                return (file, provider);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Failed to read save file cache: {ex.Message}");
                return null;
            }
        }
    }
} 