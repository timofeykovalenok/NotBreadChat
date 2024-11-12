using Chat.BLL.Extensions;
using Chat.BLL.Models.PrivateChat.Requests;
using Chat.BLL.Models.PrivateChat.Responses;
using Chat.BLL.Models.Shared;
using Chat.BLL.Services.Interfaces;
using Context;
using Core.Exceptions;
using LinqToDB;
using System.Net;

namespace Chat.BLL.Services
{
    internal class ChatService : IChatService
    {
        #region Injects

        private readonly ChatContext _context;

        #endregion

        #region Ctors

        public ChatService(ChatContext context)
        {
            _context = context;
        }

        #endregion

        #region Public Methods

        public async Task<GetChatsResponse> GetChats(GetChatsRequest request)
        {
            var privateChatsQuery = _context.GetTable<PrivateChat>()
                    .LoadWith(privateChat => privateChat.OtherUser)
                    .Where(privateChat => privateChat.OwnerId == request.AuthorizedUserId);

            var privateChatsWithMessagesGroups = await _context.GetTable<Message>()
                .IsNotDeletedLocally(request)
                .LoadWith(message => message.Message2)
                    .ThenLoad(messageInPrivateChat => messageInPrivateChat.ReceiverUser)
                .Where(message => message.Message2 != null)
                .InnerJoin(privateChatsQuery,
                    (message, privateChat) =>
                        message.AuthorId == privateChat.OwnerId && message.Message2!.ReceiverUserId == privateChat.OtherUserId
                        || message.AuthorId == privateChat.OtherUserId && message.Message2!.ReceiverUserId == privateChat.OwnerId,
                    (message, privateChat) => new { message, privateChat })
                .GroupBy(join => new
                {
                    LeastUserId = join.message.AuthorId <= join.message.Message2!.ReceiverUserId
                        ? join.message.AuthorId
                        : join.message.Message2.ReceiverUserId,
                    GreatestUserId = join.message.AuthorId > join.message.Message2!.ReceiverUserId
                        ? join.message.AuthorId
                        : join.message.Message2!.ReceiverUserId
                })
                .DisableGuard()
                .Select(joinsGroup => new
                {
                    LastMessage = joinsGroup
                        .OrderByDescending(join => join.message.CreateAt)
                        .Take(1),
                    UnviewedMessagesCount = joinsGroup
                        .Count(join => join.message.CreateAt > join.privateChat.LastViewedMessageCreateAt)
                })
                .ToListAsync();

            var orderedMessagesGroups = privateChatsWithMessagesGroups
                .OrderByDescending(group => group.LastMessage.First().message.CreateAt);

            var chatsModels = new List<ChatModel>();

            foreach (var messagesGroup in orderedMessagesGroups)
            {
                var privateChat = messagesGroup.LastMessage.First().privateChat;
                var lastMessage = messagesGroup.LastMessage.First().message;

                var messageModel = new MessageModel
                {
                    AuthorId = lastMessage.AuthorId,
                    ReceiverUserId = lastMessage.Message2!.ReceiverUserId,
                    MessageId = lastMessage.Id,
                    MessageContent = lastMessage.Content,
                    MessageDate = lastMessage.CreateAt,
                    EditedDate = lastMessage.ModifyAt
                };

                var otherUser = privateChat.OtherUser;

                chatsModels.Add(new ChatModel
                {
                    OtherUserId = otherUser.Id,
                    Title = /*otherUser.ContactUserContacts.CustomTitle ??*/ otherUser.Name,
                    Image = otherUser.Image,
                    UnviewedMessagesCount = messagesGroup.UnviewedMessagesCount,
                    LastMessage = messageModel
                });
            }

            return new GetChatsResponse
            {
                Chats = chatsModels
            };
        }

