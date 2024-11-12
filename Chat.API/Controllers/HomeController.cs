using Chat.API.Controllers.Base;
using Chat.BLL.Models.PrivateChat.Requests;
using Chat.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.API.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly IChatService _chatService;

        public HomeController(IUserService userService, IChatService chatService)
        {
            _chatService = chatService;
        }

        public IActionResult Index()
        {
            return SpaView();
        }

        [Route("/{OtherUserId:long}")]
        public async Task<IActionResult> PrivateChat([FromRoute] GetPrivateChatRequest request)
        {
            var response = await _chatService.GetPrivateChat(request);

            return SpaView(response);
        }        
    }
}
