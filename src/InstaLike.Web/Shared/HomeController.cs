using System.Diagnostics;
using System.Threading.Tasks;
using InstaLike.Core.Posts;
using InstaLike.Web.Extensions;
using InstaLike.Web.Posts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Shared
{
    [Authorize]
    public sealed class HomeController : Controller
    {
        private const int PostsPerBatch = 10;

        private readonly ISender _dispatcher;

        public HomeController(ISender dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task<IActionResult> Index()
        {
            var query = new TimelineQuery(User.GetIdentifier(), PostsPerBatch);
            var model = await _dispatcher.Send(query);

            return View(model);
        }

        public async Task<IActionResult> OlderPosts(PostId id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var query = new OlderTimelinePostsQuery(User.GetIdentifier(), id, PostsPerBatch);
            var model = await _dispatcher.Send(query);

            return PartialView("_TimelinePostsPartial", model);
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
