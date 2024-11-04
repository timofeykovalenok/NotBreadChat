using Chat.BLL.Models.Base;

namespace Chat.BLL.Models
{
    public class EditMessageRequest : BaseAuthorizedRequest
    {
        public required long MessageId { get; init; }
        public required string Content { get; init; }
    }
}
