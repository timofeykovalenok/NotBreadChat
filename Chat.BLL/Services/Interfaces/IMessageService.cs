using Chat.BLL.Models.Message.Requests;
using Chat.BLL.Models.Message.Responses;
using Chat.BLL.Models.PrivateChat.Responses;
using Chat.BLL.Models.Shared;

namespace Chat.BLL.Services.Interfaces
{
    public interface IMessageService
    {
        Task<MessageModel> SendPrivateChatMessage(SendPrivateChatMessageRequest request);

        Task<ViewMessageResponse> ViewMessage(ViewMessageRequest request);
        Task<EditMessageResponse> EditMessage(EditMessageRequest request);

        Task<DeleteMessageResponse> DeleteMessageLocally(DeleteMessageRequest request);
        Task<DeleteMessageResponse> DeleteMessage(DeleteMessageRequest request);

        //Task InviteUserToChat(InviteUserToChatRequest request);
        //Task DeleteUserFromChat(DeleteUserFromChatRequest request);
    }
}