        public async Task<GetPrivateChatResponse> GetPrivateChat(GetPrivateChatRequest request)
        {
            var otherUser = await _context.GetTable<User>()
                .FirstOrDefaultAsync(user => user.Id == request.OtherUserId);

            if (otherUser == null)
                throw new StatusCodeException(HttpStatusCode.NotFound);

            var privateChat = await _context.GetTable<PrivateChat>()
                .FirstOrDefaultAsync(privateChat => privateChat.OwnerId == request.AuthorizedUserId
                    && privateChat.OtherUserId == request.OtherUserId);

            IEnumerable<MessageModel> messagesModels = [];
            if (privateChat != null)
            {
                var messages = await _context.GetTable<MessageInPrivateChat>()
                    .IsNotDeletedLocally(request)
                    .LoadWith(messageInPrivateChat => messageInPrivateChat.Message)
                    //.ThenLoad(message => message.Author)
                    .Where(messageInPrivateChat =>
                        messageInPrivateChat.ReceiverUserId == request.OtherUserId
                        && messageInPrivateChat.Message.AuthorId == request.AuthorizedUserId
                        || messageInPrivateChat.ReceiverUserId == request.AuthorizedUserId
                        && messageInPrivateChat.Message.AuthorId == request.OtherUserId)
                    .OrderBy(messageInPrivateChat => messageInPrivateChat.Message.CreateAt)
                    .ToListAsync();

                messagesModels = messages.Select(messageInPrivateChat =>
                    new MessageModel
                    {
                        AuthorId = messageInPrivateChat.Message.AuthorId,
                        ReceiverUserId = messageInPrivateChat.ReceiverUserId,
                        MessageId = messageInPrivateChat.MessageId,
                        MessageContent = messageInPrivateChat.Message.Content,
                        MessageDate = messageInPrivateChat.Message.CreateAt,
                        EditedDate = messageInPrivateChat.Message.ModifyAt
                    });
            }

            return new GetPrivateChatResponse
            {
                OtherUser = new UserModel
                {
                    UserId = request.OtherUserId,
                    Username = otherUser.Name,
                    Image = otherUser.Image
                },
                LastViewedMessageCreateAt = privateChat?.LastViewedMessageCreateAt,
                Messages = messagesModels
            };
        }

        public async Task DeleteChatLocally(DeleteChatRequest request)
        {
            await _context.BeginTransactionAsync();

            await _context.GetTable<PrivateChat>()
                .Where(privateChat => privateChat.OwnerId == request.AuthorizedUserId
                    && privateChat.OtherUserId == request.OtherUserId)
                .DeleteAsync();

            await _context.GetTable<Message>()
                .Where(message => message.Message2 != null)
                .Where(message => message.AuthorId == request.AuthorizedUserId
                    && message.Message2!.ReceiverUserId == request.OtherUserId
                    || message.AuthorId == request.OtherUserId
                    && message.Message2!.ReceiverUserId == request.AuthorizedUserId)
                .DeleteAsync();

            await _context.CommitTransactionAsync();
        }

        public async Task DeleteChat(DeleteChatRequest request)
        {
            await _context.BeginTransactionAsync();

            await _context.GetTable<PrivateChat>()
                .Where(privateChat => privateChat.OwnerId == request.AuthorizedUserId && privateChat.OtherUserId == request.OtherUserId
                    || privateChat.OwnerId == request.OtherUserId && privateChat.OtherUserId == request.AuthorizedUserId)
                .DeleteAsync();

            await _context.GetTable<Message>()
                .Where(message => message.Message2 != null)
                .Where(message => message.AuthorId == request.AuthorizedUserId
                    && message.Message2!.ReceiverUserId == request.OtherUserId
                    || message.AuthorId == request.OtherUserId
                    && message.Message2!.ReceiverUserId == request.AuthorizedUserId)
                .InsertAsync(_context.GetTable<DeletedMessage>(), message =>
                    new DeletedMessage
                    {
                        UserId = request.AuthorizedUserId,
                        MessageId = message.Id
                    });

            await _context.CommitTransactionAsync();
        }

        #endregion
    }
}
