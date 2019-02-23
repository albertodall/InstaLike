using System;
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
            UserProfileModel result = null;

            using (var tx = _session.BeginTransaction())
            {
                var authorQuery = _session.QueryOver<User>()
                    .Fetch(SelectMode.FetchLazyProperties, u => u)
                    .Fetch(SelectMode.Skip, u => u.Followers, u => u.Following)
                    .Where(Restrictions.Eq("Nickname", query.Nickname));

                var author = await authorQuery.SingleOrDefaultAsync();

                // Number of posts published by this author.
                var postCountQuery = _session.QueryOver<Post>()
                    .Where(p => p.Author == author)
                    .Select(Projections.RowCount())
                    .FutureValue<int>();

                // Thumbnails of the latest published posts.
                PostThumbnailModel thumbnailModel = null;
                var thumbnailsQuery = _session.QueryOver<Post>()
                    .Where(p => p.Author == author)
                    .OrderBy(p => p.PostDate).Desc()
                    .Select(Projections.ProjectionList()
                        .Add(Projections.Property<Post>(p => p.ID)
                            .WithAlias(() => thumbnailModel.PostID))
                        .Add(Projections.Property<Post>(p => p.Picture)
                            .WithAlias(() => thumbnailModel.Picture))
                    )
                    .TransformUsing(Transformers.AliasToBean<PostThumbnailModel>())
                    .Take(query.NumberOfThumbnails)
                    .Future<PostThumbnailModel>();
                
                // Number of followers
                var followersCountQuery = _session.QueryOver<Follow>()
                    .Where(f => f.Following == author)
                    .Select(Projections.RowCount())
                    .FutureValue<int>();

                // Number of followed users
                var followingCountQuery = _session.QueryOver<Follow>()
                    .Where(f => f.Follower == author)
                    .Select(Projections.RowCount())
                    .FutureValue<int>();

                // Is the current user following the selected user?
                var isFollowedByCurrentUserQuery =_session.QueryOver<Follow>()
                    .Where(f => f.Following == author)
                    .And(f => f.Follower.ID == query.CurrentUserID)
                    .Select(Projections.RowCount())
                    .FutureValue<int>();

                result = new UserProfileModel()
                {
                    Nickname = author.Nickname,
                    Name = author.Name,
                    Surname = author.Surname,
                    Bio = author.Biography,
                    ProfilePicture = author.ProfilePicture,
                    NumberOfPosts = postCountQuery.Value,
                    // NumberOfFollowers = followersCountQuery.Value,
                    // NumberOfFollows = followingCountQuery.Value,
                    IsCurrentUserProfile = author.ID == query.CurrentUserID
                };

                if (result.IsCurrentUserProfile)
                {
                    result.Following = true;
                }
                else
                {
                    result.Following = isFollowedByCurrentUserQuery.Value == 1;
                }
            }

            return result;
        }
    }
}
