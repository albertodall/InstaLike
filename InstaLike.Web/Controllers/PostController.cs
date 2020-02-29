using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using InstaLike.Core.Events;
using InstaLike.Web.Data.Query;
using InstaLike.Web.Extensions;
using InstaLike.Web.Models;
using InstaLike.Web.Services;
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
        private readonly IImageRecognitionService _imageRecognition;

        public PostController(IMediator dispatcher, IImageRecognitionService imageRecognition)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _imageRecognition = imageRecognition ?? throw new ArgumentNullException(nameof(imageRecognition));
        }

        public IActionResult Publish()
        {
            var model = new PublishPostModel()
            {
                Picture = Picture.EmptyPicture
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Publish(PublishPostModel newPost, IFormFile pictureFile)
        {
            if (ModelState.IsValid)
            {
                var command = new PublishPostCommand(User.GetIdentifier(), newPost.Text, await pictureFile.ToByteArrayAsync());
                var commandResult = await _dispatcher.Send(command);
                if (commandResult.IsSuccess)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", commandResult.Error);
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

        [Authorize(Policy = "IsPostAuthor")]
        public async Task<IActionResult> Edit(int id)
        {
            var editPostQuery = new EditPostQuery(id, User.GetIdentifier());
            var model = await _dispatcher.Send(editPostQuery);

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "IsPostAuthor")]
        public async Task<IActionResult> Edit(EditPostModel editedPost, IFormFile pictureFile)
        {
            if (ModelState.IsValid)
            {
                var command = new EditPostCommand(User.GetIdentifier(), editedPost.PostID, editedPost.Text, await pictureFile.ToByteArrayAsync());
                var commandResult = await _dispatcher.Send(command);
                if (commandResult.IsSuccess)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", commandResult.Error);
            }
            else
            {
                ModelState.AddModelError("Picture", "Please select a picture to share together with your post.");
            }

            return View(editedPost);
        }

        [HttpPost]
        public async Task<IActionResult> PublishComment(PublishCommentModel newComment)
        {
            var command = new PublishCommentCommand(newComment.PostID, newComment.CommentText, User.GetIdentifier());
            var commandResult = await _dispatcher.Send(command);

            if (commandResult.IsSuccess)
            {
                var newCommentNotification = new CommentPublishedEvent(
                    User.Identity.Name,
                    Url.Action("Profile", "Account", new { id = User.Identity.Name }),
                    newComment.PostID,
                    Url.Action("Detail", "Post", new { id = newComment.PostID })
                );

                await _dispatcher.Publish(newCommentNotification);
            }
            var commentsQuery = new PostCommentsQuery(newComment.PostID);
            var comments = await _dispatcher.Send(commentsQuery);

            return PartialView("_CommentsPartial", comments);
        }

        [HttpPost]
        public async Task<IActionResult> Like(LikePostModel like)
        {
            var command = new LikePostCommand(like.PostID, like.UserID);
            var commandResult = await _dispatcher.Send(command);

            if (commandResult.IsSuccess)
            {
                var postLikedNotification = new PostLikedEvent(
                    User.Identity.Name,
                    Url.Action("Profile", "Account", new { id = User.Identity.Name }),
                    like.PostID,
                    Url.Action("Detail", "Post", new { id = like.PostID })
                );

                await _dispatcher.Publish(postLikedNotification);
            }

            return commandResult.IsSuccess ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPost]
        public async Task<IActionResult> Dislike(LikePostModel like)
        {
            var command = new DislikePostCommand(like.PostID, like.UserID);
            var commandResult = await _dispatcher.Send(command);

            return commandResult.IsSuccess ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPost]
        public async Task<IActionResult> Autotag()
        {
            var formFiles = Request.Form.Files;
            if (formFiles.Count > 0)
            {
                Result<string[]> taggingResult;

                using (var pictureStream = await Request.Form.Files[0].ToStreamAsync())
                {
                    taggingResult = await _imageRecognition.AutoTagImage(pictureStream);
                }

                if (taggingResult.IsSuccess)
                {
                    return Ok(taggingResult.Value);
                }

                return BadRequest(taggingResult.Error);
            }

            return BadRequest("No files selected");
        }
    }
}