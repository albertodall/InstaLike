using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using InstaLike.Web.CommandHandlers;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace InstaLike.IntegrationTests
{
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
            User user1;
            User user2;
            Result followResult;

            using (var session = _testFixture.OpenSession(_output))
            {
                user1 = new User((Nickname)"user1", (FullName)"test1 user1", Password.Create("password").Value, (Email)"testuser1@acme.com", "bio1");
                user2 = new User((Nickname)"user2", (FullName)"test2 user2", Password.Create("password").Value, (Email)"testuser2@acme.com", "bio2");

                await session.SaveAsync(user1);
                await session.SaveAsync(user2);
            }

            var followCommand = new FollowCommand(user1.ID, "user2");
            using (var session = _testFixture.OpenSession(_output))
            {
                var handler = new FollowCommandHandler(session, Log.Logger);
                followResult = await handler.Handle(followCommand, default);
            }

            using (new AssertionScope())
            {
                followResult.IsSuccess.Should().BeTrue();
                using (var session = _testFixture.OpenSession(_output))
                {
                    session.Get<User>(user1.ID).Followers.Count.Should().Be(1);
                    session.Get<User>(user2.ID).Followed.Count.Should().Be(1);
                }
            }
        }
    }
}
