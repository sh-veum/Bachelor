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
            // Use the existing parser for queries that start with "{" (postman, etc.)
            return ParseImplicitQuery(query);
        }
        else if (query.TrimStart().StartsWith("query CombinedQuery"))
        {
            // For combined Apollo queries
            return ParseCombinedQuery(query);
        }
        else
        {
            // Use a new parser for queries that start with an operation name (Apollo, etc.)
            return ParseNamedQuery(query);
        }
    }

    private static Dictionary<string, List<string>> ParseImplicitQuery(string query)
    {
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
        var pattern = @"\b(\w+)\s*\(([^)]*)\)\s*{\s*(\w+)\s*\(([^)]*)\)\s*{\s*([^}]+?)\s*}\s*}";
        var fieldPattern = @"\b(\w+)\b";

        var operationMatches = Regex.Matches(query, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        foreach (Match match in operationMatches)
        {
            var operationName = match.Groups[3].Value.Trim();
            var fieldsBlock = match.Groups[5].Value;

            var fieldMatches = Regex.Matches(fieldsBlock, fieldPattern);
            var fields = fieldMatches.Cast<Match>()
                                     .Select(match => match.Groups[1].Value)
                                     .Where(field => !field.Equals("__typename", StringComparison.OrdinalIgnoreCase))
                                     .Distinct()
                                     .ToList();

            if (!operations.ContainsKey(operationName))
            {
                operations.Add(operationName, fields);
            }
            else
            {
                operations[operationName].AddRange(fields.Where(f => !operations[operationName].Contains(f)));
            }
        }

        return operations;
    }

    private static Dictionary<string, List<string>> ParseCombinedQuery(string query)
    {
        var operations = new Dictionary<string, List<string>>();
        var operationPattern = @"(\w+)\s*\(\s*encryptedKey\s*:\s*""[^""]*""\s*\)\s*{\s*([^}]+?)\s*}";
        var fieldPattern = @"\b(\w+)\b";

        var operationMatches = Regex.Matches(query, operationPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        foreach (Match operationMatch in operationMatches)
        {
            var operationName = operationMatch.Groups[1].Value.Trim();
            var fieldsBlock = operationMatch.Groups[2].Value;

            var fieldMatches = Regex.Matches(fieldsBlock, fieldPattern);
            var fields = fieldMatches.Cast<Match>()
                                     .Select(match => match.Groups[1].Value)
                                     .Where(field => !field.Equals("__typename", StringComparison.OrdinalIgnoreCase))
                                     .Distinct()
                                     .ToList();

            if (!operations.ContainsKey(operationName))
            {
                operations.Add(operationName, fields);
            }
            else
            {
                operations[operationName].AddRange(fields.Where(f => !operations[operationName].Contains(f)));
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
