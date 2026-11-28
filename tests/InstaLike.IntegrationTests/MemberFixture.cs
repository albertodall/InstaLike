using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Membership;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Membership;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class MemberFixture : IntegrationTestFixture
    {
        public MemberFixture(TestInfrastructureFactory infrastructure)
            : base(infrastructure)
        { }

        [Fact]
        public async Task Should_Load_User_Profile()
        {
            Result<MemberProfileModel> result;

            var author = TestDataHelper.NewMember("authoruser");
            var followed = TestDataHelper.NewMember("followeduser");
            var follower = TestDataHelper.NewMember("followeruser");
            Post post;
            using (var session = Infrastructure.OpenSession())
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

            var query = new MemberProfileQuery(follower.ID, author.Nickname, 3);
            using (var session = Infrastructure.OpenStatelessSession())
            {
                var sut = new MemberProfileQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, TestContext.Current.CancellationToken);
            }

            using (new AssertionScope())
            {
                result.IsSuccess.Should().BeTrue();
                result.Value.MemberID.Should().Be(author.ID);
                result.Value.Nickname.Should().Be(author.Nickname);
                result.Value.Name.Should().Be(author.FullName.Name);
                result.Value.Surname.Should().Be(author.FullName.Surname);
                result.Value.Bio.Should().Be(author.Biography);
                result.Value.IsCurrentMemberProfile.Should().BeFalse();
                result.Value.Following.Should().BeTrue();
                result.Value.NumberOfFollowers.Should().Be(1);
                result.Value.NumberOfFollows.Should().Be(1);
                result.Value.NumberOfPosts.Should().Be(1);
                result.Value.RecentPosts.Should().ContainSingle().Which.PostID.Should().Be(post.ID);
            }
        }

        [Fact]
        public async Task Should_Not_Find_The_Profile_Of_A_Missing_Member()
        {
            Result<MemberProfileModel> result;

            var query = new MemberProfileQuery(MemberId.New(), "nobody", 3);
            using (var session = Infrastructure.OpenStatelessSession())
            {
                var sut = new MemberProfileQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, TestContext.Current.CancellationToken);
            }

            result.Error.Should().Be(MemberErrors.NotFound("nobody"));
        }

        [Fact]
        public async Task Should_Read_User_Details_For_Editing()
        {
            EditMemberProfileModel result;

            var user = TestDataHelper.NewMember("testuser");
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(user, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var query = new EditMemberProfileQuery(user.ID);
            using (var session = Infrastructure.OpenStatelessSession())
            {
                var sut = new EditMemberProfileQueryHandler(session, Log.Logger);
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
