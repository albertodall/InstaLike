using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Posts;
using InstaLike.Core.Social;
using InstaLike.Web.Membership.Queries;
using InstaLike.Web.Models;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class MemberFixture : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _testFixture;
        private readonly ITestOutputHelper _output;

        public MemberFixture(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testFixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task Should_Load_User_Profile()
        {
            MemberProfileModel result;

            var author = TestDataHelper.NewMember("authoruser");
            var followed = TestDataHelper.NewMember("followeduser");
            var follower = TestDataHelper.NewMember("followeruser");
            Post post;
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(followed, TestContext.Current.CancellationToken);
                await session.SaveAsync(follower, TestContext.Current.CancellationToken);
                await session.SaveAsync(author, TestContext.Current.CancellationToken);
                post = TestDataHelper.NewPost(author);
                await session.SaveAsync(post, TestContext.Current.CancellationToken);
                await session.SaveAsync(TestDataHelper.Following(author, followed), TestContext.Current.CancellationToken);
                await session.SaveAsync(TestDataHelper.Following(follower, author), TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var query = new MemberProfileQuery(follower.ID.Value, author.Nickname, 3);
            using (var session = _testFixture.OpenStatelessSession())
            {
                var sut = new MemberProfileQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, TestContext.Current.CancellationToken);
            }

            using (new AssertionScope())
            {
                result.IsCurrentMemberProfile.Should().BeFalse();
                result.NumberOfFollowers.Should().Be(1);
                result.NumberOfFollows.Should().Be(1);
                result.NumberOfPosts.Should().Be(1);
                result.RecentPosts.Length.Should().Be(1);
            }
        }

        [Fact]
        public async Task Should_Read_User_Details_For_Editing()
        {
            MemberDetailsModel result;

            var user = TestDataHelper.NewMember("testuser");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(user, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var query = new MemberDetailsQuery(user.ID);
            using (var session = _testFixture.OpenStatelessSession())
            {
                var sut = new MemberDetailsQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, TestContext.Current.CancellationToken);
            }

            using (new AssertionScope())
            {
                result.Name.Should().Be(user.FullName.Name);
                result.Surname.Should().Be(user.FullName.Surname);
                result.Nickname.Should().Be(user.Nickname);
                result.Bio.Should().Be(user.Biography);
            }
        }
    }
}
