namespace Chat.BLL.Models.User.Responses
{
    public class GetUserResponse
    {
        public required long UserId { get; init; }
        public required string Username { get; init; }
        public required string? UserImage { get; init; }
    }
}
