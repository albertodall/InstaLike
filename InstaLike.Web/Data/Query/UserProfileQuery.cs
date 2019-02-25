using System;
using System.Linq;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace InstaLike.Web.Data.Query
{
    public class UserProfileQuery : IQuery<UserProfileModel>
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

    internal sealed class UserProfileQueryHandler : IQueryHandler<UserProfileQuery, UserProfileModel>
    {
        private readonly ISession _session;

        public UserProfileQueryHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<UserProfileModel> HandleAsync(UserProfileQuery query)
        {
            UserProfileModel profile = null;

            using (var tx = _session.BeginTransaction())
            {
                var userProfileQuery = _session.QueryOver<User>()
                    .Where(Restrictions.Eq("Nickname", query.Nickname))
                    .SelectList(list => list
                        .Select(u => u.ID).WithAlias(() => profile.UserID)
                        .Select(u => u.Nickname).WithAlias(() => profile.Nickname)
                        .Select(u => u.Name).WithAlias(() => profile.Name)
                        .Select(u => u.Surname).WithAlias(() => profile.Surname)
                        .Select(u => u.Biography).WithAlias(() => profile.Bio)
                        .Select(u => u.ProfilePicture.RawBytes).WithAlias(() => profile.ProfilePicture)
                    )
                    .TransformUsing(Transformers.AliasToBean<UserProfileModel>());

                // Loads user's information
                profile = await userProfileQuery.SingleOrDefaultAsync<UserProfileModel>();

                // Number of posts published by this author. 
                var postCountQuery = _session.QueryOver<Post>()
                   .Where(p => p.Author.ID == profile.UserID)
                   .Select(Projections.Count<Post>(p => p.ID))
                   .FutureValue<int>();

                // Thumbnails of the latest published posts.
                PostThumbnailModel thumbnailModel = null;
                var thumbnailsQuery = _session.QueryOver<Post>()
                    .Where(p => p.Author.ID == profile.UserID)
                    .OrderBy(p => p.PostDate).Desc()
                    .Select(Projections.ProjectionList()
                        .Add(Projections.Property<Post>(p => p.ID)
                            .WithAlias(() => thumbnailModel.PostID))
                        .Add(Projections.Property<Post>(p => p.Picture.RawBytes)
                            .WithAlias(() => thumbnailModel.Picture))
                    )
                    .TransformUsing(Transformers.AliasToBean<PostThumbnailModel>())
                    .Take(query.NumberOfThumbnails)
                    .Future<PostThumbnailModel>();

                // Number of followers
                var followersCountQuery = _session.QueryOver<Follow>()
                    .Where(f => f.Following.ID == profile.UserID)
                    .Select(Projections.Count<Follow>(f => f.ID))
                    .FutureValue<int>();

                // Number of followed users
                var followingCountQuery = _session.QueryOver<Follow>()
                    .Where(f => f.Follower.ID == profile.UserID)
                    .Select(Projections.Count<Follow>(f => f.ID))
                    .FutureValue<int>();

                // Is the current user following the selected user?
                var isFollowedByCurrentUserQuery = _session.QueryOver<Follow>()
                    .Where(f => f.Following.ID == profile.UserID)
                    .And(f => f.Follower.ID == query.CurrentUserID)
                    .Select(Projections.Count<Follow>(f => f.ID))
                    .FutureValue<int>();

                profile.NumberOfPosts = await postCountQuery.GetValueAsync();
                profile.RecentPosts = (await thumbnailsQuery.GetEnumerableAsync()).ToArray();
                profile.NumberOfFollowers = await followersCountQuery.GetValueAsync();
                profile.NumberOfFollows = await followingCountQuery.GetValueAsync();
                profile.IsCurrentUserProfile = profile.UserID == query.CurrentUserID;

                if (profile.IsCurrentUserProfile)
                {
                    profile.Following = true;
                }
                else
                {
                    profile.Following = await isFollowedByCurrentUserQuery.GetValueAsync() == 1;
                }
            }

            return profile;
        }
    }
}
