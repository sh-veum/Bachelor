namespace NetBackend.Constants;

public static class GraphQLConstants
{
    public static readonly List<List<string>> AvailableQueries =
    [
        // Query, Return Type
        ["species", "Species"],
        ["organizations", "Organization"]
    ];

    public static readonly string[] AvailableQueryTables =
    [
        "Species",
        "Organization"
    ];
}