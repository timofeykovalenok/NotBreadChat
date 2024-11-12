using Chat.BLL.Models.Base;

namespace Chat.BLL.Models.Message.Requests
{
    public class SendPrivateChatMessageRequest : BaseAuthorizedRequest
    {
        public required long ReceiverId { get; init; }
        public required string Content { get; init; }
    }
}
