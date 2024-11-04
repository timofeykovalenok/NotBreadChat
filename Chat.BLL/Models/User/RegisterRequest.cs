namespace Chat.BLL.Models.User
{
    public class RegisterRequest
    {
        public required string Login { get; init; }
        public required string UserName { get; init; }
        public required string Password { get; init; }
    }
}
