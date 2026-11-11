using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Core.Social;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Social.CommandHandlers;

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
            var followedID = await _session.QueryOver<User>()
                .Where(Restrictions.Eq("Nickname", request.FollowedNickname))
                .Select(u => u.ID)
                .SingleOrDefaultAsync<int?>(cancellationToken);
            if (followedID is not int followedUserID)
            {
                return Result.Failure($"User [{request.FollowedNickname}] does not exist.");
            }

            var alreadyFollowing = await _session.QueryOver<Follow>()
                .Where(f => f.FollowerID == request.FollowerID && f.FollowedID == followedUserID)
                .RowCountAsync(cancellationToken) > 0;
            if (alreadyFollowing)
            {
                return Result.Failure($"User {request.FollowerID} is already following user [{request.FollowedNickname}].");
            }

            try
            {
                await _session.SaveAsync(new Follow(request.FollowerID, followedUserID), cancellationToken);
                await tx.CommitAsync(cancellationToken);

                _logger.Information("User {UserID} started following user [{FollowedUserNickname}({FollowedUserID})]",
                    request.FollowerID,
                    request.FollowedNickname,
                    followedUserID);

                return Result.Success();
            }
            catch (ADOException ex)
            {
                await tx.RollbackAsync(cancellationToken);

                _logger.Error("Error while following [{FollowedUserNickname}({FollowedUserID})] by user {UserID}. Error message: {ErrorMessage}",
                    request.FollowedNickname,
                    followedUserID,
                    request.FollowerID,
                    ex.Message);

                return Result.Failure(ex.Message);
            }
        }
    }
}
