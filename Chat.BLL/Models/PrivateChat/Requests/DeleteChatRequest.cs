using Chat.BLL.Models.Base;

namespace Chat.BLL.Models.PrivateChat.Requests
{
    public class DeleteChatRequest : BaseAuthorizedRequest
    {
        public required long OtherUserId { get; init; }
    }
}
