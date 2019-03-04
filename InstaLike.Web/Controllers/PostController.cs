using System;
using System.IO;
using System.Threading.Tasks;
using InstaLike.Core.Commands;
using InstaLike.Core.Events;
using InstaLike.Web.Data.Query;
using InstaLike.Web.Extensions;
using InstaLike.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly IMediator _dispatcher;

        public PostController(IMediator dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public IActionResult Publish()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Publish(PublishPostModel newPost, IFormFile pictureFile)
        {
            if (ModelState.IsValid)
            {
                using (var stream = new MemoryStream())
                {
                    await pictureFile.CopyToAsync(stream);
                    newPost.Picture = stream.ToArray();
                }

                var command = new PublishPostCommand(User.GetIdentifier(), newPost.Text, newPost.Picture);
                var commandResult = await _dispatcher.Send(command);
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
            var postQuery = new PostDetailQuery(id, User.GetIdentifier());
            var model = await _dispatcher.Send(postQuery);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PublishComment(PublishCommentModel newComment)
        {
            var command = new PublishCommentCommand(newComment.PostID, newComment.CommentText, User.GetIdentifier());
            var commandResult = await _dispatcher.Send(command);

            var newCommentNotification = new CommentPublishedEvent(
                User.Identity.Name,
                Url.Action("Profile", "Account", new { id = User.Identity.Name }),
                newComment.PostID,
                Url.Action("Detail", "Post", new { id = newComment.PostID })
            );

            await _dispatcher.Publish(newCommentNotification);

            var commentsQuery = new PostCommentsQuery(newComment.PostID);
            var comments = await _dispatcher.Send(commentsQuery);

            return PartialView("_CommentsPartial", comments);
        }

        [HttpPost]
        public async Task<IActionResult> Like(LikePostModel like)
        {
            var command = new LikeOrDislikePostCommand(like.PostID, like.UserID);
            var commandResult = await _dispatcher.Send(command);

            // Send Notification
            // Another command here or raise an event?

            return new EmptyResult();
        }
    }
}