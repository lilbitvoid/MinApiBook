
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BookDb>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("NpgSql"));
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BookDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/books", async (BookDb db) => await db.Books.ToListAsync());
app.MapGet("/books/{id}", async (int id, BookDb db) => 
    await db.Books.FirstOrDefaultAsync(b => b.Id == id) is Book book
    ? Results.Ok(book)
    : Results.NotFound());

app.MapPost("/books", async ([FromBody]Book book, BookDb db) => {
    await db.Books.AddAsync(book);
    await db.SaveChangesAsync();
    return Results.Created($"/books/{book.Id}", book);
});
app.MapPut("/books", async (Book book, BookDb db) => 
{
    var bookFromDb = await db.Books.FindAsync(book.Id);
    if (bookFromDb is null) throw new Exception("Index not found");
    bookFromDb.Name = book.Name;
    bookFromDb.Author = book.Author;
    bookFromDb.YearPubl = book.YearPubl;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/books/{id}", async (int id, BookDb db) => 
{
    var bookFromDb = await db.Books.FindAsync(id);
    if (bookFromDb is null) return Results.NotFound();
    db.Books.Remove(bookFromDb);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.UseHttpsRedirection();
app.Run();

public class BookDb: DbContext 
{
    public BookDb(DbContextOptions options) : base (options) {}
    public DbSet<Book> Books => Set<Book>();
}

public class Book 
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string YearPubl { get; set; } = string.Empty;
}