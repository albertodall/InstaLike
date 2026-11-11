using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Posts;
using InstaLike.Core.Social;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Serilog;

namespace InstaLike.Web.Posts.Queries;

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

        var currentUser = await _session.LoadAsync<User>(request.UserID, cancellationToken);
        PostModel[] timeline;

        using (var tx = _session.BeginTransaction())
        {
            Post post = null!;

            var followedUserIdsQuery = QueryOver.Of<Follow>()
                .Where(f => f.FollowerID == request.UserID)
                .Select(f => f.FollowedID);

            // Post to show in the timeline (user's own posts and posts published by followed users)
            var postIdsToShowInTimelineQuery = QueryOver.Of<Post>()
                .Where(
                    Restrictions.Disjunction()
                        .Add(Restrictions.Where<Post>(p => p.Author.ID == request.UserID))
                        .Add(Subqueries.WhereProperty<Post>(p => p.Author.ID).In(followedUserIdsQuery))
                )
                .Select(p => p.ID)
                .OrderBy(p => p.PostDate).Desc
                .Take(request.NumberOfPosts);

            // Comments for all posts included in the timeline.
            Comment commentEntity = null!;
            User commentAuthor = null!;
            CommentModel comment = null!;
            var commentsQuery = _session.QueryOver(() => commentEntity)
                .JoinEntityAlias(() => commentAuthor, () => commentAuthor.ID == commentEntity.AuthorID)
                .WithSubquery
                    .WhereProperty(() => commentEntity.PostID).In(postIdsToShowInTimelineQuery)
                .SelectList(list => list
                    .Select(() => commentEntity.PostID).WithAlias(() => comment.PostID)
                    .Select(() => commentAuthor.Nickname).WithAlias(() => comment.AuthorNickName)
                    .Select(() => commentEntity.Text).WithAlias(() => comment.Text)
                    .Select(() => commentEntity.CommentDate).WithAlias(() => comment.CommentDate)
                )
                .TransformUsing(Transformers.AliasToBean<CommentModel>())
                .Future<CommentModel>();

            // Number of likes for each post that has likes.
            var postLikesCountQuery = _session.QueryOver<Like>()
                .WithSubquery
                    .WhereProperty(l => l.PostID).In(postIdsToShowInTimelineQuery)
                .SelectList(fields => fields
                    .SelectGroup(l => l.PostID)
                    .SelectCount(l => l.ID)
                )
                .Future<object[]>();

            // Posts liked by current user
            var postsLikedByCurrentUserQuery = _session.QueryOver<Like>()
                .WithSubquery
                    .WhereProperty(l => l.PostID).In(postIdsToShowInTimelineQuery)
                    .And(l => l.UserID == request.UserID)
                .Select(l => l.PostID)
                .Future<int>();

            var timelineQuery = _session.QueryOver(() => post)
                .Fetch(SelectMode.FetchLazyProperties, () => post.Author)
                .WithSubquery
                    .WhereProperty(p => p.ID).In(postIdsToShowInTimelineQuery)
                .Future<Post>();

            var timelineQueryResult = await timelineQuery.GetEnumerableAsync(cancellationToken);
            var postLikesCount = postLikesCountQuery.ToDictionary(o => (int)o[0], o => (int)o[1]);
            var commentsByPost = commentsQuery.ToLookup(c => c.PostID);

            // Mapping
            var timelineList = timelineQueryResult.Select(p => new PostModel
            {
                PostID = p.ID,
                AuthorNickName = p.Author.Nickname,
                AuthorProfilePicture = p.Author.ProfilePicture,
                Picture = p.Picture,
                PostDate = p.PostDate,
                Text = p.Text,
                Comments = commentsByPost[p.ID]
                        .OrderByDescending(c => c.CommentDate)
                        .ToArray(),
                LikesCount = postLikesCount.ContainsKey(p.ID) ? postLikesCount[p.ID] : 0,
                IsLikedByCurrentUser = postsLikedByCurrentUserQuery.Any(id => id == p.ID),
                CanBeEditedByCurrentUser = p.CanBeEditedBy(currentUser)
            });

            timeline = timelineList.OrderByDescending(p => p.PostDate).ToArray();

            await tx.CommitAsync(cancellationToken);
        }

        return timeline;
    }
}
