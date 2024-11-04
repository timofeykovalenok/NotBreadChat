using Chat.BLL.Models.Base;

namespace Chat.BLL.Models
{
    public class DeleteChatRequest : BaseAuthorizedRequest
    {
        public required long OtherUserId { get; init; }
    }
}
