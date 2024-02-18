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

        // Simplified regex pattern to match operations and fields, including handling variables
        var pattern = @"query\s+(\w+)\s*\(.*?\)\s*{\s*([\w\s\n,]+)\s*}";
        var matches = Regex.Matches(query, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

        foreach (Match match in matches)
        {
            var operationName = match.Groups[1].Value.Trim();
            var fieldsBlock = match.Groups[2].Value;

            // Further processing to extract fields from the fields block
            var fields = ExtractFields(fieldsBlock);

            operations.TryAdd(operationName, fields);
        }

        return operations;
    }

    private static List<string> ExtractFields(string fieldsBlock)
    {
        var fieldPattern = @"\b\w+\b";
        var matches = Regex.Matches(fieldsBlock, fieldPattern);

        var fields = matches.Cast<Match>()
                            .Select(m => m.Value)
                            .ToList();

        return fields;
    }

    [GeneratedRegex(@"(\w+)(?:\([^)]*\))?\s*{\s*([^}]+)\s*}", RegexOptions.IgnoreCase | RegexOptions.Multiline, "nb-NO")]
    private static partial Regex MyRegex();
}