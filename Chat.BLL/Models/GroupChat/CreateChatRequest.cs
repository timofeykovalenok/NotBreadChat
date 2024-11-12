namespace Chat.BLL.Models.GroupChat
{
    public class CreateChatRequest
    {
        public required string Title { get; init; }
        public required string? Image { get; init; }
        public required List<long> InvitedUsers { get; init; }
    }
}
