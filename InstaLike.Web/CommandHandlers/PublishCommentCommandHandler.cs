using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;
using Serilog;

#nullable disable

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class PublishCommentCommandHandler : IRequestHandler<PublishCommentCommand, Result<int>>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public PublishCommentCommandHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<PublishCommentCommand>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<int>> Handle(PublishCommentCommand request, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                User author = null;
                try
                {
                    author = await _session.LoadAsync<User>(request.UserID, cancellationToken);
                    var post = await _session.LoadAsync<Post>(request.PostID, cancellationToken);

                    var comment = new Comment(post, author, (CommentText)request.Text);
                    post.AddComment(comment);

                    await tx.CommitAsync(cancellationToken);

                    _logger.Information("User [{NickName}({UserID})] wrote a new comment to post {PostID}",
                        request.UserID,
                        author.Nickname,
                        request.PostID);

                    return Result.Success(comment.ID);
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync(cancellationToken);

                    _logger.Error("Failed to save comment to post {PostID} written by user [{NickName}({UserID})]. Error message: {ErrorMessage}",
                        request.PostID,
                        author?.Nickname,
                        request.UserID,
                        ex.Message);

                    return Result.Failure<int>(ex.Message);
                }
            }
        }
    }
}
