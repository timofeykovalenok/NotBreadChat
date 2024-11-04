using Chat.BLL.Models.Base;

namespace Chat.BLL.Models
{
    public class DeleteMessageRequest : BaseAuthorizedRequest
    {
        public required long MessageId { get; init; }
    }
}
