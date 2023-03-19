var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<BookDb>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("NpgSql"));
});
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddSingleton<ITokenService>(new TokenService());
builder.Services.AddSingleton<IUserRepository>(new UserRepository());
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
    {
        options.TokenValidationParameters = new() 
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BookDb>();
    db.Database.EnsureCreated();
}
app.MapGet("/login", [AllowAnonymous] (HttpContext context, //this endpoint is only for testing
    ITokenService tokenService, IUserRepository userRepository) => 
    {
        UserModel user = new UserModel() 
        {
            UserName = context.Request.Query["username"],
            Password = context.Request.Query["password"]
        };
        var userDto = userRepository.GetUser(user);
        if (userDto is null) return Results.Unauthorized();
        var token = tokenService.BuildToken(builder.Configuration["Jwt:Key"], 
            builder.Configuration["Jwt:Issuer"], userDto);
        return Results.Ok(token);
    });
app.MapGet("/books", [Authorize] async (IBookRepository bookRepository) => 
    Results.Ok(await bookRepository.GetBooksAsync()))
    .Produces<List<Book>>(StatusCodes.Status200OK)
    .WithName("GetAllBooks")
    .WithTags("Getters");

app.MapGet("/books/{id}", [Authorize] async (int id, IBookRepository bookRepository) => 
    await bookRepository.GetBookAsync(id) is Book book
    ? Results.Ok(book)
    : Results.NotFound())
    .Produces<Book>(StatusCodes.Status200OK)
    .WithName("GetBookById")
    .WithTags("Getters");;

app.MapPost("/books", [Authorize] async ([FromBody]Book book, IBookRepository bookRepository) => {
    await bookRepository.InsertBookAsync(book);
    await bookRepository.SaveAsync();
    return Results.Created($"/books/{book.Id}", book);
    })
    .Accepts<Book>("application/json")
    .Produces<Book>(StatusCodes.Status201Created)
    .WithName("CreateBook")
    .WithTags("Creator");
app.MapPut("/books", [Authorize] async ([FromBody]Book book, IBookRepository bookRepository) => 
    {
        await bookRepository.UpdateBookAsync(book);
        await bookRepository.SaveAsync();
        return Results.NoContent();
    })
    .Accepts<Book>("application/json")
    .WithName("UpdateBook")
    .WithTags("Updater");

app.MapDelete("/books/{id}", [Authorize] async (int id, IBookRepository bookRepository) => 
    {
        await bookRepository.DeleteBookAsync(id);
        await bookRepository.SaveAsync();
        return Results.NoContent();
    })
    .WithName("DeleteBookById")
    .WithTags("Deleters");
app.MapGet("/books/search/name/{query}",
    [Authorize] async (string query, IBookRepository repository) =>    
        await repository.GetBooksAsync(query) is IEnumerable<Book> books 
        ? Results.Ok(books)
        : Results.NotFound(Array.Empty<Book>()));

app.UseHttpsRedirection();
app.Run();