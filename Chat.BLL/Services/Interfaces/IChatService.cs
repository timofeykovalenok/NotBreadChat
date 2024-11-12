using Chat.BLL.Models.PrivateChat.Requests;
using Chat.BLL.Models.PrivateChat.Responses;

namespace Chat.BLL.Services.Interfaces
{
    public interface IChatService
    {
        Task<GetChatsResponse> GetChats(GetChatsRequest request);
        Task<GetPrivateChatResponse> GetPrivateChat(GetPrivateChatRequest request);

        Task DeleteChatLocally(DeleteChatRequest request);
        Task DeleteChat(DeleteChatRequest request);
    }
}
