using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                PostModel post = null;
                CommentModel comment = null;
                
                Post postAlias = null;
                User postAuthor = null;
                User commentAuthor = null;
                Follow followingAlias = null;

                // Post to show in the timeline.
                var postsToShowInTimelineQuery = QueryOver.Of<Post>()
                    .Inner.JoinAlias(p => p.Author, () => postAuthor)
                        .Left.JoinAlias(() => postAuthor.Following, () => followingAlias)
                    .Where(() => followingAlias.Follower.ID == query.UserID)
                    .Select(p => p.ID)
                    .OrderBy(p => p.PostDate).Desc
                    .Take(query.NumberOfPosts);

                // Comments for all post included in the timeline.
                _session.QueryOver<Comment>()
                    .Inner.JoinAlias(c => c.Author, () => commentAuthor)
                    .WithSubquery.WhereProperty(c => c.Post.ID).In(postsToShowInTimelineQuery)
                    .SelectList(commentFields => commentFields
                        .Select(c => c.Post.ID).WithAlias(() => comment.PostID)
                        .Select(c => c.Text).WithAlias(() => comment.Text)
                        .Select(c => c.CommentDate).WithAlias(() => comment.CommentDate)
                        .Select(() => commentAuthor.Nickname).WithAlias(() => comment.AuthorNickName)
                    )
                    .TransformUsing(Transformers.AliasToBean<CommentModel>())
                    .Future<CommentModel>();

                var timelineQuery = _session.QueryOver(() => postAlias)
                    .WithSubquery.WhereProperty(p => p.ID).In(postsToShowInTimelineQuery)
                    .Inner.JoinAlias(p => p.Author, () => postAuthor)
                    .SelectList(list => list
                        .Select(p => p.ID).WithAlias(() => post.PostID)
                        .Select(p => p.PostDate).WithAlias(() => post.PostDate)
                        .Select(p => p.Picture.RawBytes).WithAlias(() => post.Picture)
                        .Select(p => p.Text).WithAlias(() => post.Text) 
                        .Select(() => postAuthor.Nickname).WithAlias(() => post.AuthorNickName)
                        .Select(() => postAuthor.ProfilePicture.RawBytes).WithAlias(() => post.AuthorProfilePicture)
                        // Count of post's likes
                        .SelectSubQuery(
                            QueryOver.Of<Like>().Where(l => l.Post.ID == postAlias.ID)
                            .ToRowCountQuery()).WithAlias(() => post.LikesCount)
                        // Checks if the post likes to the current user
                        .SelectSubQuery(
                            QueryOver.Of<Like>()
                                .Where(l => l.Post.ID == postAlias.ID)
                                .And(l => l.User.ID == query.UserID)
                            .ToRowCountQuery()).WithAlias(() => post.IsLikedByCurrentUser)
                    )
                    .TransformUsing(Transformers.AliasToBean<PostModel>())
                    .Future<PostModel>();

                timeline = (await timelineQuery.GetEnumerableAsync()).ToArray();
            }

            return timeline;
        }
    }
}