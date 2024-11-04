using Chat.API.Helpers;
using Chat.BLL.Models.Base;
using Core.Exceptions;
using Microsoft.AspNetCore.SignalR;
using System.Net;

namespace Chat.API.Filters.SignalR
{
    public class AddUserDataHubFilter : IHubFilter
    {
        public ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
        {
            var authorizedRequest = invocationContext.HubMethodArguments.FirstOrDefault(x => x is BaseAuthorizedRequest)
                as BaseAuthorizedRequest;            

            if (authorizedRequest != null)
            {
                authorizedRequest.AuthorizedUserId = UserHelper.GetUserId(invocationContext.Context)
                    ?? throw new StatusCodeException(HttpStatusCode.Unauthorized);
            }

            return next.Invoke(invocationContext);
        }
    }
}
