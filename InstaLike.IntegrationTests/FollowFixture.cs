using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using InstaLike.Web.CommandHandlers;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public class FollowFixture : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _testFixture;

        public FollowFixture(DatabaseFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async Task User_Should_Follow_Another_User()
        {
            User user1;
            User user2;
            Result followResult;

            using (var session = _testFixture.OpenSession())
            {
                user1 = new User((Nickname)"user1", (FullName)"test1 user1", (Password)"password", (Email)"testuser1@acme.com", "bio1");
                user2 = new User((Nickname)"user2", (FullName)"test2 user2", (Password)"password", (Email)"testuser2@acme.com", "bio2");

                using (var tx = session.BeginTransaction())
                {
                    await session.SaveAsync(user1);
                    await session.SaveAsync(user2);
                    await tx.CommitAsync();
                }
            }

            var followCommand = new FollowCommand(user1.ID, "user2");
            using (var session = _testFixture.OpenSession())
            {
                var handler = new FollowCommandHandler(session, Log.Logger);
                followResult = await handler.Handle(followCommand, default);
            }

            using (new AssertionScope())
            {
                followResult.IsSuccess.Should().BeTrue();
                using (var session = _testFixture.OpenSession())
                {
                    session.Get<User>(user1.ID).Followers.Count.Should().Be(1);
                    session.Get<User>(user2.ID).Followed.Count.Should().Be(1);
                }
            }
        }
    }
}
