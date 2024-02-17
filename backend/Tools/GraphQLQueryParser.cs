using System.Text.RegularExpressions;

namespace NetBackend.Tools;

public partial class GraphQLQueryParser
{
    private readonly ILogger<GraphQLQueryParser> _logger;

    public GraphQLQueryParser(ILogger<GraphQLQueryParser> logger)
    {
        _logger = logger;
    }

    public static Dictionary<string, List<string>> ParseQuery(string query)
    {
        var operations = new Dictionary<string, List<string>>();
        var operationMatches = MyRegex().Matches(query);

        foreach (Match match in operationMatches.Cast<Match>())
        {
            var operationName = match.Groups[1].Value.Trim();
            var fields = match.Groups[2].Value
                .Split(new[] { '\n', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrEmpty(f))
                .ToList();

            operations.TryAdd(operationName, fields);
        }

        return operations;
    }

    [GeneratedRegex(@"(\w+)(?:\([^)]*\))?\s*{\s*([^}]+)\s*}", RegexOptions.IgnoreCase | RegexOptions.Multiline, "nb-NO")]
    private static partial Regex MyRegex();
}