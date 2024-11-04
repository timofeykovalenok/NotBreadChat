using Chat.BLL.Models.Base;
using Context;
using LinqToDB;

namespace Chat.BLL.Extensions
{
    public static class QueryExtensions
    {
        public static IQueryable<Message> IsNotDeletedLocally(
            this IQueryable<Message> messages,
            BaseAuthorizedRequest request)
        {
            return messages
                .Where(message => !message.MessageDeletedMessages
                    .Any(deletedMessage => deletedMessage.UserId == request.AuthorizedUserId));
        }
        
        public static IQueryable<MessageInPrivateChat> IsNotDeletedLocally(
            this IQueryable<MessageInPrivateChat> messagesInPrivateChats,
            BaseAuthorizedRequest request)
        {
            return messagesInPrivateChats
                .Where(messageInPrivateChat => !messageInPrivateChat.Message.MessageDeletedMessages
                    .Any(deletedMessage => deletedMessage.UserId == request.AuthorizedUserId));
        }
    }
}
