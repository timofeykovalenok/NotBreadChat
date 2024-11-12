using Chat.BLL.Models.Base;

namespace Chat.BLL.Models.GroupChat
{
    public class SendMessageRequest : BaseAuthorizedRequest
    {
        public required long ChatId { get; init; }
        public required string Content { get; init; }
    }
}
