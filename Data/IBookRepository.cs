public interface IBookRepository : IDisposable
{
    Task<List<Book>> GetBooksAsync();
    Task<List<Book>> GetBooksAsync(string name);
    Task<Book?> GetBookAsync(int id);
    Task InsertBookAsync(Book book);
    Task UpdateBookAsync(Book book);
    Task DeleteBookAsync(int id);
    Task SaveAsync();
}