using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Controllers
{
    public class PostController : Controller
    {
        public IActionResult Publish()
        {
            return View();
        }

        public IActionResult Detail(int id)
        {
            return View();
        }
    }
}