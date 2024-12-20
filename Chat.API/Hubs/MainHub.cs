﻿using Chat.API.Extensions;
using Chat.API.Services.Interfaces;
using Chat.BLL.Models.Message.Requests;
using Chat.BLL.Models.PrivateChat.Requests;
using Chat.BLL.Models.Shared;
using Chat.BLL.Models.User.Requests;
using Chat.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.API.Hubs
{
    [Authorize]
    public class MainHub : Hub
    {
        #region Injects

        private readonly IUserService _userService;
        private readonly IChatService _chatService;
        private readonly IMessageService _messageService;
        private readonly IRazorRenderer _razorRenderer;

        #endregion

        #region Ctors

        public MainHub(IUserService userService, IMessageService messageService, IChatService chatService, IRazorRenderer razorRenderer)
        {
            _userService = userService;
            _chatService = chatService;
            _messageService = messageService;
            _razorRenderer = razorRenderer;
        }

        #endregion

        #region Endpoints

        public async Task<string[]> SearchUsers(SearchUsersRequest request)
        {
            var response = await _userService.SearchUsers(request);

            var result = await _razorRenderer.RenderPartialToStringArrayAsync("_SearchUserPartial", response.Users);
            return result;
        }

        public async Task<string[]> GetChats(GetChatsRequest request)
        {
            var response = await _chatService.GetChats(request);

            var result = await _razorRenderer.RenderPartialToStringArrayAsync("_ChatPreviewPartial", response.Chats);
            return result;
        }

        public async Task DeletePrivateChatLocally(DeleteChatRequest request)
        {
            throw new NotImplementedException();
        }
        
        public async Task DeletePrivateChat(DeleteChatRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task SendPrivateChatMessage(SendPrivateChatMessageRequest request)
        {
            var message = await _messageService.SendPrivateChatMessage(request);

            await Clients.UsersAsCollection(request.AuthorizedUserId, request.ReceiverId)
                .ForEachUserSendAsync("NewMessage", async userId =>
                {

                    var messagePartialTask = _razorRenderer
                        .RenderPartialForUserAsync("_MessagePartial", message, userId);

                    var otherUser = await _userService
                        .GetUser(userId != request.AuthorizedUserId ? request.AuthorizedUserId : request.ReceiverId);

                    var chatPreview = new ChatModel
                    {
                        OtherUserId = otherUser.UserId,
                        Title = otherUser.Username,
                        Image = otherUser.UserImage,
                        UnviewedMessagesCount = 0,
                        LastMessage = message
                    };

                    var chatPreviewPartialTask = _razorRenderer
                        .RenderPartialForUserAsync("_ChatPreviewPartial", chatPreview, userId);

                    await Task.WhenAll(messagePartialTask, chatPreviewPartialTask);

                    return new 
                    {
                        Message = new 
                        {
                            Model = message,
                            Html = messagePartialTask.Result
                        },
                        ChatPreview = new
                        {
                            Model = chatPreview,
                            Html = chatPreviewPartialTask.Result
                        }
                    };
                    
                });
        }

        public async Task ViewMessage(ViewMessageRequest request)
        {
            var response = await _messageService.ViewMessage(request);

            await Clients.Users(request.AuthorizedUserId, response.MessageAuthorId)
                .SendAsync("MessageViewed", response);
        }

        public async Task EditMessage(EditMessageRequest request)
        {
            var response = await _messageService.EditMessage(request);

            await Clients.Users(request.AuthorizedUserId, response.OtherUserId)
                .SendAsync("MessageEdited", response);
        }
        
        public async Task DeleteMessageLocally(DeleteMessageRequest request)
        {
            var response = await _messageService.DeleteMessageLocally(request);

            await Clients.User(request.AuthorizedUserId).SendAsync("MessageDeleted", response);
        }
        
        public async Task DeleteMessage(DeleteMessageRequest request)
        {
            var response = await _messageService.DeleteMessage(request);

            await Clients.Users(request.AuthorizedUserId, response.OtherUserId).SendAsync("MessageDeleted", response);
        }        

        #endregion
    }
}
