using Chat.BLL.Interfaces;
using Chat.BLL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.API.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;

        public HomeController(IUserService userService, IMessageService messageService)
        {
            _userService = userService;
            _messageService = messageService;
        }

        public IActionResult Index()
        {
            return SpaView();
        }

        [Route("/{OtherUserId:long}")]
        public async Task<IActionResult> PrivateChat([FromRoute] GetPrivateChatRequest request)
        {
            var response = await _messageService.GetPrivateChat(request);

            return SpaView(response);
        }

        private IActionResult SpaView(object? model = null)
        {
            Response.Headers.CacheControl = "no-store";

            return Request.Headers.Accept.FirstOrDefault()?.Contains("text/html") ?? false
                ? View(model)
                : PartialView(model);
        }
    }
}
