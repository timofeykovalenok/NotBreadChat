using Chat.API.Helpers;
using Chat.BLL.Models.Base;
using Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Chat.API.Filters
{
    public class AddUserDataFilter : IActionFilter
    {
        #region Public Methods

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = UserHelper.GetUserId(context.HttpContext);            

            var authorizedRequest = context.ActionArguments.Values.FirstOrDefault(x => x is BaseAuthorizedRequest)
                as BaseAuthorizedRequest;

            if (authorizedRequest != null)
            {
                authorizedRequest.AuthorizedUserId = userId
                    ?? throw new StatusCodeException(HttpStatusCode.Unauthorized);
            }

            if (context.Controller is Controller controller)
                controller.ViewBag.UserId = userId;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        #endregion
    }
}
