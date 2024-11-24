using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Book_Management_Module.Contexts;
using Book_Management_Module.Entities;

namespace Book_Management_Module.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly libDatabaseContext _context;

        public BooksController(libDatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Books
        [HttpGet("Get")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
          if (_context.Books == null)
          {
              return NotFound();
          }
            return await _context.Books.ToListAsync();
        }

        // GET: api/Books/5
        [HttpGet("Get/{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
          if (_context.Books == null)
          {
              return NotFound();
          }
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }

        // PUT: api/Books/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("Put/{id}")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            if (id != book.BookId)
            {
                return BadRequest();
            }

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Books
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Post")]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {
          if (_context.Books == null)
          {
              return Problem("Entity set 'libDatabaseContext.Books'  is null.");
          }
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBook", new { id = book.BookId }, book);
        }

        // DELETE: api/Books/5
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            if (_context.Books == null)
            {
                return NotFound();
            }
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookExists(int id)
        {
            return (_context.Books?.Any(e => e.BookId == id)).GetValueOrDefault();
        }

        [HttpGet("categories/{genre}")]
        public async Task<IActionResult> GetBooksByCategory(string genre)
        {
            var books = await _context.Books
                .Where(b => b.Genre.ToLower() == genre.ToLower())  // Genre'ye göre filtreleme
                .ToListAsync();

            if (books == null || !books.Any())
            {
                return NotFound($"No books found for the category: {genre}");
            }

            return Ok(books);
        }
        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.Books
                .Select(b => b.Genre)  
                .Distinct()            
                .ToListAsync();

            return Ok(categories);
        }
        [HttpGet("search")]
        public IActionResult SearchBooks(string? title, string? author, string? category, int page = 1, int size = 10)
        {
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(b => b.BookName.Contains(title));
            }

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(b => b.Author.Contains(author));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(b => b.Genre.Contains(category));
            }

            var totalItems = query.Count();
            var books = query
                .Skip((page - 1) * size)
                .Take(size)
                .Select(b => new
                {
                    b.BookId,
                    b.BookName,
                    b.Author,
                    b.Genre
                })
                .ToList();

            return Ok(new
            {
                TotalItems = totalItems,
                Page = page,
                Size = size,
                Books = books
            });


        }
        [HttpGet("{bookId}/getbookavailability")]
        public IActionResult GetBookAvailability(int bookId)
        {
            var borrowing = _context.Borrowings
                .Where(b => b.BookId == bookId)
                .OrderByDescending(b => b.BorrowDate) // En son ödünç alma kaydını al
                .FirstOrDefault();

            if (borrowing == null)
            {
                return Ok("available"); // Eğer Borrowing kaydı yoksa kitap mevcut
            }

            return Ok(borrowing.Status); // Borrowing içindeki durum döndürülür
        }

        [HttpPut("{bookId}/updatebookavailability")]
        public IActionResult UpdateBookAvailability(int bookId, [FromBody] string newStatus)
        {
            // Kitabı bul
            var book = _context.Books.Find(bookId);

            if (book == null)
            {
                return NotFound("Book not found.");
            }

            // Borrowing kaydı varsa, son durumunu güncelle
            var borrowing = _context.Borrowings
                .Where(b => b.BookId == bookId)
                .OrderByDescending(b => b.BorrowDate)
                .FirstOrDefault();

            if (borrowing != null)
            {
                borrowing.Status = newStatus; // "available", "reserved", "borrowed" gibi
            }
            else if (newStatus == "available")
            {
                // Eğer borrowing kaydı yok ve durum "available" ise bir şey yapmaya gerek yok
                return Ok($"Book status updated to '{newStatus}'. (No borrowing record found)");
            }
            else
            {
                // Borrowing kaydı yoksa ve durum değişikliği yapılıyorsa, hata döndür
                return BadRequest("Cannot update status without a borrowing record.");
            }

            _context.SaveChanges();
            return Ok($"Book status updated to '{newStatus}'.");
        }













    }
}
