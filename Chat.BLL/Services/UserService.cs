using Azure.Core;
using Chat.BLL.Interfaces;
using Chat.BLL.Models.User;
using Context;
using Core.Exceptions;
using Core.Extensions;
using LinqToDB;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Chat.BLL.Services
{
    internal class UserService : IUserService
    {
        private PostgresDb _db;

        public UserService(PostgresDb db)
        {
            _db = db;
        }

        public Task DeleteUser(long id, CancellationToken ctn)
        {
            throw new NotImplementedException();
        }        

        public async Task<RegisterResponse> Register(RegisterRequest request, CancellationToken ctn = default)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var passwordHash = GetPasswordHash(request.Password, salt);

            if (await _db.GetTable<User>().AnyAsync(x => x.Login == request.Login, ctn))
                throw new StatusCodeException(HttpStatusCode.Conflict);

            var userId = await _db.GetTable<User>().InsertWithInt64IdentityAsync(() =>
                new User
                {
                    Login = request.Login,
                    Name = request.UserName,
                    PasswordHash = Convert.ToBase64String(passwordHash),
                    PasswordSalt = Convert.ToBase64String(salt),
                }, 
                ctn);
            
            return new RegisterResponse
            {
                UserId = userId
            };
        }

        public async Task<LoginResponse> Login(LoginRequest request, CancellationToken ctn = default)
        {
            var user = await _db.GetTable<User>()
                .FirstOrDefaultAsync(user => user.Login == request.Login, ctn)
                ?? throw new StatusCodeException(HttpStatusCode.Unauthorized);

            var salt = Convert.FromBase64String(user.PasswordSalt);
            var passwordHash = GetPasswordHash(request.Password, salt);

            var savedPasswordHash = Convert.FromBase64String(user.PasswordHash);
            if (!passwordHash.SequenceEqual(savedPasswordHash))
                throw new StatusCodeException(HttpStatusCode.Unauthorized);

            return new LoginResponse
            {
                UserId = user.Id
            };
        }

        public async Task<GetUserResponse> GetUser(long id)
        {
            var user = await _db.GetTable<User>()
                .FirstOrDefaultAsync(user => user.Id == id)
                ?? throw new StatusCodeException(HttpStatusCode.NotFound);

            return new GetUserResponse
            {
                UserId = user.Id,
                Username = user.Name,
                UserImage = user.Image
            };
        }

        private static byte[] GetPasswordHash(string password, byte[] salt)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var saltedPassword = passwordBytes.ConcatBytes(salt);

            return SHA256.HashData(saltedPassword);
        }

        public async Task<SearchUsersResponse> SearchUsers(SearchUsersRequest request)
        {
            var users = await _db.GetTable<User>()
                .Where(user => Sql.Like(user.Name, $"%{request.SearchValue}%"))
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
    }
}
