namespace Chat.BLL.Models.Shared
{
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
