const express = require('express');
const router = express.Router();
const libraryService = require('../services/libraryService');

// GET /api/books – list all books
router.get('/', (req, res) => {
  res.json(libraryService.getAllBooks());
});

// GET /api/books/:id – get a single book
router.get('/:id', (req, res) => {
  const book = libraryService.getBook(Number(req.params.id));
  if (!book) return res.status(404).json({ message: 'Book not found.' });
  res.json(book);
});

// POST /api/books/:id/reserve – reserve a book
router.post('/:id/reserve', (req, res) => {
  const { reservedBy } = req.body;
  if (!reservedBy) return res.status(400).json({ message: 'reservedBy is required.' });

  const result = libraryService.reserveBook(Number(req.params.id), reservedBy);
  res.status(result.success ? 200 : 400).json({ message: result.message });
});

// POST /api/books/:id/return – return a book
router.post('/:id/return', (req, res) => {
  const result = libraryService.returnBook(Number(req.params.id));
  res.status(result.success ? 200 : 400).json({ message: result.message });
});

module.exports = router;