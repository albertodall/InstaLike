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
    internal sealed class FollowCommandHandler : IRequestHandler<FollowCommand, Result>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public FollowCommandHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<FollowCommand>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(FollowCommand request, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                var follower = await _session.LoadAsync<User>(request.FollowerID);
                var followedUser = await _session.QueryOver<User>()
                    .Where(Restrictions.Eq("Nickname", request.FollowedNickname))
                    .SingleOrDefaultAsync();

                return await follower.Follow(followedUser)
                    .OnSuccessTry(async () => await tx.CommitAsync())
                        .OnSuccess(_ =>
                            _logger.Information("User {UserID} started following user [{FollowedUserNickname}({FollowedUserID})])",
                                request.FollowerID,
                                request.FollowedNickname,
                                followedUser.ID)
                        )
                        .OnFailure(async errorMessage =>
                        {
                            await tx.RollbackAsync();

                            _logger.Error("Error while following [{FollowedUserNickname}({FollowedUserID})] by user {UserID}. Error message: {ErrorMessage}",
                                request.FollowedNickname,
                                followedUser.ID,
                                request.FollowerID,
                                errorMessage);
                        });
            }
        }
    }
}
