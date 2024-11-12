using Chat.BLL.Models.Shared;

namespace Chat.BLL.Models.PrivateChat.Responses
{
    public class GetPrivateChatResponse
    {
        public required UserModel OtherUser { get; init; }
        public required DateTime? LastViewedMessageCreateAt { get; init; }
        public required IEnumerable<MessageModel> Messages { get; init; }
    }
}
