using Chat.BLL.Models.Base;

namespace Chat.BLL.Models
{
    public class SendPrivateChatMessageRequest : BaseAuthorizedRequest
    {
        public required long ReceiverId { get; init; }
        public required string Content { get; init; }
    }
}
