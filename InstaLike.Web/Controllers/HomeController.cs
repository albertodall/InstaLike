using System;
using System.Diagnostics;
using System.Threading.Tasks;
using InstaLike.Web.Models;
using InstaLike.Web.Data.Query;
using InstaLike.Web.Extensions;
using InstaLike.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace InstaLike.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IMessageDispatcher _dispatcher;

        public HomeController(IMessageDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public async Task<IActionResult> Index()
        {
            var query = new TimelineQuery(User.GetIdentifier());
            var model = await _dispatcher.DispatchAsync(query);
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
