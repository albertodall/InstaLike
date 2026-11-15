using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Membership;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Core.Social;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Serilog;

namespace InstaLike.Web.Posts
{
    public sealed class TimelineModel
    {
        public PostModel[] Posts { get; set; } = [];
        public bool HasOlderPosts { get; set; }
    }

    public sealed class TimelineQuery : IRequest<TimelineModel>
    {
        public Guid UserID { get; }
        public int NumberOfPosts { get; }

        public TimelineQuery(Guid userID, int numberOfPosts)
        {
            UserID = userID;
            NumberOfPosts = numberOfPosts;
        }
    }

    public sealed class OlderTimelinePostsQuery : IRequest<TimelineModel>
    {
        public Guid UserID { get; }
        public PostId LastPostShownID { get; }
        public int NumberOfPosts { get; }

        public OlderTimelinePostsQuery(Guid userID, PostId lastPostShownID, int numberOfPosts)
        {
            UserID = userID;
            LastPostShownID = lastPostShownID;
            NumberOfPosts = numberOfPosts;
        }
    }

    public sealed class TimelineQueryHandler : IRequestHandler<TimelineQuery, TimelineModel>, IRequestHandler<OlderTimelinePostsQuery, TimelineModel>
    {
        private readonly IStatelessSession _session;
        private readonly ILogger _logger;

        public TimelineQueryHandler(IStatelessSession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<TimelineQuery>();
        }

        public Task<TimelineModel> Handle(TimelineQuery request, CancellationToken cancellationToken)
        {
            _logger.Debug("Fetching timeline for user {UserID} with parameters {@Request}.", request.UserID, request);

            return ReadTimeline(request.UserID, null, request.NumberOfPosts, cancellationToken);
        }

        public Task<TimelineModel> Handle(OlderTimelinePostsQuery request, CancellationToken cancellationToken)
        {
            _logger.Debug("Fetching older timeline posts for user {UserID} with parameters {@Request}.", request.UserID, request);

            return ReadTimeline(request.UserID, request.LastPostShownID, request.NumberOfPosts, cancellationToken);
        }

