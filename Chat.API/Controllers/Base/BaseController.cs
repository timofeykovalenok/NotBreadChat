using Microsoft.AspNetCore.Mvc;

namespace Chat.API.Controllers.Base
{
    public abstract class BaseController : Controller
    {
        public IActionResult SpaView(object? model = null)
        {
            Response.Headers.CacheControl = "no-store";

            return Request.Headers["Sec-Fetch-Mode"] == "navigate"
                ? View(model)
                : PartialView(model);
        }
    }
}
