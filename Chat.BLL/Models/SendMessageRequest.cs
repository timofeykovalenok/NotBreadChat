using Chat.BLL.Models.Base;

namespace Chat.BLL.Models
{
    public class SendMessageRequest : BaseAuthorizedRequest
    {
        public required long ChatId { get; init; }
        public required string Content { get; init; }
    }
}
