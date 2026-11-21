using System.Threading.Tasks;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Extensions;
using InstaLike.Web.Media;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InstaLike.Web.Posts
{
    [Authorize]
    public sealed class PostController : Controller
    {
        private const string MissingPost = "This post does not exist.";

        private readonly ISender _dispatcher;
        private readonly IImageRecognitionService _imageRecognition;

        public PostController(ISender dispatcher, IImageRecognitionService imageRecognition)
        {
            _dispatcher = dispatcher;
            _imageRecognition = imageRecognition;
        }

        public IActionResult Publish()
        {
            return View(new PublishPostModel());
        }

        [HttpPost]
        public async Task<IActionResult> Publish(PublishPostModel newPost)
        {
            if (ModelState.IsValid)
            {
                var command = new PublishPostCommand(User.GetIdentifier(), newPost.Text, await newPost.PictureFile!.ToByteArrayAsync());
                var commandResult = await _dispatcher.Send(command);
                if (commandResult.IsSuccess)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelErrors(commandResult.Error);
            }

            return View(newPost);
        }

        public async Task<IActionResult> Detail(PostId id)
        {
            var postQuery = new PostDetailQuery(id, User.GetIdentifier());
            var model = await _dispatcher.Send(postQuery);

            return View(model);
        }

        public async Task<IActionResult> Edit(PostId id)
        {
            var editPostQuery = new EditPostQuery(id, User.GetIdentifier());
            var queryResult = await _dispatcher.Send(editPostQuery);

            if (queryResult.IsSuccess)
            {
                return View(queryResult.Value);
            }

            return queryResult.Error.Type == ErrorType.Forbidden ? Forbid() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditPostModel editedPost)
        {
            if (ModelState.IsValid)
            {
                var command = new EditPostCommand(User.GetIdentifier(), editedPost.PostID, editedPost.Text, await editedPost.PictureFile!.ToByteArrayAsync());
                var commandResult = await _dispatcher.Send(command);
                if (commandResult.IsSuccess)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (commandResult.Error.Type == ErrorType.Forbidden)
                {
                    return Forbid();
                }

                if (commandResult.Error.Type == ErrorType.NotFound)
                {
                    return NotFound();
                }

                ModelState.AddModelErrors(commandResult.Error);
            }

            return View(editedPost);
        }

        [HttpPost]
        public async Task<IActionResult> PublishComment(PublishCommentModel newComment)
        {
            if (ModelState.GetFieldValidationState(nameof(PublishCommentModel.PostID)) == ModelValidationState.Invalid)
            {
                return BadRequest(MissingPost);
            }

            var command = new PublishCommentCommand(newComment.PostID, newComment.CommentText, User.GetIdentifier());
            await _dispatcher.Send(command);

            var commentsQuery = new PostCommentsQuery(newComment.PostID);
            var comments = await _dispatcher.Send(commentsQuery);

            return PartialView("_CommentsPartial", comments);
        }

        [HttpPost]
        public async Task<IActionResult> Like(LikePostModel like)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(MissingPost);
            }

            var command = new LikePostCommand(like.PostID, User.GetIdentifier());
            var commandResult = await _dispatcher.Send(command);

            return commandResult.IsSuccess ? Ok() : BadRequest(commandResult.Error.Description);
        }

        [HttpPost]
        public async Task<IActionResult> Dislike(LikePostModel like)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(MissingPost);
            }

            var command = new DislikePostCommand(like.PostID, User.GetIdentifier());
            var commandResult = await _dispatcher.Send(command);

            return commandResult.IsSuccess ? Ok() : BadRequest(commandResult.Error.Description);
        }

        [HttpPost]
        public async Task<IActionResult> Autotag()
        {
            var formFiles = Request.Form.Files;
            if (formFiles.Count > 0)
            {
                Result<string[]> taggingResult;

                await using (var pictureStream = await formFiles[0].ToStreamAsync())
                {
                    taggingResult = await _imageRecognition.AutoTagImage(pictureStream, formFiles[0].ContentType);
                }

                if (taggingResult.IsSuccess)
                {
                    return Ok(taggingResult.Value);
                }

                return BadRequest(taggingResult.Error.Description);
            }

            return BadRequest("No files selected");
        }
    }
}
