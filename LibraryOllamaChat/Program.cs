using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol.Client;

// ── Configuration ──────────────────────────────────────────────────────────
var ollamaUrl     = Environment.GetEnvironmentVariable("OLLAMA_URL")       ?? "http://localhost:11434";
var model         = Environment.GetEnvironmentVariable("OLLAMA_MODEL")      ?? "deepseek-r1";
var libraryApiUrl = Environment.GetEnvironmentVariable("LIBRARY_API_URL")   ?? "http://localhost:5000";
var mcpServerUrl  = Environment.GetEnvironmentVariable("MCP_SERVER_URL");   // e.g. https://library-mcp-server.onrender.com/sse

// ── Connect to MCP server via stdio ────────────────────────────────────────
Console.WriteLine("Starting Library MCP Server...");

IClientTransport transport = mcpServerUrl is not null
    ? new HttpClientTransport(new HttpClientTransportOptions
      {
          Endpoint      = new Uri(mcpServerUrl),
          TransportMode = HttpTransportMode.Sse   // Node.js server uses SSE transport
      })
    : new StdioClientTransport(new StdioClientTransportOptions
      {
          Name    = "LibraryNodeJsMcpServer",
          Command = "node",
          Arguments = [@"C:\projects\ollama.playground\LibraryNodeJsMcpServer\src\index.js"],
          EnvironmentVariables = new Dictionary<string, string?>
          {
              ["LIBRARY_API_URL"] = libraryApiUrl
          }
      });

await using var mcp = await McpClient.CreateAsync(transport);

var mcpTools = await mcp.ListToolsAsync();
Console.WriteLine($"Loaded {mcpTools.Count} tools: {string.Join(", ", mcpTools.Select(t => t.Name))}");

// ── Convert MCP tool schemas → Ollama tool format ──────────────────────────
var ollamaTools = mcpTools.Select(t => (object)new
{
    type     = "function",
    function = new
    {
        name        = t.Name,
        description = t.Description ?? string.Empty,
        parameters  = t.JsonSchema  // JsonElement JSON Schema exposed by McpClientTool
    }
}).ToList();

// ── Chat loop ──────────────────────────────────────────────────────────────
var jsonOpts = new JsonSerializerOptions
{
    PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true
};

using var http = new HttpClient();

var messages = new List<Dictionary<string, object?>>
{
    new() { ["role"] = "system", ["content"] =
        "You are a helpful library assistant. Use the available tools to " +
        "help users browse books, reserve them, and return them." }
};

Console.WriteLine($"\nLibrary Chat  [model: {model}] — type 'exit' to quit\n");
Console.Write("You: ");

string? line;
while ((line = Console.ReadLine()) is not null)
{
    line = line.Trim();
    if (line.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
    if (string.IsNullOrEmpty(line)) { Console.Write("You: "); continue; }

    messages.Add(new() { ["role"] = "user", ["content"] = line });

    // Agentic loop: keep calling Ollama until no more tool calls
    while (true)
    {
        var requestBody = JsonSerializer.Serialize(new
        {
            model,
            messages,
            tools  = ollamaTools,
            stream = false
        }, jsonOpts);

        using var req = new HttpRequestMessage(HttpMethod.Post, $"{ollamaUrl}/api/chat")
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };

        using var res = await http.SendAsync(req);
        var rawJson = await res.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(rawJson);
        var msgEl = doc.RootElement.GetProperty("message");

        // Materialise the message as a plain dict to add back to history
        var assistantMsg = new Dictionary<string, object?>();
        foreach (var prop in msgEl.EnumerateObject())
            assistantMsg[prop.Name] = JsonSerializer.Deserialize<object?>(prop.Value.GetRawText(), jsonOpts);

        messages.Add(assistantMsg);

        // Check for tool_calls
        if (!msgEl.TryGetProperty("tool_calls", out var toolCallsEl) ||
            toolCallsEl.ValueKind != JsonValueKind.Array ||
            toolCallsEl.GetArrayLength() == 0)
        {
            // No more tool calls — print final answer
            var content = msgEl.TryGetProperty("content", out var c) ? c.GetString() ?? "" : "";
            Console.WriteLine($"\nAssistant: {content}\n");
            break;
        }

        // Execute each tool call via MCP
        foreach (var toolCall in toolCallsEl.EnumerateArray())
        {
            var fn   = toolCall.GetProperty("function");
            var name = fn.GetProperty("name").GetString()!;
            var argsEl = fn.TryGetProperty("arguments", out var a) ? a : default;

            // Convert JsonElement arguments to IReadOnlyDictionary<string, object?>
            var toolArgs = new Dictionary<string, object?>();
            if (argsEl.ValueKind == JsonValueKind.Object)
                foreach (var kv in argsEl.EnumerateObject())
                    toolArgs[kv.Name] = DeserializeArg(kv.Value);

            Console.WriteLine($"  [tool] {name}({argsEl.GetRawText()})");

            var result     = await mcp.CallToolAsync(name, toolArgs);
            var resultText = string.Join("\n", result.Content
                .OfType<ModelContextProtocol.Protocol.TextContentBlock>()
                .Select(c => c.Text));

            messages.Add(new() { ["role"] = "tool", ["content"] = resultText });
        }
    }

    Console.Write("You: ");
}

Console.WriteLine("Goodbye!");

// ── Helpers ────────────────────────────────────────────────────────────────
static object? DeserializeArg(JsonElement el) => el.ValueKind switch
{
    JsonValueKind.String  => el.GetString(),
    JsonValueKind.Number  => el.TryGetInt32(out var i)  ? i
                           : el.TryGetInt64(out var l)  ? l
                           : (object?)el.GetDouble(),
    JsonValueKind.True    => true,
    JsonValueKind.False   => false,
    JsonValueKind.Null    => null,
    _                     => el.GetRawText()   // arrays / objects as raw JSON string
};