        private async Task<TimelineModel> ReadTimeline(Guid userID, PostId? lastPostShownID, int numberOfPosts, CancellationToken cancellationToken)
        {
            TimelineModel timeline;

            using (var tx = _session.BeginTransaction())
            {
                Post post = null!;

                var followedUserIdsQuery = QueryOver.Of<Follow>()
                    .Where(f => f.Follower.ID == FollowerId.From(userID))
                    .Select(f => f.FollowedID);

                // Post to show in the timeline (user's own posts and posts published by followed users)
                var postsInTimelineQuery = QueryOver.Of<Post>()
                    .Where(
                        Restrictions.Disjunction()
                            .Add(Restrictions.Where<Post>(p => p.AuthorID == userID))
                            .Add(Subqueries.WhereProperty<Post>(p => p.AuthorID).In(followedUserIdsQuery))
                    );

                if (lastPostShownID is not null)
                {
                    postsInTimelineQuery = postsInTimelineQuery.Where(PostedBefore(lastPostShownID));
                }

                // One post more than the ones to show tells whether there are older posts.
                var postIdsToShowInTimelineQuery = postsInTimelineQuery
                    .Select(p => p.ID)
                    .OrderBy(p => p.PostDate).Desc
                    .ThenBy(p => p.ID).Desc
                    .Take(numberOfPosts + 1);

                // Comments for all posts included in the timeline.
                Comment commentEntity = null!;
                Member commentAuthor = null!;
                CommentModel comment = null!;
                var commentsQuery = _session.QueryOver(() => commentEntity)
                    .JoinEntityAlias(() => commentAuthor, Restrictions.EqProperty(Projections.Property(() => commentAuthor.ID), Projections.Property(() => commentEntity.AuthorID)), JoinType.InnerJoin)
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
                PostLikesCount likesCount = null!;
                var postLikesCountQuery = _session.QueryOver<Like>()
                    .WithSubquery
                        .WhereProperty(l => l.Post.ID).In(postIdsToShowInTimelineQuery)
                    .SelectList(fields => fields
                        .SelectGroup(l => l.Post.ID).WithAlias(() => likesCount.PostID)
                        .SelectCount(l => l.ID).WithAlias(() => likesCount.Count)
                    )
                    .TransformUsing(Transformers.AliasToBean<PostLikesCount>())
                    .Future<PostLikesCount>();

                // Posts liked by current user
                var postsLikedByCurrentUserQuery = _session.QueryOver<Like>()
                    .WithSubquery
                        .WhereProperty(l => l.Post.ID).In(postIdsToShowInTimelineQuery)
                        .And(l => l.MemberID == userID)
                    .Select(l => l.Post.ID)
                    .Future<PostId>();

                PostAuthor author = null!;
                var authorsQuery = _session.QueryOver<Member>()
                    .WithSubquery
                        .WhereProperty(u => u.ID).In(QueryOver.Of<Post>()
                            .WithSubquery.WhereProperty(p => p.ID).In(postIdsToShowInTimelineQuery)
                            .Select(p => p.AuthorID))
                    .SelectList(fields => fields
                        .Select(u => u.ID).WithAlias(() => author.ID)
                        .Select(u => u.Nickname).WithAlias(() => author.Nickname)
                        .Select(u => u.ProfilePicture).WithAlias(() => author.ProfilePicture)
                    )
                    .TransformUsing(Transformers.AliasToBean<PostAuthor>())
                    .Future<PostAuthor>();

                var timelineQuery = _session.QueryOver(() => post)
                    .WithSubquery
                        .WhereProperty(p => p.ID).In(postIdsToShowInTimelineQuery)
                    .OrderBy(p => p.PostDate).Desc
                    .ThenBy(p => p.ID).Desc
                    .Future<Post>();

                var posts = (await timelineQuery.GetEnumerableAsync(cancellationToken)).ToArray();

                // Each query above reads the rows of all the posts at once: index them by post to map each post.
                var authorsByID = authorsQuery.ToDictionary(a => a.ID.Value);
                var likesCountByPostID = postLikesCountQuery.ToDictionary(l => l.PostID, l => l.Count);
                var postIDsLikedByCurrentUser = postsLikedByCurrentUserQuery.ToHashSet();
                var commentsByPostID = commentsQuery.ToLookup(c => c.PostID);

                timeline = new TimelineModel
                {
                    Posts = [.. posts.Take(numberOfPosts).Select(p => new PostModel
                    {
                        PostID = p.ID,
                        AuthorNickName = authorsByID[p.AuthorID].Nickname,
                        AuthorProfilePicture = authorsByID[p.AuthorID].ProfilePicture,
                        Picture = p.Picture,
                        PostDate = p.PostDate,
                        Text = p.Text,
                        Comments = [.. commentsByPostID[p.ID].OrderByDescending(c => c.CommentDate)],
                        LikesCount = likesCountByPostID.GetValueOrDefault(p.ID),
                        IsLikedByCurrentUser = postIDsLikedByCurrentUser.Contains(p.ID),
                        CanBeLikedByCurrentUser = p.CanBeLikedBy(userID),
                        CanBeEditedByCurrentUser = p.CanBeEditedBy(userID)
                    })],
                    HasOlderPosts = posts.Length > numberOfPosts
                };

                await tx.CommitAsync(cancellationToken);
            }

            return timeline;
        }

        // Posts are sorted by date, then by ID when two posts share the same date.
        private static Junction PostedBefore(PostId lastPostShownID)
        {
            var lastPostShownDate = QueryOver.Of<Post>()
                .Where(p => p.ID == lastPostShownID)
                .Select(p => p.PostDate);

            return Restrictions.Disjunction()
                .Add(Subqueries.WhereProperty<Post>(p => p.PostDate).Lt(lastPostShownDate))
                .Add(Restrictions.Conjunction()
                    .Add(Subqueries.WhereProperty<Post>(p => p.PostDate).Eq(lastPostShownDate))
                    .Add(Restrictions.Lt(Projections.Property<Post>(p => p.ID), lastPostShownID)));
        }

        private sealed class PostAuthor
        {
            public MemberId ID { get; set; } = null!;
            public string Nickname { get; set; } = string.Empty;
            public PictureId? ProfilePicture { get; set; }
        }

        private sealed class PostLikesCount
        {
            public PostId PostID { get; set; } = null!;
            public int Count { get; set; }
        }
    }
}
