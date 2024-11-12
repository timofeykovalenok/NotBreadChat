using Chat.BLL.Extensions;
using Chat.BLL.Models.Message.Requests;
using Chat.BLL.Models.Message.Responses;
using Chat.BLL.Models.PrivateChat.Responses;
using Chat.BLL.Models.Shared;
using Chat.BLL.Services.Interfaces;
using Context;
using Core.Exceptions;
using LinqToDB;
using System.Net;

namespace Chat.BLL.Services
{
    internal class MessageService : IMessageService
    {
        #region Injects

        private readonly ChatContext _context;

        #endregion

        #region Ctors

        public MessageService(ChatContext context)
        {
            _context = context;
        }

        #endregion

        #region Public Methods

        public async Task<MessageModel> SendPrivateChatMessage(SendPrivateChatMessageRequest request)
        {
            var dateTimeNow = DateTime.UtcNow;

            await _context.BeginTransactionAsync();

            await _context.GetTable<PrivateChat>()
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

            await _context.GetTable<PrivateChat>()
                .InsertOrUpdateAsync(() => new PrivateChat
                {
                    OwnerId = request.ReceiverId,
                    OtherUserId = request.AuthorizedUserId
                },
                null);

            var insertedMessage = await _context.GetTable<Message>()
                .InsertWithOutputAsync(() => new Message
                {
                    AuthorId = request.AuthorizedUserId,
                    Content = request.Content,
                    CreateAt = dateTimeNow,
                    ModifyAt = null
                });

            await _context.GetTable<MessageInPrivateChat>()
                .InsertAsync(() => new MessageInPrivateChat
                {
                    MessageId = insertedMessage.Id,
                    ReceiverUserId = request.ReceiverId
                });

            await _context.CommitTransactionAsync();

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
            var viewedMessage = await _context.GetTable<MessageInPrivateChat>()
                .IsNotDeletedLocally(request)
                .LoadWith(messageInPrivateChat => messageInPrivateChat.Message)
                .FirstOrDefaultAsync(messageInPrivateChat => messageInPrivateChat.MessageId == request.MessageId
                    && messageInPrivateChat.ReceiverUserId == request.AuthorizedUserId);

            if (viewedMessage == null)
                throw new StatusCodeException(HttpStatusCode.NotFound);

            var updatedChatsCount = await _context.GetTable<PrivateChat>()
               .Where(privateChat => privateChat.OwnerId == request.AuthorizedUserId
                   && privateChat.OtherUserId == viewedMessage.Message.AuthorId
                   && privateChat.LastViewedMessageCreateAt < viewedMessage.Message.CreateAt)
               .Set(privateChat => privateChat.LastViewedMessageCreateAt, viewedMessage.Message.CreateAt)
               .UpdateAsync();

            if (updatedChatsCount == 0)
                throw new StatusCodeException(HttpStatusCode.Conflict);

            var unviewedMessagesCount = await _context.GetTable<MessageInPrivateChat>()
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

            var editedMessageQuery = _context.GetTable<Message>()
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
            var deletedMessage = await _context.GetTable<MessageInPrivateChat>()
                .LoadWith(messageInPrivateChat => messageInPrivateChat.Message)
                .Where(messageInPrivateChat => messageInPrivateChat.MessageId == request.MessageId
                    && (messageInPrivateChat.Message.AuthorId == request.AuthorizedUserId
                    || messageInPrivateChat.ReceiverUserId == request.AuthorizedUserId))
                .FirstOrDefaultAsync();

            if (deletedMessage == null)
                throw new StatusCodeException(HttpStatusCode.NotFound);

            await _context.GetTable<DeletedMessage>()
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
            var deletedMessageQuery = _context.GetTable<Message>()
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
