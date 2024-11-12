namespace Chat.BLL.Models.User.Requests
{
    public class LoginRequest
    {
        public required string Login { get; init; }
        public required string Password { get; init; }
    }
}
