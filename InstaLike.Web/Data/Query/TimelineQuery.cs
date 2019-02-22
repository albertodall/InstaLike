using System;
using System.Linq;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using NHibernate;
using NHibernate.Criterion;

namespace InstaLike.Web.Data.Query
{
    public class TimelineQuery : IQuery<PostModel[]>
    {
        public int UserID { get; }

        public TimelineQuery(int userID)
        {
            UserID = userID;
        }
    }

    public sealed class TimelineQueryHandler : IQueryHandler<TimelineQuery, PostModel[]>
    {
        private readonly ISession _session;

        public TimelineQueryHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<PostModel[]> HandleAsync(TimelineQuery query)
        {
            using (var tx = _session.BeginTransaction())
            {
                Post postAlias = null;
                User authorAlias = null;
                Follow followingAlias = null;

                var timelineQuery = _session.QueryOver(() => postAlias)
                    .Fetch(SelectMode.ChildFetch, () => postAlias.Comments)
                    .Fetch(SelectMode.ChildFetch, () => postAlias.Likes)
                    .Inner.JoinQueryOver(p => p.Author, () => authorAlias)
                        .Inner.JoinAlias(() => authorAlias.Following, () => followingAlias)
                            .Where(() => followingAlias.Follower.ID == query.UserID)
                    .OrderBy(() => postAlias.PostDate).Desc()
                    .Take(10);

                var timeline = await timelineQuery.ListAsync();
            }

            return new PostModel[] { };
        }
    }
}
