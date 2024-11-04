using Chat.BLL.Models.User;

namespace Chat.BLL.Interfaces
{
    public interface IUserService
    {
        Task<GetUserResponse> GetUser(long id);
        Task<RegisterResponse> Register(RegisterRequest request, CancellationToken ctn = default);
        Task<LoginResponse> Login(LoginRequest request, CancellationToken ctn = default);
        //Task UpdateUser(UpdateUserRequest request);
        Task DeleteUser(long id, CancellationToken ctn);

        //Task<GetContactsResponse> GetContacts();
        //Task CreateContact(CreateContactRequest request);
        //Task UpdateContact(UpdateUserRequest request);
        //Task DeleteContact(long id);

        Task<SearchUsersResponse> SearchUsers(SearchUsersRequest request);
    }
}
