public interface ITokenService 
{
    string BuildToken(string key, string issure, UserModel user);
}