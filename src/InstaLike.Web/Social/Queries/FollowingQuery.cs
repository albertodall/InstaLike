using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Membership;
using InstaLike.Core.Social;
using InstaLike.Web.Data;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Serilog;

namespace InstaLike.Web.Social.Queries;

public sealed class FollowingQuery : IRequest<FollowModel[]>
{
    public string Nickname { get; }

    public FollowingQuery(string nickname)
    {
        Nickname = nickname;
    }
}

internal sealed class FollowingQueryHandler : IRequestHandler<FollowingQuery, FollowModel[]>
{
    private readonly IStatelessSession _session;
    private readonly ILogger _logger;

    public FollowingQueryHandler(IStatelessSession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<FollowingQuery>();
    }

    public async Task<FollowModel[]> Handle(FollowingQuery request, CancellationToken cancellationToken)
    {
        FollowModel[] result;

        _logger.Debug("Reading following list for user {Nickname} with parameters {@Request}", request.Nickname, request);

        using (var tx = _session.BeginTransaction())
        {
            FollowModel model = null!;
            Follow follow = null!;
            Member followed = null!;
            Member follower = null!;

            var followersQuery = _session.QueryOver(() => follow)
                .JoinEntityAlias(() => follower, Restrictions.EqProperty(Projections.Property(() => follower.ID), Projections.Property(() => follow.Follower.ID)), JoinType.InnerJoin)
                .JoinEntityAlias(() => followed, Restrictions.EqProperty(Projections.Property(() => followed.ID), Projections.Property(() => follow.FollowedID)), JoinType.InnerJoin)
                .Where(Restrictions.Eq(Projections.Property(() => follower.Nickname), request.Nickname))
                .SelectList(fields => fields
                    .Select(f => followed.Nickname).WithAlias(() => model.Nickname)
                    .Select(f => followed.ProfilePicture).WithAlias(() => model.ProfilePicture)
                )
                .TransformUsing(new EntityToModelResultTransformer<FollowModel>());

            result = (await followersQuery.ListAsync<FollowModel>(cancellationToken)).ToArray();
        }

        return result;
    }
}
