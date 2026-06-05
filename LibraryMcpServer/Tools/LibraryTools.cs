using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace LibraryMcpServer.Tools;

[McpServerToolType]
public class LibraryTools(IHttpClientFactory httpClientFactory)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "get_books"), Description("Get all books in the library with their availability status.")]
    public async Task<string> GetBooks()
    {
        var client = httpClientFactory.CreateClient("LibraryApi");
        var response = await client.GetStringAsync("/api/books");
        return response;
    }

    [McpServerTool(Name = "get_book"), Description("Get details of a specific book by its ID.")]
    public async Task<string> GetBook([Description("The book ID")] int id)
    {
        var client = httpClientFactory.CreateClient("LibraryApi");
        var response = await client.GetAsync($"/api/books/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return $"Book with ID {id} not found.";
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool(Name = "reserve_book"), Description("Reserve a book by ID for a specific person. Returns error if already reserved.")]
    public async Task<string> ReserveBook(
        [Description("The book ID to reserve")] int id,
        [Description("The name of the person reserving the book")] string reservedBy)
    {
        var client = httpClientFactory.CreateClient("LibraryApi");
        var body = JsonSerializer.Serialize(new { reservedBy });
        var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/books/{id}/reserve", content);
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool(Name = "return_book"), Description("Return a previously reserved book by ID.")]
    public async Task<string> ReturnBook([Description("The book ID to return")] int id)
    {
        var client = httpClientFactory.CreateClient("LibraryApi");
        var response = await client.PostAsync($"/api/books/{id}/return", null);
        return await response.Content.ReadAsStringAsync();
    }
}
