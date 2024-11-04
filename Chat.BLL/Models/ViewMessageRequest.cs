using Chat.BLL.Models.Base;

namespace Chat.BLL.Models
{
    public class ViewMessageRequest : BaseAuthorizedRequest
    {
        public required long MessageId { get; init; }
    }
}
