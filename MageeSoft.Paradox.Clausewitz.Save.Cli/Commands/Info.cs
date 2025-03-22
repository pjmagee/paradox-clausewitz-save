using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Commands;

/// <summary>
/// Command to display version information
/// </summary>
public class Info : BaseCommand
{
    public Info() : base("info", "Display the version of the tool")
    {
        Handler = CommandHandler.Create<IHost, IConsole>(HandleCommand);
    }
    
    private void HandleCommand(IHost host, IConsole console)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var productName = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "MageeSoft.Paradox.Clausewitz.Save.Cli";
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";
        var buildConfig = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "Unknown";
        var isAot = System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported ? "No" : "Yes";
        
        string gitInfo = string.Empty;
        
        if (version.Contains('+'))
        {
            var parts = version.Split('+');
            if (parts.Length > 1)
            {
                gitInfo = parts[1];
            }
        }

        console.WriteLine($"{productName} v{version}");
        console.WriteLine($"Build: {buildConfig}");
        console.WriteLine($"Native AOT: {isAot}");
        
        if (!string.IsNullOrEmpty(gitInfo))
        {
            console.WriteLine($"Git: {gitInfo}");
        }
    }
} 