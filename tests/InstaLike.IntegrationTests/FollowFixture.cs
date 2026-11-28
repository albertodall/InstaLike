using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.SharedKernel;
using InstaLike.Core.Social;
using InstaLike.Web.Social;
using NHibernate;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class FollowFixture : IntegrationTestFixture
    {
        public FollowFixture(TestInfrastructureFactory infrastructure)
            : base(infrastructure)
        { }

        [Fact]
        public async Task User_Should_Follow_Another_User()
        {
            var followerUser = TestDataHelper.NewMember("user1");
            var followedUser = TestDataHelper.NewMember("user2");

            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var followCommand = new FollowCommand(followerUser.ID, followedUser.Nickname);
            var followResult = await SendAsync(followCommand);

            using (new AssertionScope())
            {
                followResult.IsSuccess.Should().BeTrue();
                using (var session = Infrastructure.OpenSession())
                {
                    (await CountFollowsAsync(session, followerUser.ID, followedUser.ID)).Should().Be(1);
                }
            }
        }

        [Fact]
        public async Task User_Should_Unfollow_Another_User()
        {
            var followerUser = TestDataHelper.NewMember("user3");
            var followedUser = TestDataHelper.NewMember("user4");

            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(TestDataHelper.Following(followerUser, followedUser), TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var unfollowCommand = new UnfollowCommand(followerUser.ID, followedUser.Nickname);
            var unfollowResult = await SendAsync(unfollowCommand);

            using (new AssertionScope())
            {
                unfollowResult.IsSuccess.Should().BeTrue();
                using (var session = Infrastructure.OpenSession())
                {
                    (await CountFollowsAsync(session, followerUser.ID, followedUser.ID)).Should().Be(0);
                }
            }
        }

        [Fact]
        public async Task Should_Not_Follow_User_Twice()
        {
            var followerUser = TestDataHelper.NewMember("user5");
            var followedUser = TestDataHelper.NewMember("user6");

            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var followCommand = new FollowCommand(followerUser.ID, followedUser.Nickname);
            await SendAsync(followCommand);

            var followResult = await SendAsync(followCommand);

            using (new AssertionScope())
            {
                followResult.IsSuccess.Should()
                    .BeFalse($"User [{followedUser.Nickname}] is already followed by user [{followerUser.Nickname}].");
                using (var session = Infrastructure.OpenSession())
                {
                    (await CountFollowsAsync(session, followerUser.ID, followedUser.ID)).Should().Be(1);
                }
            }
        }

        [Fact]
        public async Task Should_Not_Unfollow_Non_Followed_User()
        {
            var followerUser = TestDataHelper.NewMember("user7");
            var followedUser = TestDataHelper.NewMember("user8");

            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var unfollowCommand = new UnfollowCommand(followerUser.ID, followedUser.Nickname);
            var unfollowResult = await SendAsync(unfollowCommand);

            unfollowResult.IsSuccess.Should()
                .BeFalse($"User [{followerUser.Nickname}] is not following user [{followedUser.Nickname}].");
        }

        [Fact]
        public async Task Should_Not_Follow_A_User_That_Does_Not_Exist()
        {
            var followerUser = TestDataHelper.NewMember("user13");
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            (await SendAsync(new FollowCommand(followerUser.ID, "nobody"))).IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task User_Should_Not_Follow_Themselves()
        {
            var user = TestDataHelper.NewMember("user14");
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(user, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            using (var session = Infrastructure.OpenSession())
            {
                var handler = new TestMediatorBuilder(session, Infrastructure.PictureStore).Build();
                (await handler.Send(new FollowCommand(user.ID, user.Nickname), TestContext.Current.CancellationToken)).IsSuccess.Should().BeFalse();
                (await CountFollowsAsync(session, user.ID, user.ID)).Should().Be(0);
            }
        }

        [Fact]
        public async Task User_Should_Have_One_Follower()
        {
            var followerUser = TestDataHelper.NewMember("user9");
            var followedUser = TestDataHelper.NewMember("user10");
            FollowModel[] result;

            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(TestDataHelper.Following(followerUser, followedUser), TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var followersQuery = new FollowersQuery(followedUser.Nickname);
            using (var session = Infrastructure.OpenStatelessSession())
            {
                var sut = new FollowersQueryHandler(session, Log.Logger);
                result = await sut.Handle(followersQuery, TestContext.Current.CancellationToken);
            }

            result.Length.Should().Be(1);
        }

        [Fact]
        public async Task User_Should_Have_Been_Following_One_User()
        {
            var followerUser = TestDataHelper.NewMember("user11");
            var followedUser = TestDataHelper.NewMember("user12");
            FollowModel[] result;

            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(TestDataHelper.Following(followerUser, followedUser), TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var followingQuery = new FollowingQuery(followerUser.Nickname);
            using (var session = Infrastructure.OpenStatelessSession())
            {
                var sut = new FollowingQueryHandler(session, Log.Logger);
                result = await sut.Handle(followingQuery, TestContext.Current.CancellationToken);
            }

            result.Length.Should().Be(1);
        }

        [Fact]
        public async Task First_Follow_Should_Make_The_Member_A_Follower()
        {
            var followerMember = TestDataHelper.NewMember("user15");
            var followedMember = TestDataHelper.NewMember("user16");

            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(followerMember, TestContext.Current.CancellationToken);
                await session.SaveAsync(followedMember, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            using (var session = Infrastructure.OpenSession())
            {
                (await CountFollowersAsync(session, followerMember.ID)).Should().Be(0);
            }

            await SendAsync(new FollowCommand(followerMember.ID, followedMember.Nickname));

            using (var session = Infrastructure.OpenSession())
            {
                (await CountFollowersAsync(session, followerMember.ID)).Should().Be(1);
            }
        }

        [Fact]
        public async Task Follower_Should_Follow_Another_Member()
        {
            var followerMember = TestDataHelper.NewMember("user17");
            var firstFollowedMember = TestDataHelper.NewMember("user18");
            var secondFollowedMember = TestDataHelper.NewMember("user19");

            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(followerMember, TestContext.Current.CancellationToken);
                await session.SaveAsync(firstFollowedMember, TestContext.Current.CancellationToken);
                await session.SaveAsync(secondFollowedMember, TestContext.Current.CancellationToken);
                await session.SaveAsync(TestDataHelper.Following(followerMember, firstFollowedMember), TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var followResult = await SendAsync(new FollowCommand(followerMember.ID, secondFollowedMember.Nickname));

            using (new AssertionScope())
            {
                followResult.IsSuccess.Should().BeTrue();
                using (var session = Infrastructure.OpenSession())
                {
                    (await CountFollowersAsync(session, followerMember.ID)).Should().Be(1);
                    (await CountFollowsAsync(session, followerMember.ID, firstFollowedMember.ID)).Should().Be(1);
                    (await CountFollowsAsync(session, followerMember.ID, secondFollowedMember.ID)).Should().Be(1);
                }
            }
        }

        /// <summary>
        /// How many times the follower follows the followed member.
        /// </summary>
        private static Task<int> CountFollowsAsync(ISession session, MemberId followerID, MemberId followedID) =>
            session.QueryOver<Follow>()
                .Where(f => f.Follower.ID == followerID && f.FollowedID == followedID)
                .RowCountAsync();

        /// <summary>
        /// 1 when the member has a follower record, created on their first follow, otherwise 0.
        /// </summary>
        private static Task<int> CountFollowersAsync(ISession session, MemberId memberID) =>
            session.QueryOver<Follower>()
                .Where(f => f.ID == memberID)
                .RowCountAsync();
    }
}
