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
        private readonly ISession _session;
        private readonly ILogger _logger;

        public FollowersQueryHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<FollowersQuery>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FollowModel[]> Handle(FollowersQuery request, CancellationToken cancellationToken)
        {
            FollowModel[] result = null;

            _logger.Debug("Reading followers list for user {Nickname} with parameters {@Request}", request.Nickname, request);

            using (var tx = _session.BeginTransaction())
            {
                FollowModel model = null;
                User followed = null;
                User follower = null;

                var followersQuery = _session.QueryOver<Follow>()
                    .Inner.JoinAlias(f => f.Follower, () => follower)
                    .Inner.JoinAlias(f => f.Followed, () => followed)
                    .Where(Restrictions.Eq(Projections.Property(() => followed.Nickname), request.Nickname))
                    .SelectList(fields => fields
                        .Select(f => follower.Nickname).WithAlias(() => model.Nickname)
                        .Select(f => follower.ProfilePicture.RawBytes).WithAlias(() => model.ProfilePicture)
                    )
                    .TransformUsing(Transformers.AliasToBean<FollowModel>());

                result = (await followersQuery.ListAsync<FollowModel>()).ToArray();
            }

            return result;
        }
    }
}