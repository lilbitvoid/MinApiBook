
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BookDb>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("NpgSql"));
});
builder.Services.AddScoped<IBookRepository, BookRepository>();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BookDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/books", async (IBookRepository bookRepository) => Results.Ok(await bookRepository.GetBooksAsync()));
app.MapGet("/books/{id}", async (int id, IBookRepository bookRepository) => 
    await bookRepository.GetBookAsync(id) is Book book
    ? Results.Ok(book)
    : Results.NotFound());

app.MapPost("/books", async ([FromBody]Book book, IBookRepository bookRepository) => {
    await bookRepository.InsertBookAsync(book);
    await bookRepository.SaveAsync();
    return Results.Created($"/books/{book.Id}", book);
});
app.MapPut("/books", async ([FromBody]Book book, IBookRepository bookRepository) => 
{
    await bookRepository.UpdateBookAsync(book);
    await bookRepository.SaveAsync();
    return Results.NoContent();
});

app.MapDelete("/books/{id}", async (int id, IBookRepository bookRepository) => 
{
    await bookRepository.DeleteBookAsync(id);
    await bookRepository.SaveAsync();
    return Results.NoContent();
});
app.UseHttpsRedirection();
app.Run();