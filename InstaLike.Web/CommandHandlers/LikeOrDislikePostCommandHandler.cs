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
            using (var tx = _session.BeginTransaction())
            {
                var user = await _session.LoadAsync<User>(request.UserID);
                var post = await GetPostAsync(request.UserID, request.PostID);

                return await user.PutLikeTo(post)
                    .OnSuccessTry(async () => await tx.CommitAsync())
                        .OnSuccess(_ => _logger
                            .ForContext<LikePostCommand>()
                            .Information("User [{Nickname}({UserID})] put a 'Like' on post {PostID}",
                                user.Nickname,
                                request.UserID,
                                request.PostID))
                        .OnFailure(async errorMessage =>
                        {
                            await tx.RollbackAsync();
                            _logger
                                .ForContext<LikePostCommand>()
                                .Error("Failed to put a 'Like' on post {PostID} by user [{Nickname}({UserID})]. Error message: {ErrorMessage}",
                                    request.PostID,
                                    user.Nickname,
                                    request.UserID,
                                    errorMessage);
                        });
            }
        }

        public async Task<Result> Handle(DislikePostCommand request, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                var user = await _session.LoadAsync<User>(request.UserID);
                var post = await GetPostAsync(request.UserID, request.PostID);

                return await user.RemoveLikeFrom(post)
                    .OnSuccessTry(async () => await tx.CommitAsync())
                        .OnSuccess(_ => _logger
                            .ForContext<DislikePostCommand>()
                            .Information("User [{Nickname}({UserID})] removed a 'Like' on post {PostID}",
                                user.Nickname,
                                request.UserID,
                                request.PostID))
                        .OnFailure(async errorMessage => 
                        {
                            await tx.RollbackAsync();
                            _logger
                                .ForContext<DislikePostCommand>()
                                .Error("Failed to remove a 'Like' on post {PostID} by user [{Nickname}({UserID})]. Error message: {ErrorMessage}",
                                    request.PostID,
                                    user.Nickname,
                                    request.UserID,
                                    errorMessage);
                        });
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