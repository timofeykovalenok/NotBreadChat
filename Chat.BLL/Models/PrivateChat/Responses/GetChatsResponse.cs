using Chat.BLL.Models.Shared;

namespace Chat.BLL.Models.PrivateChat.Responses
{
    public class GetChatsResponse
    {
        public required IEnumerable<ChatModel> Chats { get; init; }
    }
}
