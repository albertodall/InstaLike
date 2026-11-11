using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts.CommandHandlers;

internal sealed class PublishPostCommandHandler : IRequestHandler<PublishPostCommand, Result<int>>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public PublishPostCommandHandler(ISession session, ILogger logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<PublishPostCommand>() ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<int>> Handle(PublishPostCommand request, CancellationToken cancellationToken)
    {
        using (var tx = _session.BeginTransaction())
        {
            User? author = null;
            try
            {
                author = await _session.LoadAsync<User>(request.UserID, cancellationToken);
                var postPicture = Picture.Create(request.PictureRawBytes, Guid.NewGuid()).Value;

                var post = new Post(author, postPicture, (PostText)request.Text);

                await _session.SaveAsync(post, cancellationToken);
                await tx.CommitAsync(cancellationToken);

                _logger.Information("User [{Nickname}({UserID})] has just shared a new post.",
                    author.Nickname,
                    request.UserID);

                return Result.Success(post.ID);
            }
            catch (ADOException ex)
            {
                await tx.RollbackAsync(cancellationToken);

                _logger.Error("Failed to publish a post for user [{Nickname}({UserID})]. Error message: {ErrorMessage}",
                    author?.Nickname,
                    request.UserID,
                    ex.Message);

                return Result.Failure<int>(ex.Message);
            }
        }
    }
}
