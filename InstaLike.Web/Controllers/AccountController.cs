using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Profile(string id)
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
    }
}