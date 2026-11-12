using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Core.Social;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Serilog;

namespace InstaLike.Web.Posts.Queries;

public sealed class TimelineQuery : IRequest<PostModel[]>
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
    private readonly IStatelessSession _session;
    private readonly ILogger _logger;

    public TimelineQueryHandler(IStatelessSession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<TimelineQuery>();
    }

    public async Task<PostModel[]> Handle(TimelineQuery request, CancellationToken cancellationToken)
    {
        _logger.Debug("Fetching timeline for user {UserID} with parameters {@Request}.", request.UserID, request);

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
                        .Add(Restrictions.Where<Post>(p => p.AuthorID == request.UserID))
                        .Add(Subqueries.WhereProperty<Post>(p => p.AuthorID).In(followedUserIdsQuery))
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
                    .WhereProperty(() => commentEntity.Post.ID).In(postIdsToShowInTimelineQuery)
                .SelectList(list => list
                    .Select(() => commentEntity.Post.ID).WithAlias(() => comment.PostID)
                    .Select(() => commentAuthor.Nickname).WithAlias(() => comment.AuthorNickName)
                    .Select(() => commentEntity.Text).WithAlias(() => comment.Text)
                    .Select(() => commentEntity.CommentDate).WithAlias(() => comment.CommentDate)
                )
                .TransformUsing(Transformers.AliasToBean<CommentModel>())
                .Future<CommentModel>();

            // Number of likes for each post that has likes.
            var postLikesCountQuery = _session.QueryOver<Like>()
                .WithSubquery
                    .WhereProperty(l => l.Post.ID).In(postIdsToShowInTimelineQuery)
                .SelectList(fields => fields
                    .SelectGroup(l => l.Post.ID)
                    .SelectCount(l => l.ID)
                )
                .Future<object[]>();

            // Posts liked by current user
            var postsLikedByCurrentUserQuery = _session.QueryOver<Like>()
                .WithSubquery
                    .WhereProperty(l => l.Post.ID).In(postIdsToShowInTimelineQuery)
                    .And(l => l.UserID == request.UserID)
                .Select(l => l.Post.ID)
                .Future<int>();

            var authorsQuery = _session.QueryOver<User>()
                .WithSubquery
                    .WhereProperty(u => u.ID).In(QueryOver.Of<Post>()
                        .WithSubquery.WhereProperty(p => p.ID).In(postIdsToShowInTimelineQuery)
                        .Select(p => p.AuthorID))
                .SelectList(fields => fields
                    .Select(u => u.ID)
                    .Select(u => u.Nickname)
                    .Select(u => u.ProfilePicture)
                )
                .Future<object[]>();

            var timelineQuery = _session.QueryOver(() => post)
                .WithSubquery
                    .WhereProperty(p => p.ID).In(postIdsToShowInTimelineQuery)
                .Future<Post>();

            var timelineQueryResult = await timelineQuery.GetEnumerableAsync(cancellationToken);
            var postLikesCount = postLikesCountQuery.ToDictionary(o => (int)o[0], o => (int)o[1]);
            var commentsByPost = commentsQuery.ToLookup(c => c.PostID);
            var authors = authorsQuery.ToDictionary(
                a => (int)a[0],
                a => (Nickname: (string)a[1], ProfilePicture: (PictureId?)a[2]));

            // Mapping
            var timelineList = timelineQueryResult.Select(p => new PostModel
            {
                PostID = p.ID,
                AuthorNickName = authors[p.AuthorID].Nickname,
                AuthorProfilePicture = authors[p.AuthorID].ProfilePicture,
                Picture = p.Picture,
                PostDate = p.PostDate,
                Text = p.Text,
                Comments = commentsByPost[p.ID]
                        .OrderByDescending(c => c.CommentDate)
                        .ToArray(),
                LikesCount = postLikesCount.ContainsKey(p.ID) ? postLikesCount[p.ID] : 0,
                IsLikedByCurrentUser = postsLikedByCurrentUserQuery.Any(id => id == p.ID),
                CanBeLikedByCurrentUser = p.CanBeLikedBy(request.UserID),
                CanBeEditedByCurrentUser = p.CanBeEditedBy(request.UserID)
            });

            timeline = timelineList.OrderByDescending(p => p.PostDate).ToArray();

            await tx.CommitAsync(cancellationToken);
        }

        return timeline;
    }
}
