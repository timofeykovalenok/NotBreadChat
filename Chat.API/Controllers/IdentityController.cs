using Chat.API.Helpers;
using Chat.BLL.Interfaces;
using Chat.BLL.Models.User;
using Core;
using Core.Extensions;
using LinqToDB.Schema;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Chat.API.Controllers
{
    [AllowAnonymous]
    public class IdentityController : Controller
    {
        #region Injects

        private readonly IUserService _userService;

        #endregion

        #region Ctors

        public IdentityController(IUserService userService)
        {
            _userService = userService;
        }

        #endregion

        #region Filters

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (UserHelper.GetUserId(HttpContext) == null)
                return;

            context.Result = RedirectToAction(nameof(HomeController.Index), nameof(HomeController).WithoutControllerSuffix());
        }

        #endregion

        #region Endpoints

        [HttpGet("/login")]
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login(LoginRequest request, string? returnUrl)
        {
            var result = await _userService.Login(request);

            return await SignIn(result.UserId, returnUrl);
        }

        [HttpGet("/registration")]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost("/registration")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _userService.Register(request);

            return await SignIn(result.UserId);
        }

        //[HttpPost("/logout")]
        //public async Task<IActionResult> Logout()
        //{
        //    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        //    return LocalRedirect("/login");
        //}

        #endregion

        #region Private Methods

        private async Task<IActionResult> SignIn(long userId, string? returnUrl = null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true
                });

            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).WithoutControllerSuffix());

            return LocalRedirect($"~{returnUrl}");
        }

        #endregion
    }
}
