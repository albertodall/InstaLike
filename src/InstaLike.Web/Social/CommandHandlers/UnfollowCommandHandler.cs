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

internal sealed class UnfollowCommandHandler : IRequestHandler<UnfollowCommand, Result>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public UnfollowCommandHandler(ISession session, ILogger logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<UnfollowCommand>() ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(UnfollowCommand request, CancellationToken cancellationToken)
    {
        var unfollowedID = await _session.QueryOver<User>()
            .Where(Restrictions.Eq("Nickname", request.UnfollowedNickname))
            .Select(u => u.ID)
            .SingleOrDefaultAsync<int?>(cancellationToken);
        if (unfollowedID is not int unfollowedUserID)
        {
            return Result.Failure($"User [{request.UnfollowedNickname}] does not exist.");
        }

        var follow = await _session.QueryOver<Follow>()
            .Where(f => f.FollowerID == request.FollowerID && f.FollowedID == unfollowedUserID)
            .SingleOrDefaultAsync(cancellationToken);
        if (follow is null)
        {
            return Result.Failure($"User {request.FollowerID} is not following user [{request.UnfollowedNickname}].");
        }

        try
        {
            await _session.DeleteAsync(follow, cancellationToken);
            await _session.FlushAsync(cancellationToken);
        }
        catch (ADOException ex)
        {
            _logger.Error("Error while unfollowing [{UnfollowedNickname}({UnfollowedUserID})] by user {UserID}. Error message: {ErrorMessage}",
                request.UnfollowedNickname,
                unfollowedUserID,
                request.FollowerID,
                ex.Message);

            return Result.Failure(ex.Message);
        }

        _logger.Information("User {UserID} stopped following user [{UnfollowedNickname}({UnfollowedUserID})]",
            request.FollowerID,
            request.UnfollowedNickname,
            unfollowedUserID);

        return Result.Success();
    }
}
