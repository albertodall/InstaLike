using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using InstaLike.Core.Services;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class PublishPostCommandHandler : IRequestHandler<PublishPostCommand, Result<int>>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;
        private readonly ISequentialIdGenerator<Guid> _idGenerator;

        public PublishPostCommandHandler(ISession session, ILogger logger, ISequentialIdGenerator<Guid> idGenerator)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<PublishPostCommand>() ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        public async Task<Result<int>> Handle(PublishPostCommand request, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                User author = null;
                try
                {
                    author = await _session.LoadAsync<User>(request.UserID, cancellationToken);
                    var postPicture = Picture.Create(request.PictureRawBytes, _idGenerator.GetNextId()).Value;

                    var post = new Post(author, postPicture, (PostText)request.Text);

                    await _session.SaveAsync(post, cancellationToken);
                    await tx.CommitAsync(cancellationToken);

                    _logger.Information("User [{Nickname}({UserID})] has just shared a new post.",
                        author.Nickname,
                        request.UserID);

                    return Result.Ok(post.ID);
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync(cancellationToken);

                    _logger.Error("Failed to publish a post for user [{Nickname}({UserID})]. Error message: {ErrorMessage}",
                        author?.Nickname,
                        request.UserID,
                        ex.Message);

                    return Result.Fail<int>(ex.Message);
                }
            }
        }
    }
}
