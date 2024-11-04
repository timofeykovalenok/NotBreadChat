namespace Chat.BLL.Models.User
{
    public class SearchUsersResponse
    {
        public required IEnumerable<UserModel> Users { get; init; }
    }

    public class UserModel
    {
        public required long UserId { get; init; }
        public required string Username { get; init; }
        public required string? Image { get; init; }
    }
}
