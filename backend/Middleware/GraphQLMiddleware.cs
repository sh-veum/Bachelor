using System.Text;
using System.Text.Json;

namespace NetBackend.Middleware;

public class GraphQLRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public GraphQLRequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<GraphQLRequestLoggingMiddleware> logger)
    {
        if (context.Request.Path.StartsWithSegments("/graphql") && context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            context.Request.EnableBuffering();

            var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
            await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            var requestBody = Encoding.UTF8.GetString(buffer);
            context.Request.Body.Position = 0; // Reset the stream position after reading

            try
            {
                var jsonDoc = JsonDocument.Parse(requestBody);
                var query = jsonDoc.RootElement.GetProperty("query").GetString();
                logger.LogInformation($"GraphQL Query: {query}");

                context.Items["GraphQLQuery"] = query;
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Error parsing GraphQL request body.");
            }
        }

        await _next(context);
    }
}