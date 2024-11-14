using Chat.BLL.Models.User.Requests;
using Chat.BLL.Models.User.Responses;
using Chat.BLL.Services;

namespace Chat.BLL.Services.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponse> Login(LoginRequest request, CancellationToken ctn = default);
        Task<RegisterResponse> Register(RegisterRequest request, CancellationToken ctn = default);

        Task<GetUserResponse> GetUser(long id);
        Task<SearchUsersResponse> SearchUsers(SearchUsersRequest request);
        
        //Task UpdateUser(UpdateUserRequest request);
        Task DeleteUser(long id, CancellationToken ctn);

        //Task<GetContactsResponse> GetContacts();
        //Task CreateContact(CreateContactRequest request);
        //Task UpdateContact(UpdateUserRequest request);
        //Task DeleteContact(long id);

    }
}
