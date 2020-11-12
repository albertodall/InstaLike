using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Domain;
using InstaLike.Web.Data.Query;
using InstaLike.Web.Models;
using Serilog;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace InstaLike.IntegrationTests
{
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
            
            var author = new User((Nickname)"authoruser", (FullName)"author user", Password.Create("password").Value, (Email)"authoruser@acme.com", "my bio");
            var followed = new User((Nickname)"followeduser", (FullName)"followed user", Password.Create("password").Value, (Email)"followed@acme.com", "my bio");
            var follower = new User((Nickname)"followeruser", (FullName)"follower user", Password.Create("password").Value, (Email)"follower@acme.com", "my bio");
            var post = new Post(author, (Picture)Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()), (PostText)"test post");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(followed);
                await session.SaveAsync(follower);
                await session.SaveAsync(author);
                await session.SaveAsync(post);
                author.Follow(followed);
                follower.Follow(author);
                await session.FlushAsync();
            }

            var query = new UserProfileQuery(follower.ID, author.Nickname, 3);
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new UserProfileQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, default);
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
            
            var user = new User((Nickname)"testuser", (FullName)"test user", Password.Create("password").Value, (Email)"testuser@acme.com", "my bio");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(user);
            }

            var query = new UserDetailsQuery(user.ID);
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new UserDetailsQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, default);
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
}