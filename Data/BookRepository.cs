public class BookRepository : IBookRepository
{
    private readonly BookDb _context;
    public BookRepository(BookDb context)
    {
        _context = context;
    }
    public async Task DeleteBookAsync(int id)
    {
        var bookFromDb = await _context.Books.FindAsync(new object[]{id});
        if (bookFromDb is null) return;
        _context.Books.Remove(bookFromDb);
    }
    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing) 
                _context.Dispose();
        }
        _disposed = true;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    public async Task<Book?> GetBookAsync(int id) => 
        await _context.Books.FindAsync(id);

    public async Task<List<Book>> GetBooksAsync() => await _context.Books.ToListAsync();

    public async Task InsertBookAsync(Book book) => await _context.AddAsync(book);

    public async Task SaveAsync() => await _context.SaveChangesAsync();

    public async Task UpdateBookAsync(Book book)
    {
        var bookFromDb = await _context.Books.FindAsync(new object[]{book.Id});
        if (bookFromDb is null) return;
        bookFromDb.Name = book.Name;
        bookFromDb.YearPubl = book.YearPubl;
        bookFromDb.Author = book.Author;
    }

    public Task<List<Book>> GetBooksAsync(string name) => 
        _context.Books.Where(book => book.Name.Contains(name)).Select(b => b).ToListAsync();
}