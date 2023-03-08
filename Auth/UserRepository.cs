public class UserRepository : IUserRepository
{
    private List<UserModel> _users = new()
    {
        new UserModel() {UserName = "Ivan", Password = "123"}
    };
    
    public UserModel GetUser(UserModel user) =>
        _users.Single(u =>
            string.Equals(u.UserName, user.UserName) 
            && string.Equals(u.Password, user.Password));
}