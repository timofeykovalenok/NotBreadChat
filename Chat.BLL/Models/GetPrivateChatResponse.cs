using Chat.BLL.Models.User;

namespace Chat.BLL.Models
{
    public class GetPrivateChatResponse
    {
        public required UserModel OtherUser { get; init; }
        public required DateTime? LastViewedMessageCreateAt { get; init; }
        public required IEnumerable<MessageModel> Messages { get; init; }
    }
}
