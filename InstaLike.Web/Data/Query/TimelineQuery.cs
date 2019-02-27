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

namespace InstaLike.Web.Data.Query
{
    public class TimelineQuery : IRequest<PostModel[]>
    {
        public int UserID { get; }
        public int NumberOfPosts { get; }

        public TimelineQuery(int userID, int numberOfPosts)
        {
            UserID = userID;
            NumberOfPosts = numberOfPosts;
        }
    }

    public sealed class TimelineQueryHandler : IRequestHandler<TimelineQuery, PostModel[]>
    {
        private readonly ISession _session;

        public TimelineQueryHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<PostModel[]> Handle(TimelineQuery query, CancellationToken cancellationToken)
        {
            PostModel[] timeline = null;

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
                    .TransformUsing(Transformers.DistinctRootEntity)
                    .Take(query.NumberOfPosts);

                var postsInTimeline = await timelineQuery.ListAsync();

                timeline = postsInTimeline.Select(p => new PostModel()
                {
                    AuthorNickName = p.Author.Nickname,
                    AuthorProfilePicture = p.Author.ProfilePicture,
                    Picture = p.Picture,
                    PostDate = p.PostDate,
                    PostID = p.ID,
                    Text = p.Text,
                    Comments = p.Comments.Select(c => new CommentModel()
                    {
                        AuthorNickName = c.Author.Nickname,
                        CommentDate = c.CommentDate,
                        Text = c.Text
                    }).ToArray(),
                    UserLikes = p.Likes.Select(l => (string)l.User.Nickname).ToArray()
                }).ToArray();
            }

            return timeline;
        }
    }
}
