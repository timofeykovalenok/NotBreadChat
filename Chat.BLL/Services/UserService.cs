using Azure.Core;
using Chat.BLL.Models.Shared;
using Chat.BLL.Models.User.Requests;
using Chat.BLL.Models.User.Responses;
using Chat.BLL.Services.Interfaces;
using Context;
using Core.Exceptions;
using Core.Extensions;
using LinqToDB;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Chat.BLL.Services
{
    internal class UserService : IUserService
    {
        #region Injects

        private readonly ChatContext _context;

        #endregion

        #region Ctors

        public UserService(ChatContext context)
        {
            _context = context;
        }

        #endregion

        #region Public Methods 

        public async Task<LoginResponse> Login(LoginRequest request, CancellationToken ctn = default)
        {
            if (!IsCredentialsValid(request.Login, request.Password))
                throw new StatusCodeException(HttpStatusCode.BadRequest);

            var user = await _context.GetTable<User>()
                .FirstOrDefaultAsync(user => user.Login == request.Login, ctn)
                ?? throw new StatusCodeException(HttpStatusCode.Unauthorized, "Неверный логин или пароль");

            var salt = Convert.FromBase64String(user.PasswordSalt);
            var passwordHash = GetPasswordHash(request.Password, salt);

            var savedPasswordHash = Convert.FromBase64String(user.PasswordHash);
            if (!passwordHash.SequenceEqual(savedPasswordHash))
                throw new StatusCodeException(HttpStatusCode.Unauthorized, "Неверный логин или пароль");

            return new LoginResponse
            {
                UserId = user.Id
            };
        }

        public async Task<RegisterResponse> Register(RegisterRequest request, CancellationToken ctn = default)
        {
            if (!IsCredentialsValid(request.Login, request.Password) || string.IsNullOrWhiteSpace(request.UserName))
                throw new StatusCodeException(HttpStatusCode.BadRequest);

            var salt = RandomNumberGenerator.GetBytes(16);
            var passwordHash = GetPasswordHash(request.Password, salt);

            var trimmedLogin = request.Login.Trim();
            if (await _context.GetTable<User>().AnyAsync(x => x.Login == trimmedLogin, ctn))
                throw new StatusCodeException(HttpStatusCode.Conflict, "Пользователь с таким логином уже существует");

            var userId = await _context.GetTable<User>().InsertWithInt64IdentityAsync(() =>
                new User
                {
                    Login = trimmedLogin,
                    Name = request.UserName.Trim(),
                    PasswordHash = Convert.ToBase64String(passwordHash),
                    PasswordSalt = Convert.ToBase64String(salt),
                },
                ctn);

            return new RegisterResponse
            {
                UserId = userId
            };
        }

        public async Task<GetUserResponse> GetUser(long id)
        {
            var user = await _context.GetTable<User>()
                .FirstOrDefaultAsync(user => user.Id == id)
                ?? throw new StatusCodeException(HttpStatusCode.NotFound);

            return new GetUserResponse
            {
                UserId = user.Id,
                Username = user.Name,
                UserImage = user.Image
            };
        }

        public async Task<SearchUsersResponse> SearchUsers(SearchUsersRequest request)
        {
            var usersQuery = _context.GetTable<User>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
                usersQuery = usersQuery.Where(user => Sql.Like(user.Name, $"%{request.SearchValue}%"));

            var users = await usersQuery                
                .ToListAsync();

            var usersModels = users.Select(user => new UserModel
            {
                UserId = user.Id,
                Username = user.Name,
                Image = user.Image
            });

            return new SearchUsersResponse
            {
                Users = usersModels
            };
        }

        public Task DeleteUser(long id, CancellationToken ctn)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        private static bool IsCredentialsValid(string login, string password) =>
            !string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(password);

        private static byte[] GetPasswordHash(string password, byte[] salt)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var saltedPassword = passwordBytes.ConcatBytes(salt);

            return SHA256.HashData(saltedPassword);
        }

        #endregion
    }
}
