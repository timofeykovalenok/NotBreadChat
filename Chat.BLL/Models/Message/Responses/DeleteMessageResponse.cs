namespace Chat.BLL.Models.Message.Responses
{
    public class DeleteMessageResponse
    {
        public required long DeletedByUserId { get; init; }
        public required long OtherUserId { get; init; }
        public required long MessageId { get; init; }
    }
}
