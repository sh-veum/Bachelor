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
        // Determine the query type based on its starting character
        if (query.TrimStart().StartsWith("{"))
        {
            // Use the existing parser for queries that start with "{"
            return ParseImplicitQuery(query);
        }
        else
        {
            // Use a new parser for queries that start with an operation name
            return ParseNamedQuery(query);
        }
    }

    private static Dictionary<string, List<string>> ParseImplicitQuery(string query)
    {
        // This method contains the existing parsing logic for queries starting with "{"
        // The implementation here remains unchanged and is assumed to work as provided in your original code snippet
        return MyRegex().Matches(query)
            .Cast<Match>()
            .GroupBy(match => match.Groups[1].Value.Trim())
            .ToDictionary(
                grp => grp.Key,
                grp => grp.SelectMany(match => ExtractFields(match.Groups[2].Value))
                          .Where(field => !field.Equals("__typename", StringComparison.OrdinalIgnoreCase))
                          .Distinct()
                          .ToList());
    }

    private static Dictionary<string, List<string>> ParseNamedQuery(string query)
    {
        var operations = new Dictionary<string, List<string>>();
        // Define a regex pattern to parse queries that start with an operation name
        var pattern = @"\b(\w+)\s*\(([^)]*)\)\s*{\s*(\w+)\s*\(([^)]*)\)\s*{\s*([^}]+?)\s*}\s*}";
        var matches = Regex.Matches(query, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            var operationName = match.Groups[3].Value.Trim(); // Assuming this captures the inner operation name correctly
            var fieldsBlock = match.Groups[5].Value;

            var fields = ExtractFields(fieldsBlock).Where(field => !field.Equals("__typename", StringComparison.OrdinalIgnoreCase)).ToList();

            if (!operations.ContainsKey(operationName))
            {
                operations.Add(operationName, fields);
            }
            else
            {
                operations[operationName] = operations[operationName].Union(fields).Distinct().ToList();
            }
        }

        return operations;
    }

    private static List<string> ExtractFields(string fieldsBlock)
    {
        var fieldPattern = @"\b\w+\b";
        var matches = Regex.Matches(fieldsBlock, fieldPattern);

        return matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();
    }

    [GeneratedRegex(@"(\w+)(?:\([^)]*\))?\s*{\s*([^}]+)\s*}", RegexOptions.IgnoreCase | RegexOptions.Multiline, "nb-NO")]
    private static partial Regex MyRegex();
}
