using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Data;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts
{
    public sealed class DeletePostCommand : ICommand<Result>
    {
        public MemberId UserID { get; }
        public PostId PostID { get; }

        public DeletePostCommand(MemberId userID, PostId postID)
        {
            UserID = userID;
            PostID = postID;
        }
    }

    internal sealed class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, Result>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public DeletePostCommandHandler(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<DeletePostCommand>();
        }

        public async Task<Result> Handle(DeletePostCommand request, CancellationToken cancellationToken)
        {
            var postToDelete = await _session.GetAsync<Post>(request.PostID, cancellationToken);
            if (postToDelete is null)
            {
                return Result.Failure(PostErrors.NotFound(request.PostID));
            }

            var deletion = postToDelete.DeleteBy(request.UserID);
            if (deletion.IsFailure)
            {
                _logger.Warning("User {UserID} tried to delete post {PostID} but was not allowed to.",
                    request.UserID,
                    request.PostID);
                return deletion;
            }

            // No flush here: the unit of work flushes after the PostDeletedEvent handlers
            // have removed the rows of other contexts that reference the post.
            await _session.DeleteAsync(postToDelete, cancellationToken);

            _logger.Information("User {UserID} deleted post {PostID}.", request.UserID, request.PostID);

            return Result.Ok();
        }
    }
}
