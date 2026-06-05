using LibraryApi.Models;

namespace LibraryApi.Services;

public class LibraryService
{
    private readonly List<Book> _books = new()
    {
        new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Isbn = "978-0132350884" },
        new Book { Id = 2, Title = "The Pragmatic Programmer", Author = "Andrew Hunt, David Thomas", Isbn = "978-0201616224" },
        new Book { Id = 3, Title = "Design Patterns", Author = "Gang of Four", Isbn = "978-0201633610" },
        new Book { Id = 4, Title = "Domain-Driven Design", Author = "Eric Evans", Isbn = "978-0321125217" },
        new Book { Id = 5, Title = "Refactoring", Author = "Martin Fowler", Isbn = "978-0201485677" },
    };

    public IReadOnlyList<Book> GetAllBooks() => _books.AsReadOnly();

    public Book? GetBook(int id) => _books.FirstOrDefault(b => b.Id == id);

    public (bool Success, string Message) ReserveBook(int id, string reservedBy)
    {
        var book = GetBook(id);
        if (book is null)
            return (false, "Book not found.");
        if (book.IsReserved)
            return (false, $"Book is already reserved by {book.ReservedBy}.");

        book.IsReserved = true;
        book.ReservedBy = reservedBy;
        return (true, $"Book '{book.Title}' reserved successfully.");
    }

    public (bool Success, string Message) ReturnBook(int id)
    {
        var book = GetBook(id);
        if (book is null)
            return (false, "Book not found.");
        if (!book.IsReserved)
            return (false, "Book is not currently reserved.");

        book.IsReserved = false;
        book.ReservedBy = null;
        return (true, $"Book '{book.Title}' returned successfully.");
    }
}
