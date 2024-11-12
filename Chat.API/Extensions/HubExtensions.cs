using Microsoft.AspNetCore.SignalR;

namespace Chat.API.Extensions
{
    public static class HubExtensions
    {
        public static IClientProxy User(this IHubCallerClients clients,  long userId) =>
            clients.User(userId.ToString());
        
        public static IClientProxy Users(this IHubCallerClients clients, params long[] usersIds) =>
            clients.Users(usersIds.Select(id => id.ToString()));
        
        public static IDictionary<long, IClientProxy> UsersAsCollection(this IHubCallerClients clients, params long[] usersIds)
        {
            var users = new Dictionary<long, IClientProxy>();

            foreach (var userId in usersIds)
            {
                 users.TryAdd(userId, clients.User(userId));
            }

            return users;
        }   

        public static async Task ForEachUserSendAsync<TData>(
            this IDictionary<long, IClientProxy> users,
            string hubMethod,
            Func<long, Task<TData>> userDataFunc,
            CancellationToken ctn = default)
        {
            foreach (var user in users)
            {
                var data = await userDataFunc.Invoke(user.Key);
                _ = user.Value.SendAsync(hubMethod, data, ctn);
            }
        }
    }
}
