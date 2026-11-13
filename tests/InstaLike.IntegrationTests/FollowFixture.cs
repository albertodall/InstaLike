using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Social;
using InstaLike.Web.Models;
using InstaLike.Web.Social.Commands;
using InstaLike.Web.Social.Queries;
using NHibernate;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class FollowFixture : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _testFixture;
        private readonly ITestOutputHelper _output;

        public FollowFixture(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testFixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task User_Should_Follow_Another_User()
        {
            var followerUser = TestDataHelper.NewMember("user1");
            var followedUser = TestDataHelper.NewMember("user2");
            Result followResult;

            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var followCommand = new FollowCommand(followerUser.ID.Value, followedUser.Nickname);
            using (var session = _testFixture.OpenSession(_output))
            {
                var handler = new TestMediatorBuilder(session).Build();
                followResult = await handler.Send(followCommand, TestContext.Current.CancellationToken);
            }

            using (new AssertionScope())
            {
                followResult.IsSuccess.Should().BeTrue();
                using (var session = _testFixture.OpenSession(_output))
                {
                    (await CountFollowsAsync(session, followerUser.ID.Value, followedUser.ID.Value)).Should().Be(1);
                }
            }
        }

        [Fact]
        public async Task User_Should_Unfollow_Another_User()
        {
            var followerUser = TestDataHelper.NewMember("user3");
            var followedUser = TestDataHelper.NewMember("user4");
            Result unfollowResult;

            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(TestDataHelper.Following(followerUser, followedUser), TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var unfollowCommand = new UnfollowCommand(followerUser.ID.Value, followedUser.Nickname);
            using (var session = _testFixture.OpenSession(_output))
            {
                var handler = new TestMediatorBuilder(session).Build();
                unfollowResult = await handler.Send(unfollowCommand, TestContext.Current.CancellationToken);
            }

            using (new AssertionScope())
            {
                unfollowResult.IsSuccess.Should().BeTrue();
                using (var session = _testFixture.OpenSession(_output))
                {
                    (await CountFollowsAsync(session, followerUser.ID.Value, followedUser.ID.Value)).Should().Be(0);
                }
            }
        }

        [Fact]
        public async Task Should_Not_Follow_User_Twice()
        {
            var followerUser = TestDataHelper.NewMember("user5");
            var followedUser = TestDataHelper.NewMember("user6");
            Result followResult;

            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var followCommand = new FollowCommand(followerUser.ID.Value, followedUser.Nickname);
            using (var session = _testFixture.OpenSession(_output))
            {
                var handler = new TestMediatorBuilder(session).Build();
                await handler.Send(followCommand, TestContext.Current.CancellationToken);
            }

            using (var session = _testFixture.OpenSession(_output))
            {
                var handler = new TestMediatorBuilder(session).Build();
                followResult = await handler.Send(followCommand, TestContext.Current.CancellationToken);
            }

            using (new AssertionScope())
            {
                followResult.IsSuccess.Should()
                    .BeFalse($"User [{followedUser.Nickname}] is already followed by user [{followerUser.Nickname}].");
                using (var session = _testFixture.OpenSession(_output))
                {
                    (await CountFollowsAsync(session, followerUser.ID.Value, followedUser.ID.Value)).Should().Be(1);
                }
            }
        }

        [Fact]
        public async Task Should_Not_Unfollow_Non_Followed_User()
        {
            var followerUser = TestDataHelper.NewMember("user7");
            var followedUser = TestDataHelper.NewMember("user8");
            Result unfollowResult;

            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var unfollowCommand = new UnfollowCommand(followerUser.ID.Value, followedUser.Nickname);
            using (var session = _testFixture.OpenSession(_output))
            {
                var handler = new TestMediatorBuilder(session).Build();
                unfollowResult = await handler.Send(unfollowCommand, TestContext.Current.CancellationToken);
            }

            unfollowResult.IsSuccess.Should()
                .BeFalse($"User [{followerUser.Nickname}] is not following user [{followedUser.Nickname}].");
        }

        [Fact]
        public async Task Should_Not_Follow_A_User_That_Does_Not_Exist()
        {
            var followerUser = TestDataHelper.NewMember("user13");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            using (var session = _testFixture.OpenSession(_output))
            {
                var handler = new TestMediatorBuilder(session).Build();
                (await handler.Send(new FollowCommand(followerUser.ID.Value, "nobody"), TestContext.Current.CancellationToken)).IsSuccess.Should().BeFalse();
            }
        }

        [Fact]
        public async Task User_Should_Not_Follow_Themselves()
        {
            var user = TestDataHelper.NewMember("user14");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(user, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            using (var session = _testFixture.OpenSession(_output))
            {
                var handler = new TestMediatorBuilder(session).Build();
                (await handler.Send(new FollowCommand(user.ID.Value, user.Nickname), TestContext.Current.CancellationToken)).IsSuccess.Should().BeFalse();
                (await CountFollowsAsync(session, user.ID.Value, user.ID.Value)).Should().Be(0);
            }
        }

        [Fact]
        public async Task User_Should_Have_One_Follower()
        {
            var followerUser = TestDataHelper.NewMember("user9");
            var followedUser = TestDataHelper.NewMember("user10");
            FollowModel[] result;

            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(TestDataHelper.Following(followerUser, followedUser), TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var followersQuery = new FollowersQuery(followedUser.Nickname);
            using (var session = _testFixture.OpenStatelessSession())
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

            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
                await session.SaveAsync(TestDataHelper.Following(followerUser, followedUser), TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var followingQuery = new FollowingQuery(followerUser.Nickname);
            using (var session = _testFixture.OpenStatelessSession())
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

            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(followerMember, TestContext.Current.CancellationToken);
                await session.SaveAsync(followedMember, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            using (var session = _testFixture.OpenSession(_output))
            {
                (await CountFollowersAsync(session, followerMember.ID.Value)).Should().Be(0);
            }

            using (var session = _testFixture.OpenSession(_output))
            {
                var handler = new TestMediatorBuilder(session).Build();
                await handler.Send(new FollowCommand(followerMember.ID.Value, followedMember.Nickname), TestContext.Current.CancellationToken);
            }

            using (var session = _testFixture.OpenSession(_output))
            {
                (await CountFollowersAsync(session, followerMember.ID.Value)).Should().Be(1);
            }
        }

        [Fact]
        public async Task Follower_Should_Follow_Another_Member()
        {
            var followerMember = TestDataHelper.NewMember("user17");
            var firstFollowedMember = TestDataHelper.NewMember("user18");
            var secondFollowedMember = TestDataHelper.NewMember("user19");
            Result followResult;

            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(followerMember, TestContext.Current.CancellationToken);
                await session.SaveAsync(firstFollowedMember, TestContext.Current.CancellationToken);
                await session.SaveAsync(secondFollowedMember, TestContext.Current.CancellationToken);
                await session.SaveAsync(TestDataHelper.Following(followerMember, firstFollowedMember), TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            using (var session = _testFixture.OpenSession(_output))
            {
                var handler = new TestMediatorBuilder(session).Build();
                followResult = await handler.Send(new FollowCommand(followerMember.ID.Value, secondFollowedMember.Nickname), TestContext.Current.CancellationToken);
            }

            using (new AssertionScope())
            {
                followResult.IsSuccess.Should().BeTrue();
                using (var session = _testFixture.OpenSession(_output))
                {
                    (await CountFollowersAsync(session, followerMember.ID.Value)).Should().Be(1);
                    (await CountFollowsAsync(session, followerMember.ID.Value, firstFollowedMember.ID.Value)).Should().Be(1);
                    (await CountFollowsAsync(session, followerMember.ID.Value, secondFollowedMember.ID.Value)).Should().Be(1);
                }
            }
        }

        private static Task<int> CountFollowsAsync(ISession session, Guid followerID, Guid followedID) =>
            session.QueryOver<Follow>()
                .Where(f => f.Follower.ID == FollowerId.From(followerID) && f.FollowedID == followedID)
                .RowCountAsync();

        private static Task<int> CountFollowersAsync(ISession session, Guid memberID) =>
            session.QueryOver<Follower>()
                .Where(f => f.ID == FollowerId.From(memberID))
                .RowCountAsync();
    }
}
