using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using InstaLike.Core.Commands;
using InstaLike.Web.Data.Query;
using InstaLike.Web.Extensions;
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

                var command = new PublishNewPostCommand(User.GetIdentifier(), newPost.Text, newPost.Picture);
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

        public async Task<IActionResult> Detail(int id)
        {
            var postQuery = new PostQuery(id);
            var model = await _dispatcher.DispatchAsync(postQuery);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PostComment(NewCommentModel newComment)
        {
            var command = new PublishCommentCommand(newComment.PostID, newComment.CommentText, newComment.AuthorNickName);
            var commandResult = await _dispatcher.DispatchAsync(command);

            // Send Notification
            // Another command here or raise an event?
            
            var commentsQuery = new PostCommentsQuery(newComment.PostID);
            var comments = await _dispatcher.DispatchAsync(commentsQuery);

            return PartialView("_CommentsPartial", comments);
        }

        [HttpPost]
        public async Task<IActionResult> Like(LikePostModel like)
        {
            var command = new LikeOrDislikePostCommand(like.PostID, like.Nickname);
            var commandResult = await _dispatcher.DispatchAsync(command);

            // Send Notification
            // Another command here or raise an event?

            return new EmptyResult();
        }
    }
}