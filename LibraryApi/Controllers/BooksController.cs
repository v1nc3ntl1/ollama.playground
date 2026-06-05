using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(LibraryService libraryService) : ControllerBase
{
    // GET /api/books
    [HttpGet]
    public IActionResult GetAll() => Ok(libraryService.GetAllBooks());

    // GET /api/books/{id}
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var book = libraryService.GetBook(id);
        return book is null ? NotFound() : Ok(book);
    }

    // POST /api/books/{id}/reserve
    [HttpPost("{id:int}/reserve")]
    public IActionResult Reserve(int id, [FromBody] ReserveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReservedBy))
            return BadRequest("ReservedBy is required.");

        var (success, message) = libraryService.ReserveBook(id, request.ReservedBy);
        return success ? Ok(new { message }) : Conflict(new { message });
    }

    // POST /api/books/{id}/return
    [HttpPost("{id:int}/return")]
    public IActionResult Return(int id)
    {
        var (success, message) = libraryService.ReturnBook(id);
        return success ? Ok(new { message }) : Conflict(new { message });
    }
}

public record ReserveRequest(string ReservedBy);
