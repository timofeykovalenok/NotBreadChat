using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Chat.API.Helpers
{
    public static class UserHelper
    {
        #region Public Methods

        public static long? GetUserId(HubCallerContext context) =>
            context.User != null
            ? GetUserId(context.User)
            : null;

        public static long? GetUserId(HttpContext context) =>
            GetUserId(context.User);

        #endregion

        #region Private Methods

        private static long? GetUserId(ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null || !long.TryParse(claim.Value, out var result))
                return null;

            return result;                
        }

        #endregion
    }
}
