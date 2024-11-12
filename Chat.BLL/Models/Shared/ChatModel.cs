namespace Chat.BLL.Models.Shared
{
    public class ChatModel
    {
        public required long OtherUserId { get; init; }
        public required string Title { get; init; }
        public required string? Image { get; init; }
        public required int UnviewedMessagesCount { get; init; }
        public required MessageModel LastMessage { get; init; }
    }
}
