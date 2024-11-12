using Chat.BLL.Models.Base;

namespace Chat.BLL.Models.Message.Requests
{
    public class ViewMessageRequest : BaseAuthorizedRequest
    {
        public required long MessageId { get; init; }
    }
}
