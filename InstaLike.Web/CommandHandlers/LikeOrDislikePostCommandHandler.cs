using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class LikeOrDislikePostCommandHandler : IRequestHandler<LikePostCommand, Result>, IRequestHandler<DislikePostCommand, Result>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public LikeOrDislikePostCommandHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(LikePostCommand request, CancellationToken cancellationToken)
        {
            _logger.ForContext<LikePostCommand>()
                .Debug("User {@UserID} is putting a 'Like' on post {@PostID} with the following parameters: {@Request}", 
                    request.UserID, 
                    request.PostID, 
                    request);

            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    var user = await _session.LoadAsync<User>(request.UserID);
                    var post = await GetPostAsync(request.UserID, request.PostID);
                    post.Like(user);

                    await _session.SaveOrUpdateAsync(post);
                    await tx.CommitAsync();
                    return Result.Ok();
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    return Result.Fail<LikePostResult>(ex.Message);
                }
            }
        }

        public async Task<Result> Handle(DislikePostCommand request, CancellationToken cancellationToken)
        {
            _logger.ForContext<DislikePostCommand>()
                .Debug("User {@UserID} is removing a 'Like' on post {@PostID} with the following parameters: {@Request}",
                    request.UserID,
                    request.PostID,
                    request);

            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    var user = await _session.LoadAsync<User>(request.UserID);
                    var post = await GetPostAsync(request.UserID, request.PostID);
                    post.Dislike(user);

                    await _session.SaveOrUpdateAsync(post);
                    await tx.CommitAsync();
                    return Result.Ok();
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    return Result.Fail<LikePostResult>(ex.Message);
                }
            }
        }

        private async Task<Post> GetPostAsync(int userID, int postID)
        {
            Like likeAlias = null;

            
            var postQuery = _session.QueryOver<Post>()
                .Left.JoinAlias(p => p.Likes, () => likeAlias)
                .Where(p => p.ID == postID)
                .And(
                    Restrictions.Disjunction()
                        .Add(() => likeAlias.User.ID == userID)
                        .Add(Restrictions.On(() => likeAlias.User).IsNull)
                    );

            return await postQuery.SingleOrDefaultAsync();
        }
    }
}
