using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class PublishPostCommandHandler : IRequestHandler<PublishPostCommand, Result>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public PublishPostCommandHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<PublishPostCommand>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(PublishPostCommand request, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                User author = null;
                try
                {
                    author = await _session.LoadAsync<User>(request.UserID);

                    var post = new Post(author, (Picture)request.PictureRawBytes, (PostText)request.Text);
                    await _session.SaveAsync(post);
                    await tx.CommitAsync();

                    _logger.Information("User {UserID} ({Nickname}) has just shared a new post.",
                        request.UserID,
                        author.Nickname);

                    return Result.Ok(post.ID);
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    _logger.Error("Failed to publish a post for user {UserID} ({Nickname}). Error message: {ErrorMessage}",
                        request.UserID,
                        author.Nickname,
                        ex.Message);

                    return Result.Fail(ex.Message);
                }
            }
        }
    }
}
