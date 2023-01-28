var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var books = new List<Book>();
app.MapGet("/books", () => books);
app.MapGet("/books/{id}", (int id) => books.FirstOrDefault(b => b.Id == id));
app.MapPost("/books", (Book book) => books.Add(book));
app.MapPut("/books", (Book book) => 
{
    var index = books.FindIndex((b) => b.Id == book.Id);
    if (index < 0)
    {
        throw new Exception("Index not found");
    }
    books[index] = book;
});
app.MapDelete("/books/{id}", (int id) => 
{
    var index = books.FindIndex((b) => b.Id == id);
    if (index < 0)
    {
        throw new Exception("Index not found");
    }
    books.RemoveAt(index);
});
app.Run();

public class Book 
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string YearPubl { get; set; }
}