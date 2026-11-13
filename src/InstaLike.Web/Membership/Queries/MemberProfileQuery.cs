using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Posts;
using InstaLike.Core.Social;
using InstaLike.Web.Data;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Membership.Queries;

public sealed class MemberProfileQuery : IRequest<MemberProfileModel>
{
    public Guid CurrentMemberID { get; }
    public string Nickname { get; }
    public int NumberOfThumbnails { get; }

    public MemberProfileQuery(Guid currentMemberID, string nickname, int numberOfThumbnails)
    {
        CurrentMemberID = currentMemberID;
        Nickname = nickname;
        NumberOfThumbnails = numberOfThumbnails;
    }
}

internal sealed class MemberProfileQueryHandler : IRequestHandler<MemberProfileQuery, MemberProfileModel>
{
    private readonly IStatelessSession _session;
    private readonly ILogger _logger;

    public MemberProfileQueryHandler(IStatelessSession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<MemberProfileModel>();
    }

    public async Task<MemberProfileModel> Handle(MemberProfileQuery request, CancellationToken cancellationToken)
    {
        MemberProfileModel profile = null!;

        _logger.Debug("Reading profile of member {MemberID} with parameters {@Request}.", request.CurrentMemberID, request);

        using (var tx = _session.BeginTransaction())
        {
            var memberProfileQuery = _session.QueryOver<Member>()
                .Where(Restrictions.Eq("Nickname", request.Nickname))
                .SelectList(list => list
                    .Select(m => m.ID).WithAlias(() => profile.MemberID)
                    .Select(m => m.Nickname).WithAlias(() => profile.Nickname)
                    .Select(m => m.FullName.Name).WithAlias(() => profile.Name)
                    .Select(m => m.FullName.Surname).WithAlias(() => profile.Surname)
                    .Select(m => m.Biography).WithAlias(() => profile.Bio)
                    .Select(m => m.ProfilePicture).WithAlias(() => profile.ProfilePicture)
                )
                .TransformUsing(new EntityToModelResultTransformer<MemberProfileModel>());

            // Load the member's information
            profile = await memberProfileQuery.SingleOrDefaultAsync<MemberProfileModel>(cancellationToken);

            // Number of posts published by this author. 
            var postCountQuery = _session.QueryOver<Post>()
                .Where(p => p.AuthorID == profile.MemberID.Value)
                .Select(Projections.Count<Post>(p => p.ID))
                .FutureValue<int>();

            // Thumbnails of the latest published posts.
            PostThumbnailModel thumbnailModel = null!;
            var thumbnailsQuery = _session.QueryOver<Post>()
                .Where(p => p.AuthorID == profile.MemberID.Value)
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
                .Where(f => f.FollowedID == profile.MemberID.Value)
                .Select(Projections.Count<Follow>(f => f.ID))
                .FutureValue<int>();

            // Number of followed members
            var followingCountQuery = _session.QueryOver<Follow>()
                .Where(f => f.FollowerID == profile.MemberID.Value)
                .Select(Projections.Count<Follow>(f => f.ID))
                .FutureValue<int>();

            // Is the current member following the selected member?
            var isFollowedByCurrentMemberQuery = _session.QueryOver<Follow>()
                .Where(f => f.FollowedID == profile.MemberID.Value)
                .And(f => f.FollowerID == request.CurrentMemberID)
                .Select(Projections.Count<Follow>(f => f.ID))
                .FutureValue<int>();

            profile.NumberOfPosts = await postCountQuery.GetValueAsync(cancellationToken);
            profile.RecentPosts = (await thumbnailsQuery.GetEnumerableAsync(cancellationToken)).ToArray();
            profile.NumberOfFollowers = await followersCountQuery.GetValueAsync(cancellationToken);
            profile.NumberOfFollows = await followingCountQuery.GetValueAsync(cancellationToken);
            profile.IsCurrentMemberProfile = profile.MemberID.Value == request.CurrentMemberID;

            profile.Following =
                await isFollowedByCurrentMemberQuery.GetValueAsync(cancellationToken) == 1
                || profile.IsCurrentMemberProfile;

            await tx.CommitAsync(cancellationToken);
        }

        return profile;
    }
}
