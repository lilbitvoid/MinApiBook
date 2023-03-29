public class AuthApi : IApi
{
    public void Register(WebApplication app)
    {
        app.MapGet("/login", [AllowAnonymous] (HttpContext context, //this endpoint is only for testing
        ITokenService tokenService, IUserRepository userRepository) => 
        {
            UserModel user = new UserModel() 
            {
                UserName = context.Request.Query["username"]!,
                Password = context.Request.Query["password"]!
            };
            var userDto = userRepository.GetUser(user);
            if (userDto is null) return Results.Unauthorized();
            var token = tokenService.BuildToken(app.Configuration["Jwt:Key"]!, 
                app.Configuration["Jwt:Issuer"]!, userDto);
            return Results.Ok(token);
        });
    }
}