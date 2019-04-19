using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

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
        private readonly ILogger _logger;

        public TimelineQueryHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<TimelineQuery>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PostModel[]> Handle(TimelineQuery request, CancellationToken cancellationToken)
        {
            _logger.Debug("Fetching timeline for user {UserID} with parameters {@Request}.", request.UserID, request);

            PostModel[] timeline = null;

            using (var tx = _session.BeginTransaction())
            {           
                Post post = null;
                Comment comment = null;
                User postAuthor = null;
                Follow follow = null;

                // Post to show in the timeline (user's own posts and posts published by followed users)
                var postsToShowInTimelineQuery = QueryOver.Of<Post>()
                    .Inner.JoinAlias(p => p.Author, () => postAuthor)
                        .Left.JoinAlias(() => postAuthor.Followers, () => follow)
                    .Where(
                        Restrictions.Disjunction()
                            .Add(() => follow.Follower.ID == request.UserID)
                            .Add(() => postAuthor.ID == request.UserID)
                    )
                    .Select(p => p.ID)
                    .OrderBy(p => p.PostDate).Desc
                    .Take(request.NumberOfPosts);

                // Comments for all posts included in the timeline.
                _session.QueryOver<Post>()
                    .Left.JoinAlias(p => p.Comments, () => comment)
                    .Fetch(SelectMode.Fetch, () => comment.Author)
                    .WithSubquery.WhereProperty(p => p.ID).In(postsToShowInTimelineQuery)
                    .Future<Post>();

                // Number of likes for each post that has likes.
                var postLikesCountQuery = _session.QueryOver<Like>()
                    .WithSubquery.WhereProperty(l => l.Post.ID).In(postsToShowInTimelineQuery)
                    .SelectList(fields => fields
                        .SelectGroup(l => l.Post.ID)
                        .SelectCount(l => l.ID)
                    )
                    .Future<object[]>();

                // Posts liked by current user
                var postsLikedByCurrentUserQuery = _session.QueryOver<Like>()
                    .WithSubquery
                        .WhereProperty(l => l.Post.ID).In(postsToShowInTimelineQuery)
                        .And(l => l.User.ID == request.UserID)
                    .Select(l => l.Post.ID)
                    .Future<int>();

                var timelineQuery = _session.QueryOver(() => post)
                    .Fetch(SelectMode.FetchLazyProperties, () => post.Author)
                    .WithSubquery.WhereProperty(p => p.ID).In(postsToShowInTimelineQuery)
                    .Future<Post>();

                var timelineQueryResult = await timelineQuery.GetEnumerableAsync();
                var postLikesCount = postLikesCountQuery.ToDictionary(o => (int)o[0], o => (int)o[1]);

                // Mapping
                var timelineList = new List<PostModel>();
                foreach (var p in timelineQueryResult)
                {
                    var postModel = new PostModel()
                    {
                        PostID = p.ID,
                        AuthorNickName = p.Author.Nickname,
                        AuthorProfilePicture = p.Author.ProfilePicture,
                        Picture = p.Picture,
                        PostDate = p.PostDate,
                        Text = p.Text,
                        Comments = p.Comments.Select(c => new CommentModel()
                        {
                            PostID = p.ID,
                            AuthorNickName = c.Author.Nickname,
                            CommentDate = c.CommentDate,
                            Text = c.Text
                        }).ToArray(),
                        LikesCount = postLikesCount.ContainsKey(p.ID) ? postLikesCount[p.ID] : 0,
                        IsLikedByCurrentUser = postsLikedByCurrentUserQuery.Any(id => id == p.ID)
                    };
                    timelineList.Add(postModel);
                }
                timeline = timelineList.OrderByDescending(p => p.PostDate).ToArray();
            }

            return timeline;
        }
    }
}