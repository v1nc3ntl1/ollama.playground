const Book = require('../models/book');

class LibraryService {
  constructor() {
    // Initialise the in‑memory collection just like the C# version
    this._books = [
      new Book({ id: 1, title: 'Clean Code', author: 'Robert C. Martin', isbn: '978-0132350884' }),
      new Book({ id: 2, title: 'The Pragmatic Programmer', author: 'Andrew Hunt, David Thomas', isbn: '978-0201616224' }),
      new Book({ id: 3, title: 'Design Patterns', author: 'Gang of Four', isbn: '978-0201633610' }),
      new Book({ id: 4, title: 'Domain-Driven Design', author: 'Eric Evans', isbn: '978-0321125217' }),
      new Book({ id: 5, title: 'Refactoring', author: 'Martin Fowler', isbn: '978-0201485677' })
    ];
  }

  getAllBooks() {
    // Return a shallow copy to avoid accidental mutation
    return this._books.map(b => ({ ...b }));
  }

  getBook(id) {
    return this._books.find(b => b.id === id) || null;
  }

  reserveBook(id, reservedBy) {
    const book = this.getBook(id);
    if (!book) return { success: false, message: 'Book not found.' };
    if (book.isReserved) return { success: false, message: `Book is already reserved by ${book.reservedBy}.` };

    book.isReserved = true;
    book.reservedBy = reservedBy;
    return { success: true, message: `Book '${book.title}' reserved successfully.` };
  }

  returnBook(id) {
    const book = this.getBook(id);
    if (!book) return { success: false, message: 'Book not found.' };
    if (!book.isReserved) return { success: false, message: 'Book is not currently reserved.' };

    book.isReserved = false;
    book.reservedBy = null;
    return { success: true, message: `Book '${book.title}' returned successfully.` };
  }
}

// Export a singleton (same behaviour as AddSingleton in ASP.NET Core)
module.exports = new LibraryService();