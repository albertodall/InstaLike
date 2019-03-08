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

        public FollowersQueryHandler(ISession session)
        {
            _session = session;
        }

        public async Task<FollowModel[]> Handle(FollowersQuery request, CancellationToken cancellationToken)
        {
            FollowModel[] result = null;

            using (var tx = _session.BeginTransaction())
            {

                FollowModel model = null;
                User following = null;
                User follower = null;

                var followersQuery = _session.QueryOver<Follow>()
                    .Inner.JoinAlias(f => f.Follower, () => follower)
                    .Inner.JoinAlias(f => f.Following, () => following)
                    .Where(Restrictions.Eq("following.Nickname", request.Nickname))
                        .And(f => f.Following.ID == following.ID)
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