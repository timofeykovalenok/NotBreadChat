namespace Chat.BLL.Models.Shared
{
    public class UserModel
    {
        public required long UserId { get; init; }
        public required string Username { get; init; }
        public required string? Image { get; init; }
    }
}
