using System.CommandLine;

namespace MageeSoft.PDX.CE.Cli.Commands.Options;

public class QueryOption : Option<string>
{
    public QueryOption() : base(
        aliases: ["--query", "-q"],
        description: "Query path expression (e.g. foo.bar.[*].x)")
    {
        IsRequired = true;       
    }
}