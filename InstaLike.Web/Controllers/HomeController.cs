using System;
using System.Diagnostics;
using System.Threading.Tasks;
using InstaLike.Web.Models;
using InstaLike.Web.Data.Query;
using InstaLike.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;

namespace InstaLike.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private const int MaxNumberOfPosts = 10;

        private readonly IMediator _dispatcher;

        public HomeController(IMediator dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public async Task<IActionResult> Index()
        {
            var query = new TimelineQuery(User.GetIdentifier(), MaxNumberOfPosts);
            var model = await _dispatcher.Send(query);

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
