using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Serilog;

namespace InstaLike.Web.Data.Query
{
    public class FollowingQuery : IRequest<FollowModel[]>
    {
        public string Nickname { get; }

        public FollowingQuery(string nickname)
        {
            Nickname = nickname;
        }
    }

    internal sealed class FollowingQueryHandler : IRequestHandler<FollowingQuery, FollowModel[]>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public FollowingQueryHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<FollowingQuery>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FollowModel[]> Handle(FollowingQuery request, CancellationToken cancellationToken)
        {
            FollowModel[] result = null;

            _logger.Debug("Reading following list for user {Nickname} with parameters {@Request}", request.Nickname, request);

            using (var tx = _session.BeginTransaction())
            {
                FollowModel model = null;
                User following = null;
                User follower = null;

                var followersQuery = _session.QueryOver<Follow>()
                    .Inner.JoinAlias(f => f.Following, () => following)
                    .Inner.JoinAlias(f => f.Follower, () => follower)
                    .Where(Restrictions.Eq("follower.Nickname", request.Nickname))
                        .And(f => f.Follower.ID == follower.ID)
                    .SelectList(fields => fields
                        .Select(f => following.Nickname).WithAlias(() => model.Nickname)
                        .Select(f => following.ProfilePicture.RawBytes).WithAlias(() => model.ProfilePicture)
                    )
                    .TransformUsing(Transformers.AliasToBean<FollowModel>());

                result = (await followersQuery.ListAsync<FollowModel>()).ToArray();
            }

            return result;
        }
    }
}