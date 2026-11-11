using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Posts;
using InstaLike.Core.Social;
using InstaLike.Web.Data;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Accounts.Queries;

public class UserProfileQuery : IRequest<UserProfileModel>
{
    public int CurrentUserID { get; }
    public string Nickname { get; }
    public int NumberOfThumbnails { get; }

    public UserProfileQuery(int currentUserID, string nickname, int numberOfThumbnails)
    {
        CurrentUserID = currentUserID;
        Nickname = nickname;
        NumberOfThumbnails = numberOfThumbnails;
    }
}

internal sealed class UserProfileQueryHandler : IRequestHandler<UserProfileQuery, UserProfileModel>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public UserProfileQueryHandler(ISession session, ILogger logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<UserProfileModel>() ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserProfileModel> Handle(UserProfileQuery request, CancellationToken cancellationToken)
    {
        UserProfileModel profile = null!;

        _logger.Debug("Reading profile of user {UserID} with parameters {@Request}.", request.CurrentUserID, request);

        using (var tx = _session.BeginTransaction())
        {
            var userProfileQuery = _session.QueryOver<User>()
                .Where(Restrictions.Eq("Nickname", request.Nickname))
                .SelectList(list => list
                    .Select(u => u.ID).WithAlias(() => profile.UserID)
                    .Select(u => u.Nickname).WithAlias(() => profile.Nickname)
                    .Select(u => u.FullName.Name).WithAlias(() => profile.Name)
                    .Select(u => u.FullName.Surname).WithAlias(() => profile.Surname)
                    .Select(u => u.Biography).WithAlias(() => profile.Bio)
                    .Select(u => u.ProfilePicture).WithAlias(() => profile.ProfilePicture)
                )
                .TransformUsing(new EntityToModelResultTransformer<UserProfileModel>());

            // Load user's information
            profile = await userProfileQuery.SingleOrDefaultAsync<UserProfileModel>(cancellationToken);

            // Number of posts published by this author. 
            var postCountQuery = _session.QueryOver<Post>()
                .Where(p => p.AuthorID == profile.UserID)
                .Select(Projections.Count<Post>(p => p.ID))
                .FutureValue<int>();

            // Thumbnails of the latest published posts.
            PostThumbnailModel thumbnailModel = null!;
            var thumbnailsQuery = _session.QueryOver<Post>()
                .Where(p => p.AuthorID == profile.UserID)
                .OrderBy(p => p.PostDate).Desc()
                .Select(Projections.ProjectionList()
                    .Add(Projections.Property<Post>(p => p.ID)
                        .WithAlias(() => thumbnailModel.PostID))
                    .Add(Projections.Property<Post>(p => p.Picture)
                        .WithAlias(() => thumbnailModel.Picture))
                )
                .TransformUsing(new EntityToModelResultTransformer<PostThumbnailModel>())
                .Take(request.NumberOfThumbnails)
                .Future<PostThumbnailModel>();

            // Number of followers
            var followersCountQuery = _session.QueryOver<Follow>()
                .Where(f => f.FollowedID == profile.UserID)
                .Select(Projections.Count<Follow>(f => f.ID))
                .FutureValue<int>();

            // Number of followed users
            var followingCountQuery = _session.QueryOver<Follow>()
                .Where(f => f.FollowerID == profile.UserID)
                .Select(Projections.Count<Follow>(f => f.ID))
                .FutureValue<int>();

            // Is the current user following the selected user?
            var isFollowedByCurrentUserQuery = _session.QueryOver<Follow>()
                .Where(f => f.FollowedID == profile.UserID)
                .And(f => f.FollowerID == request.CurrentUserID)
                .Select(Projections.Count<Follow>(f => f.ID))
                .FutureValue<int>();

            profile.NumberOfPosts = await postCountQuery.GetValueAsync(cancellationToken);
            profile.RecentPosts = (await thumbnailsQuery.GetEnumerableAsync(cancellationToken)).ToArray();
            profile.NumberOfFollowers = await followersCountQuery.GetValueAsync(cancellationToken);
            profile.NumberOfFollows = await followingCountQuery.GetValueAsync(cancellationToken);
            profile.IsCurrentUserProfile = profile.UserID == request.CurrentUserID;

            profile.Following =
                await isFollowedByCurrentUserQuery.GetValueAsync(cancellationToken) == 1
                || profile.IsCurrentUserProfile;

            await tx.CommitAsync(cancellationToken);
        }

        return profile;
    }
}
