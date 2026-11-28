using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Core.Social;
using InstaLike.Web.Data;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Membership
{
    public record MemberProfileModel
    {
        public required MemberId MemberID { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public int NumberOfPosts { get; set; }
        public int NumberOfFollowers { get; set; }
        public int NumberOfFollows { get; set; }
        public bool IsCurrentMemberProfile { get; set; }
        public bool Following { get; set; }
        public PictureId? ProfilePicture { get; set; }
        public PostThumbnailModel[] RecentPosts { get; set; } = Array.Empty<PostThumbnailModel>();
    }

    public sealed class PostThumbnailModel
    {
        public required PostId PostID { get; set; }
        public PictureId? Picture { get; set; }
    }

    public sealed class MemberProfileQuery : IRequest<Result<MemberProfileModel>>
    {
        public MemberId CurrentMemberID { get; }
        public string Nickname { get; }
        public int NumberOfThumbnails { get; }

        public MemberProfileQuery(MemberId currentMemberID, string nickname, int numberOfThumbnails)
        {
            CurrentMemberID = currentMemberID;
            Nickname = nickname;
            NumberOfThumbnails = numberOfThumbnails;
        }
    }

    internal sealed class MemberProfileQueryHandler : IRequestHandler<MemberProfileQuery, Result<MemberProfileModel>>
    {
        private readonly IStatelessSession _session;
        private readonly ILogger _logger;

        public MemberProfileQueryHandler(IStatelessSession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<MemberProfileModel>();
        }

        public async Task<Result<MemberProfileModel>> Handle(MemberProfileQuery request, CancellationToken cancellationToken)
        {
            MemberProfileModel profile;

            _logger.Debug("Reading profile of member {MemberID} with parameters {@Request}.", request.CurrentMemberID, request);

            using (var tx = _session.BeginTransaction())
            {
                var memberProfileQuery = _session.QueryOver<Member>()
                    .Where(Restrictions.Eq("Nickname", request.Nickname))
                    .SelectInto(m => new MemberProfileModel
                    {
                        MemberID = m.ID,
                        Nickname = m.Nickname,
                        Name = m.FullName.Name,
                        Surname = m.FullName.Surname,
                        Bio = m.Biography,
                        ProfilePicture = m.ProfilePicture
                    });

                // Load the member's information
                profile = await memberProfileQuery.SingleOrDefaultAsync<MemberProfileModel>(cancellationToken);
                if (profile is null)
                {
                    return Result.Failure<MemberProfileModel>(MemberErrors.NotFound(request.Nickname));
                }

                // Number of posts published by this author. 
                var postCountQuery = _session.QueryOver<Post>()
                    .Where(p => p.AuthorID == profile.MemberID)
                    .ToRowCountQuery()
                    .FutureValue<int>();

                // Thumbnails of the latest published posts.
                var thumbnailsQuery = _session.QueryOver<Post>()
                    .Where(p => p.AuthorID == profile.MemberID)
                    .OrderBy(p => p.PostDate).Desc()
                    .SelectInto(p => new PostThumbnailModel
                    {
                        PostID = p.ID,
                        Picture = p.Picture
                    })
                    .Take(request.NumberOfThumbnails)
                    .Future<PostThumbnailModel>();

                // Number of followers
                var followersCountQuery = _session.QueryOver<Follow>()
                    .Where(f => f.FollowedID == profile.MemberID)
                    .ToRowCountQuery()
                    .FutureValue<int>();

                // Number of followed members
                var followingCountQuery = _session.QueryOver<Follow>()
                    .Where(f => f.Follower.ID == profile.MemberID)
                    .ToRowCountQuery()
                    .FutureValue<int>();

                // Is the current member following the selected member?
                var isFollowedByCurrentMemberQuery = _session.QueryOver<Follow>()
                    .Where(f => f.FollowedID == profile.MemberID)
                    .And(f => f.Follower.ID == request.CurrentMemberID)
                    .ToRowCountQuery()
                    .FutureValue<int>();

                profile.NumberOfPosts = await postCountQuery.GetValueAsync(cancellationToken);
                profile.RecentPosts = (await thumbnailsQuery.GetEnumerableAsync(cancellationToken)).ToArray();
                profile.NumberOfFollowers = await followersCountQuery.GetValueAsync(cancellationToken);
                profile.NumberOfFollows = await followingCountQuery.GetValueAsync(cancellationToken);
                profile.IsCurrentMemberProfile = profile.MemberID == request.CurrentMemberID;

                profile.Following =
                    await isFollowedByCurrentMemberQuery.GetValueAsync(cancellationToken) == 1
                    || profile.IsCurrentMemberProfile;

                await tx.CommitAsync(cancellationToken);
            }

            return Result.Ok(profile);
        }
    }
}
