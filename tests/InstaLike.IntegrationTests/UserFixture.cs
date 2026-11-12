using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Posts;
using InstaLike.Core.Social;
using InstaLike.Web.Accounts.Queries;
using InstaLike.Web.Models;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests;

public class UserFixture : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _testFixture;
    private readonly ITestOutputHelper _output;

    public UserFixture(DatabaseFixture fixture, ITestOutputHelper output)
    {
        _testFixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Should_Load_User_Profile()
    {
        UserProfileModel result;

        var author = TestDataHelper.NewUser("authoruser");
        var followed = TestDataHelper.NewUser("followeduser");
        var follower = TestDataHelper.NewUser("followeruser");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(followed, TestContext.Current.CancellationToken);
            await session.SaveAsync(follower, TestContext.Current.CancellationToken);
            await session.SaveAsync(author, TestContext.Current.CancellationToken);
            post = TestDataHelper.NewPost(author);
            await session.SaveAsync(post, TestContext.Current.CancellationToken);
            await session.SaveAsync(Follow.Create(author.ID, followed.ID).Value, TestContext.Current.CancellationToken);
            await session.SaveAsync(Follow.Create(follower.ID, author.ID).Value, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var query = new UserProfileQuery(follower.ID, author.Nickname, 3);
        using (var session = _testFixture.OpenStatelessSession())
        {
            var sut = new UserProfileQueryHandler(session, Log.Logger);
            result = await sut.Handle(query, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            result.IsCurrentUserProfile.Should().BeFalse();
            result.NumberOfFollowers.Should().Be(1);
            result.NumberOfFollows.Should().Be(1);
            result.NumberOfPosts.Should().Be(1);
            result.RecentPosts.Length.Should().Be(1);
        }
    }

    [Fact]
    public async Task Should_Read_User_Details_For_Editing()
    {
        UserDetailsModel result;

        var user = TestDataHelper.NewUser("testuser");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(user, TestContext.Current.CancellationToken);
        }

        var query = new UserDetailsQuery(user.ID);
        using (var session = _testFixture.OpenStatelessSession())
        {
            var sut = new UserDetailsQueryHandler(session, Log.Logger);
            result = await sut.Handle(query, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            result.Name.Should().Be(user.FullName.Name);
            result.Surname.Should().Be(user.FullName.Surname);
            result.Nickname.Should().Be(user.Nickname);
            result.Email.Should().Be(user.Email);
            result.Bio.Should().Be(user.Biography);
        }
    }
}
