using System.Text.RegularExpressions;

namespace NetBackend.Tools;

public class GraphQLQueryParser
{
    private readonly ILogger<GraphQLQueryParser> _logger;

    public GraphQLQueryParser(ILogger<GraphQLQueryParser> logger)
    {
        _logger = logger;
    }

    public Dictionary<string, List<string>> ParseQuery(string query)
    {
        _logger.LogInformation($"Received GraphQL query for parsing: {query}");
        var operations = new Dictionary<string, List<string>>();
        var operationMatches = Regex.Matches(query, @"(\w+)\s*{([^}]+)}", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        foreach (Match match in operationMatches)
        {
            var operationName = match.Groups[1].Value.Trim();
            var fields = match.Groups[2].Value
                .Split(new[] { '\n', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrEmpty(f))
                .ToList();

            if (!operations.ContainsKey(operationName))
            {
                operations.Add(operationName, fields);
            }
        }

        return operations;
    }
}