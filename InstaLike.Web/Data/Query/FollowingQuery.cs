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
                var followers = await _session.QueryOver<Follow>()
                    .Where(f => f.Follower == user)
                    .SelectList(list => list
                        .Select(f => f.Following.Nickname)
                            .WithAlias(() => model.Nickname)
                        .Select(f => f.Following.ProfilePicture)
                            .WithAlias(() => model.ProfilePicture)
                    )
                    .TransformUsing(Transformers.AliasToBean<FollowModel>())
                    .ListAsync<FollowModel>();

                result = followers.ToArray();
            }

            return result;
        }
    }
}
