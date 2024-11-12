namespace Chat.BLL.Models.User.Requests
{
    public class RegisterRequest
    {
        public required string Login { get; init; }
        public required string UserName { get; init; }
        public required string Password { get; init; }
    }
}
