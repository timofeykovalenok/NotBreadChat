namespace Chat.BLL.Models
{
    public class GetChatsResponse
    {
        public required IEnumerable<ChatModel> Chats { get; init; }
    }

    public class ChatModel
    {
        public required long OtherUserId { get; init; }
        public required string Title { get; init; }
        public required string? Image { get; init; }
        public required int UnviewedMessagesCount { get; init; }
        public required MessageModel LastMessage { get; init; }
    }
    public record MessageModel
    {
        //public required string AuthorName { get; init; }
        //public required string? AuthorImage { get; init; }
        public required long AuthorId { get; init; }
        public required long ReceiverUserId { get; init; }
        public required long MessageId { get; init; }
        public required string MessageContent { get; init; }
        public required DateTime MessageDate { get; init; }
        public required DateTime? EditedDate { get; init; }
    }
}
