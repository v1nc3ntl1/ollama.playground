using LibraryMcpServer.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

var libraryApiUrl = args.FirstOrDefault(a => a.StartsWith("--library-url="))?.Split('=')[1]
    ?? Environment.GetEnvironmentVariable("LIBRARY_API_URL")
    ?? "http://localhost:5000";

builder.Services.AddHttpClient("LibraryApi", client =>
{
    client.BaseAddress = new Uri(libraryApiUrl);
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<LibraryTools>();

// Suppress host startup logs so they don't pollute the MCP stdio stream
builder.Logging.SetMinimumLevel(LogLevel.Warning);

await builder.Build().RunAsync();
