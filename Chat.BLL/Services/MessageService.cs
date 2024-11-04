using Chat.BLL.Extensions;
using Chat.BLL.Interfaces;
using Chat.BLL.Models;
using Context;
using Core.Exceptions;
using LinqToDB;
using LinqToDB.Data;
using Npgsql.Replication.PgOutput.Messages;
using System.Net;

namespace Chat.BLL.Services
{
    internal class MessageService : IMessageService
    {
        private PostgresDb _db;

        #region Public Methods

        public MessageService(PostgresDb db)
        {
            _db = db;
        }        

        public async Task<GetChatsResponse> GetChats(GetChatsRequest request)
        {
            //var groupChats = await _db.GetTable<UserInChat>()
            //    .LoadWith(userInChat => userInChat.Chat)
            //    .Where(userInChat => userInChat.UserId == request.AuthorizedUserId)
            //    .ToDictionaryAsync(userInChat => userInChat.ChatId);

            //var lastGroupChatMessages = await _db.GetTable<Message>()
            //    .LoadWith(message => message.Message1)
            //        .ThenLoad(messageInGroupChat => messageInGroupChat.Chat)
            //    .Where(message => message.Message1 != null && groupChats.ContainsKey(message.Message1.ChatId))
            //    .GroupBy(message => message.Message1!.ChatId, message => message, (key, messages) => messages.MaxBy(z => z.CreateAt))
            //    .ToListAsync();

            var privateChatsQuery = _db.GetTable<PrivateChat>()
                    .LoadWith(privateChat => privateChat.OtherUser)
                    .Where(privateChat => privateChat.OwnerId == request.AuthorizedUserId);

            var privateChatsWithMessagesGroups = await _db.GetTable<Message>()
                .IsNotDeletedLocally(request)
                .LoadWith(message => message.Message2)
                    .ThenLoad(messageInPrivateChat => messageInPrivateChat.ReceiverUser)
                .Where(message => message.Message2 != null)                
                .InnerJoin(privateChatsQuery,
                    (message, privateChat) =>
                        (message.AuthorId == privateChat.OwnerId && message.Message2!.ReceiverUserId == privateChat.OtherUserId)
                        || (message.AuthorId == privateChat.OtherUserId && message.Message2!.ReceiverUserId == privateChat.OwnerId),
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
            var otherUser = await _db.GetTable<User>()
                .FirstOrDefaultAsync(user => user.Id == request.OtherUserId);

            if (otherUser == null)
                throw new StatusCodeException(HttpStatusCode.NotFound);

            var privateChat = await _db.GetTable<PrivateChat>()
                .FirstOrDefaultAsync(privateChat => privateChat.OwnerId == request.AuthorizedUserId
                    && privateChat.OtherUserId == request.OtherUserId);

            IEnumerable<MessageModel> messagesModels = [];
            if (privateChat != null)
            {
                var messages = await _db.GetTable<MessageInPrivateChat>()
                    .IsNotDeletedLocally(request)
                    .LoadWith(messageInPrivateChat => messageInPrivateChat.Message)
                        //.ThenLoad(message => message.Author)
                    .Where(messageInPrivateChat =>
                        (messageInPrivateChat.ReceiverUserId == request.OtherUserId
                        && messageInPrivateChat.Message.AuthorId == request.AuthorizedUserId)
                        || (messageInPrivateChat.ReceiverUserId == request.AuthorizedUserId
                        && messageInPrivateChat.Message.AuthorId == request.OtherUserId))                    
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
                OtherUser = new Models.User.UserModel
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
            await _db.BeginTransactionAsync();

            await _db.GetTable<PrivateChat>()
                .Where(privateChat => privateChat.OwnerId == request.AuthorizedUserId 
                    && privateChat.OtherUserId == request.OtherUserId)
                .DeleteAsync();

            await _db.GetTable<Message>()
                .Where(message => message.Message2 != null)
                .Where(message => (message.AuthorId == request.AuthorizedUserId
                    && message.Message2!.ReceiverUserId == request.OtherUserId)
                    || (message.AuthorId == request.OtherUserId
                    && message.Message2!.ReceiverUserId == request.AuthorizedUserId))
                .DeleteAsync();

            await _db.CommitTransactionAsync();
        }
        
        public async Task DeleteChat(DeleteChatRequest request)
        {
            await _db.BeginTransactionAsync();

            await _db.GetTable<PrivateChat>()
                .Where(privateChat => (privateChat.OwnerId == request.AuthorizedUserId && privateChat.OtherUserId == request.OtherUserId)
                    || (privateChat.OwnerId == request.OtherUserId && privateChat.OtherUserId == request.AuthorizedUserId))
                .DeleteAsync();

            await _db.GetTable<Message>()
                .Where(message => message.Message2 != null)
                .Where(message => (message.AuthorId == request.AuthorizedUserId
                    && message.Message2!.ReceiverUserId == request.OtherUserId)
                    || (message.AuthorId == request.OtherUserId
                    && message.Message2!.ReceiverUserId == request.AuthorizedUserId))
                .InsertAsync(_db.GetTable<DeletedMessage>(), message => 
                    new DeletedMessage
                    {
                        UserId = request.AuthorizedUserId,
                        MessageId = message.Id
                    });

            await _db.CommitTransactionAsync();
        }

        public async Task<MessageModel> SendPrivateChatMessage(SendPrivateChatMessageRequest request)
        {
            var dateTimeNow = DateTime.UtcNow;

            await _db.BeginTransactionAsync();

            await _db.GetTable<PrivateChat>()
                .InsertOrUpdateAsync(() => new PrivateChat
                {
                    OwnerId = request.AuthorizedUserId,
                    OtherUserId = request.ReceiverId,
                    LastViewedMessageCreateAt = dateTimeNow
                },
                privateChat => new PrivateChat
                {
                    LastViewedMessageCreateAt = dateTimeNow
                });
            
            await _db.GetTable<PrivateChat>()
                .InsertOrUpdateAsync(() => new PrivateChat
                {
                    OwnerId = request.ReceiverId,
                    OtherUserId = request.AuthorizedUserId
                },
                null);

            var insertedMessage = await _db.GetTable<Message>()
                .InsertWithOutputAsync(() => new Message
                {
                    AuthorId = request.AuthorizedUserId,
                    Content = request.Content,
                    CreateAt = dateTimeNow,
                    ModifyAt = null
                });

            await _db.GetTable<MessageInPrivateChat>()
                .InsertAsync(() => new MessageInPrivateChat
                {
                    MessageId = insertedMessage.Id,
                    ReceiverUserId = request.ReceiverId
                });

            await _db.CommitTransactionAsync();
            
            return new MessageModel
            {
                AuthorId = request.AuthorizedUserId,
                ReceiverUserId = request.ReceiverId,
                MessageId = insertedMessage.Id,
                MessageContent = insertedMessage.Content,
                MessageDate = insertedMessage.CreateAt,
                EditedDate = insertedMessage.ModifyAt
            };
        }

        public async Task<ViewMessageResponse> ViewMessage(ViewMessageRequest request)
        {
            var viewedMessage = await _db.GetTable<MessageInPrivateChat>()
                .IsNotDeletedLocally(request)
                .LoadWith(messageInPrivateChat => messageInPrivateChat.Message)
                .FirstOrDefaultAsync(messageInPrivateChat => messageInPrivateChat.MessageId == request.MessageId
                    && messageInPrivateChat.ReceiverUserId == request.AuthorizedUserId);

            if (viewedMessage == null)
                throw new StatusCodeException(HttpStatusCode.NotFound);

            var updatedChatsCount = await _db.GetTable<PrivateChat>()
               .Where(privateChat => privateChat.OwnerId == request.AuthorizedUserId
                   && privateChat.OtherUserId == viewedMessage.Message.AuthorId
                   && privateChat.LastViewedMessageCreateAt < viewedMessage.Message.CreateAt)
               .Set(privateChat => privateChat.LastViewedMessageCreateAt, viewedMessage.Message.CreateAt)
               .UpdateAsync();

            if (updatedChatsCount == 0)
                throw new StatusCodeException(HttpStatusCode.Conflict);

            var unviewedMessagesCount = await _db.GetTable<MessageInPrivateChat>()
                .Where(messageInPrivateChat => messageInPrivateChat.ReceiverUserId == request.AuthorizedUserId
                    && messageInPrivateChat.Message.AuthorId == viewedMessage.Message.AuthorId)
                .CountAsync(messageInPrivateChat => messageInPrivateChat.Message.CreateAt > viewedMessage.Message.CreateAt);

            return new ViewMessageResponse
            {
                ViewedByUserId = request.AuthorizedUserId,
                MessageAuthorId = viewedMessage.Message.AuthorId,
                UnviewedMessagesLeft = unviewedMessagesCount
            };
        }

        public async Task<EditMessageResponse> EditMessage(EditMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                throw new StatusCodeException(HttpStatusCode.BadRequest);

            var dateTimeNow = DateTime.UtcNow;

            var editedMessageQuery = _db.GetTable<Message>()
                .Where(message => message.Id == request.MessageId
                    && message.AuthorId == request.AuthorizedUserId);

            var editedMessage = await editedMessageQuery
                .IsNotDeletedLocally(request)
                .LoadWith(message => message.Message2)
                .FirstOrDefaultAsync();

            if (editedMessage == null)
                throw new StatusCodeException(HttpStatusCode.NotFound);

            if (editedMessage.Content == request.Content)
                throw new StatusCodeException(HttpStatusCode.OK);

            var updatedMessagesCount = await editedMessageQuery
                .Set(message => message.Content, request.Content.Trim())
                .Set(message => message.ModifyAt, dateTimeNow)
                .UpdateAsync();

            return new EditMessageResponse
            {
                EditedByUserId = request.AuthorizedUserId,
                OtherUserId = editedMessage.Message2!.ReceiverUserId,
                MessageId = request.MessageId,
                Content = request.Content
            };
        }

        public async Task<DeleteMessageResponse> DeleteMessageLocally(DeleteMessageRequest request)
        {
            var deletedMessage = await _db.GetTable<MessageInPrivateChat>()
                .LoadWith(messageInPrivateChat => messageInPrivateChat.Message)
                .Where(messageInPrivateChat => messageInPrivateChat.MessageId == request.MessageId
                    && (messageInPrivateChat.Message.AuthorId == request.AuthorizedUserId 
                    || messageInPrivateChat.ReceiverUserId == request.AuthorizedUserId))
                .FirstOrDefaultAsync();

            if (deletedMessage == null)
                throw new StatusCodeException(HttpStatusCode.NotFound);

            await _db.GetTable<DeletedMessage>()
                .InsertOrUpdateAsync(() => new DeletedMessage
                {
                    UserId = request.AuthorizedUserId,
                    MessageId = request.MessageId
                },
                null);

            return new DeleteMessageResponse
            {
                DeletedByUserId = request.AuthorizedUserId,
                OtherUserId = deletedMessage.Message.AuthorId == request.AuthorizedUserId
                    ? deletedMessage.Message.AuthorId : deletedMessage.ReceiverUserId,
                MessageId = request.MessageId
            };
        }     
        
        public async Task<DeleteMessageResponse> DeleteMessage(DeleteMessageRequest request)
        {
            var deletedMessageQuery = _db.GetTable<Message>()
                .Where(message => message.Id == request.MessageId
                    && message.AuthorId == request.AuthorizedUserId);

            var deletedMessage = await deletedMessageQuery
                .IsNotDeletedLocally(request)
                .LoadWith(message => message.Message2)
                .FirstOrDefaultAsync();

            if (deletedMessage == null)
                throw new StatusCodeException(HttpStatusCode.NotFound);

            await deletedMessageQuery.DeleteAsync();

            return new DeleteMessageResponse
            {
                DeletedByUserId = request.AuthorizedUserId,
                OtherUserId = deletedMessage.Message2!.ReceiverUserId,
                MessageId = request.MessageId
            };
        }

        #endregion
    }
}
