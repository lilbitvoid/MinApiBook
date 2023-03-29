var builder = WebApplication.CreateBuilder(args);
RegisterServices(builder.Services);
var app = builder.Build();
Configure(app);
var apis = app.Services.GetServices<IApi>();
foreach (var api in apis)
    api.Register(app);
app.Run();

void RegisterServices(IServiceCollection services) 
{
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddDbContext<BookDb>(options => {
        options.UseNpgsql(builder.Configuration.GetConnectionString("NpgSql"));
    });
    services.AddScoped<IBookRepository, BookRepository>();
    services.AddSingleton<ITokenService>(new TokenService());
    services.AddSingleton<IUserRepository>(new UserRepository());
    services.AddAuthorization();
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
    services.AddTransient<IApi, Api>();
    services.AddTransient<IApi, AuthApi>();
}
void Configure(WebApplication app)
{
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
    app.UseHttpsRedirection();
}