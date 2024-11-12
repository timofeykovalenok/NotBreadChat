using Chat.BLL.Models.Shared;

namespace Chat.BLL.Models.User.Responses
{
    public class SearchUsersResponse
    {
        public required IEnumerable<UserModel> Users { get; init; }
    }       
}
