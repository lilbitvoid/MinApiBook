
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<BookDb>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("NpgSql"));
});
builder.Services.AddScoped<IBookRepository, BookRepository>();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BookDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/books", async (IBookRepository bookRepository) => 
    Results.Ok(await bookRepository.GetBooksAsync()))
    .Produces<List<Book>>(StatusCodes.Status200OK)
    .WithName("GetAllBooks")
    .WithTags("Getters");

app.MapGet("/books/{id}", async (int id, IBookRepository bookRepository) => 
    await bookRepository.GetBookAsync(id) is Book book
    ? Results.Ok(book)
    : Results.NotFound())
    .Produces<Book>(StatusCodes.Status200OK)
    .WithName("GetBookById")
    .WithTags("Getters");;

app.MapPost("/books", async ([FromBody]Book book, IBookRepository bookRepository) => {
    await bookRepository.InsertBookAsync(book);
    await bookRepository.SaveAsync();
    return Results.Created($"/books/{book.Id}", book);
    })
    .Accepts<Book>("application/json")
    .Produces<Book>(StatusCodes.Status201Created)
    .WithName("CreateBook")
    .WithTags("Creator");
app.MapPut("/books", async ([FromBody]Book book, IBookRepository bookRepository) => 
    {
        await bookRepository.UpdateBookAsync(book);
        await bookRepository.SaveAsync();
        return Results.NoContent();
    })
    .Accepts<Book>("application/json")
    .WithName("UpdateBook")
    .WithTags("Updater");

app.MapDelete("/books/{id}", async (int id, IBookRepository bookRepository) => 
    {
        await bookRepository.DeleteBookAsync(id);
        await bookRepository.SaveAsync();
        return Results.NoContent();
    })
    .WithName("DeleteBookById")
    .WithTags("Deleters");
app.MapGet("/books/search/name/{query}",
    async (string query, IBookRepository repository) =>    
        await repository.GetBooksAsync(query) is IEnumerable<Book> books 
        ? Results.Ok(books)
        : Results.NotFound(Array.Empty<Book>()));

app.UseHttpsRedirection();
app.Run();