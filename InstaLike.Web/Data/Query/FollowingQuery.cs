using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

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

        public FollowingQueryHandler(ISession session)
        {
            _session = session;
        }

        public async Task<FollowModel[]> Handle(FollowingQuery query, CancellationToken cancellationToken)
        {
            FollowModel[] result = null;

            using (var tx = _session.BeginTransaction())
            {
                var user = await _session.QueryOver<User>()
                    .Where(Restrictions.Eq("Nickname", query.Nickname))
                    .SingleOrDefaultAsync();

                FollowModel model = null;
                User following = null;
                var followersQuery = _session.QueryOver<Follow>()
                    .Inner.JoinAlias(f => f.Following, () => following)
                    .Where(f => f.Follower.ID == user.ID)
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