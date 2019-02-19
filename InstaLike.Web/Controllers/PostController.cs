using System;
using System.IO;
using System.Threading.Tasks;
using InstaLike.Core.Commands;
using InstaLike.Web.Data.Query;
using InstaLike.Web.Infrastructure;
using InstaLike.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly IMessageDispatcher _dispatcher;

        public PostController(IMessageDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public IActionResult Publish()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Publish(PublishNewPostModel newPost, IFormFile pictureFile)
        {
            if (ModelState.IsValid)
            {
                using (var stream = new MemoryStream())
                {
                    await pictureFile.CopyToAsync(stream);
                    newPost.Picture = stream.ToArray();
                }

                var command = new PublishNewPostCommand(newPost.AuthorNickName, newPost.Text, newPost.Picture);
                var commandResult = await _dispatcher.DispatchAsync(command);
                if (commandResult.IsSuccess)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", commandResult.Error);
                }
            }
            else
            {
                ModelState.AddModelError("Picture", "Please select a picture to share together with your post.");
            }

            return View(newPost);
        }

        public async Task<ActionResult> Detail(int id)
        {
            var postQuery = new PostQuery(id);
            var model = await _dispatcher.DispatchAsync(postQuery);
            return View(model);
        }
    }
}