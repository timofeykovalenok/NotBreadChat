using Chat.BLL.Models.Base;

namespace Chat.BLL.Models.User.Requests
{
    public class SearchUsersRequest : BaseAuthorizedRequest
    {
        public required string SearchValue { get; init; }
    }
}
