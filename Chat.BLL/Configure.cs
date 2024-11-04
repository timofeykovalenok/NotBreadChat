using Chat.BLL.Interfaces;
using Chat.BLL.Services;
using Chat.DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.BLL
{
    public static class Configure
    {
        public static IServiceCollection AddBLL(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMessageService, MessageService>();

            services.AddDAL(configuration);
            return services;
        }
    }
}
