using System.IO;
using System.Text.Json;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace PublishPubSub;

public class Function : IHttpFunction
{
    private readonly ILogger _logger;
    private const string projectId = "";
    private const string topicId = "";

    public Function(ILogger<Function> logger) =>
        _logger = logger;

    public async Task HandleAsync(HttpContext context)
    {
        HttpRequest request = context.Request;

        // Check URL parameters for "message" field
        string message = request.Query["message"];

        // If there's a body, parse it as JSON and check for "message" field.
        using TextReader reader = new StreamReader(request.Body);
        string text = await reader.ReadToEndAsync();

        try
        {
            if (text.Length > 0)
            {
                JsonElement json = JsonSerializer.Deserialize<JsonElement>(text);
                if (json.TryGetProperty("message", out JsonElement messageElement) &&
                    messageElement.ValueKind == JsonValueKind.String)
                {
                    message = messageElement.GetString();
                }
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                var pubsubClient = new PublishMessages();
                await pubsubClient.PublishMessagesAsync(projectId, topicId, new[] { message });
            }
        }
        catch (JsonException parseException)
        {
            _logger.LogError(parseException, "Error parsing JSON request");
        }


        await context.Response.WriteAsync(message ?? "Empty message");
    }
}