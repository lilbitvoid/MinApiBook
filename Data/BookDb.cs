public class BookDb: DbContext 
{
    public BookDb(DbContextOptions options) : base (options) {}
    public DbSet<Book> Books => Set<Book>();
}