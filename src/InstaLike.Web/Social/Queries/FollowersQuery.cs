using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Social;
using InstaLike.Web.Data;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Social.Queries;

public class FollowersQuery : IRequest<FollowModel[]>
{
    public string Nickname { get; }

    public FollowersQuery(string nickname)
    {
        Nickname = nickname;
    }
}

internal sealed class FollowersQueryHandler : IRequestHandler<FollowersQuery, FollowModel[]>
{
    private readonly IStatelessSession _session;
    private readonly ILogger _logger;

    public FollowersQueryHandler(IStatelessSession session, ILogger logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<FollowersQuery>() ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FollowModel[]> Handle(FollowersQuery request, CancellationToken cancellationToken)
    {
        FollowModel[] result;

        _logger.Debug("Reading followers list for user {Nickname} with parameters {@Request}", request.Nickname, request);

        using (var tx = _session.BeginTransaction())
        {
            FollowModel model = null!;
            Follow follow = null!;
            User followed = null!;
            User follower = null!;

            var followersQuery = _session.QueryOver(() => follow)
                .JoinEntityAlias(() => follower, () => follower.ID == follow.FollowerID)
                .JoinEntityAlias(() => followed, () => followed.ID == follow.FollowedID)
                .Where(Restrictions.Eq(Projections.Property(() => followed.Nickname), request.Nickname))
                .SelectList(fields => fields
                    .Select(f => follower.Nickname).WithAlias(() => model.Nickname)
                    .Select(f => follower.ProfilePicture).WithAlias(() => model.ProfilePicture)
                )
                .TransformUsing(new EntityToModelResultTransformer<FollowModel>());

            result = (await followersQuery.ListAsync<FollowModel>(cancellationToken)).ToArray();
        }

        return result;
    }
}
