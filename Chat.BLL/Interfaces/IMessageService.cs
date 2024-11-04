using Chat.BLL.Models;

namespace Chat.BLL.Interfaces
{
    public interface IMessageService
    {
        Task<GetChatsResponse> GetChats(GetChatsRequest request);
        Task<GetPrivateChatResponse> GetPrivateChat(GetPrivateChatRequest request);
        Task DeleteChat(DeleteChatRequest request);

        Task<MessageModel> SendPrivateChatMessage(SendPrivateChatMessageRequest request);
        Task<ViewMessageResponse> ViewMessage(ViewMessageRequest request);
        Task<EditMessageResponse> EditMessage(EditMessageRequest request);
        Task<DeleteMessageResponse> DeleteMessageLocally(DeleteMessageRequest request);
        Task<DeleteMessageResponse> DeleteMessage(DeleteMessageRequest request);

        //Task InviteUserToChat(InviteUserToChatRequest request);
        //Task DeleteUserFromChat(DeleteUserFromChatRequest request);
    }
}
