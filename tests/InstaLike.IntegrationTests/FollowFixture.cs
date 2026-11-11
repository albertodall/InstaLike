using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Social;
using InstaLike.Web.Models;
using InstaLike.Web.Social.Commands;
using InstaLike.Web.Social.Queries;
using NHibernate;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests;

public class FollowFixture : IClassFixture<DatabaseFixture>
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
        var followerUser = new User((Nickname)"user1", (FullName)"test1 user1", Password.Create("password").Value, (Email)"testuser1@acme.com", "bio1");
        var followedUser = new User((Nickname)"user2", (FullName)"test2 user2", Password.Create("password").Value, (Email)"testuser2@acme.com", "bio2");
        Result followResult;

        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
            await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
        }

        var followCommand = new FollowCommand(followerUser.ID, followedUser.Nickname);
        using (var session = _testFixture.OpenSession(_output))
        {
            var handler = DatabaseFixture.CreateMediator(session);
            followResult = await handler.Send(followCommand, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            followResult.IsSuccess.Should().BeTrue();
            using (var session = _testFixture.OpenSession(_output))
            {
                (await CountFollowsAsync(session, followerUser.ID, followedUser.ID)).Should().Be(1);
            }
        }
    }

    [Fact]
    public async Task User_Should_Unfollow_Another_User()
    {
        var followerUser = new User((Nickname)"user3", (FullName)"test3 user3", Password.Create("password").Value, (Email)"testuser3@acme.com", "bio3");
        var followedUser = new User((Nickname)"user4", (FullName)"test4 user4", Password.Create("password").Value, (Email)"testuser4@acme.com", "bio4");
        Result unfollowResult;

        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
            await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
            await session.SaveAsync(Follow.Create(followerUser.ID, followedUser.ID).Value, TestContext.Current.CancellationToken);
        }

        var unfollowCommand = new UnfollowCommand(followerUser.ID, followedUser.Nickname);
        using (var session = _testFixture.OpenSession(_output))
        {
            var handler = DatabaseFixture.CreateMediator(session);
            unfollowResult = await handler.Send(unfollowCommand, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            unfollowResult.IsSuccess.Should().BeTrue();
            using (var session = _testFixture.OpenSession(_output))
            {
                (await CountFollowsAsync(session, followerUser.ID, followedUser.ID)).Should().Be(0);
            }
        }
    }

    [Fact]
    public async Task Should_Not_Follow_User_Twice()
    {
        var followerUser = new User((Nickname)"user5", (FullName)"test5 user5", Password.Create("password").Value, (Email)"testuser5@acme.com", "bio5");
        var followedUser = new User((Nickname)"user6", (FullName)"test6 user6", Password.Create("password").Value, (Email)"testuser6@acme.com", "bio6");
        Result followResult;

        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
            await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
        }

        var followCommand = new FollowCommand(followerUser.ID, followedUser.Nickname);
        using (var session = _testFixture.OpenSession(_output))
        {
            var handler = DatabaseFixture.CreateMediator(session);
            await handler.Send(followCommand, TestContext.Current.CancellationToken);
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            var handler = DatabaseFixture.CreateMediator(session);
            followResult = await handler.Send(followCommand, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            followResult.IsSuccess.Should()
                .BeFalse($"User [{followedUser.Nickname}] is already followed by user [{followerUser.Nickname}].");
            using (var session = _testFixture.OpenSession(_output))
            {
                (await CountFollowsAsync(session, followerUser.ID, followedUser.ID)).Should().Be(1);
            }
        }
    }

    [Fact]
    public async Task Should_Not_Unfollow_Non_Followed_User()
    {
        var followerUser = new User((Nickname)"user7", (FullName)"test7 user7", Password.Create("password").Value, (Email)"testuser7@acme.com", "bio7");
        var followedUser = new User((Nickname)"user8", (FullName)"test8 user8", Password.Create("password").Value, (Email)"testuser8@acme.com", "bio8");
        Result unfollowResult;

        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
            await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
        }

        var unfollowCommand = new UnfollowCommand(followerUser.ID, followedUser.Nickname);
        using (var session = _testFixture.OpenSession(_output))
        {
            var handler = DatabaseFixture.CreateMediator(session);
            unfollowResult = await handler.Send(unfollowCommand, TestContext.Current.CancellationToken);
        }

        unfollowResult.IsSuccess.Should()
            .BeFalse($"User [{followerUser.Nickname}] is not following user [{followedUser.Nickname}].");
    }

    [Fact]
    public async Task Should_Not_Follow_A_User_That_Does_Not_Exist()
    {
        var followerUser = new User((Nickname)"user13", (FullName)"test13 user13", Password.Create("password").Value, (Email)"testuser13@acme.com", "bio13");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            var handler = DatabaseFixture.CreateMediator(session);
            (await handler.Send(new FollowCommand(followerUser.ID, "nobody"), TestContext.Current.CancellationToken)).IsSuccess.Should().BeFalse();
        }
    }

    [Fact]
    public async Task User_Should_Not_Follow_Themselves()
    {
        var user = new User((Nickname)"user14", (FullName)"test14 user14", Password.Create("password").Value, (Email)"testuser14@acme.com", "bio14");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(user, TestContext.Current.CancellationToken);
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            var handler = DatabaseFixture.CreateMediator(session);
            (await handler.Send(new FollowCommand(user.ID, user.Nickname), TestContext.Current.CancellationToken)).IsSuccess.Should().BeFalse();
            (await CountFollowsAsync(session, user.ID, user.ID)).Should().Be(0);
        }
    }

    [Fact]
    public async Task User_Should_Have_One_Follower()
    {
        var followerUser = new User((Nickname)"user9", (FullName)"test9 user9", Password.Create("password").Value, (Email)"testuser9@acme.com", "bio9");
        var followedUser = new User((Nickname)"user10", (FullName)"test10 user10", Password.Create("password").Value, (Email)"testuser10@acme.com", "bio10");
        FollowModel[] result;

        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
            await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
            await session.SaveAsync(Follow.Create(followerUser.ID, followedUser.ID).Value, TestContext.Current.CancellationToken);
        }

        var followersQuery = new FollowersQuery(followedUser.Nickname);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new FollowersQueryHandler(session, Log.Logger);
            result = await sut.Handle(followersQuery, TestContext.Current.CancellationToken);
        }

        result.Length.Should().Be(1);
    }

    [Fact]
    public async Task User_Should_Have_Been_Following_One_User()
    {
        var followerUser = new User((Nickname)"user11", (FullName)"test11 user11", Password.Create("password").Value, (Email)"testuser11@acme.com", "bio11");
        var followedUser = new User((Nickname)"user12", (FullName)"test12 user12", Password.Create("password").Value, (Email)"testuser12@acme.com", "bio12");
        FollowModel[] result;

        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(followedUser, TestContext.Current.CancellationToken);
            await session.SaveAsync(followerUser, TestContext.Current.CancellationToken);
            await session.SaveAsync(Follow.Create(followerUser.ID, followedUser.ID).Value, TestContext.Current.CancellationToken);
        }

        var followingQuery = new FollowingQuery(followerUser.Nickname);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new FollowingQueryHandler(session, Log.Logger);
            result = await sut.Handle(followingQuery, TestContext.Current.CancellationToken);
        }

        result.Length.Should().Be(1);
    }

    private static Task<int> CountFollowsAsync(ISession session, int followerID, int followedID) =>
        session.QueryOver<Follow>()
            .Where(f => f.FollowerID == followerID && f.FollowedID == followedID)
            .RowCountAsync();
}
