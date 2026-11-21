using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Membership;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Accounts;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class AccountAuthenticationFixture : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _testFixture;
        private readonly ITestOutputHelper _output;

        public AccountAuthenticationFixture(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testFixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task Registered_Member_Should_Be_Able_To_Log_In_With_The_Username()
        {
            var member = await SaveMemberWithAccountAsync("testuser2", "password");
            Result<Guid> authenticationResult;

            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new TestMediatorBuilder(session).Build();
                authenticationResult = await sut.Send(new LogInCommand("testuser2", "password"), TestContext.Current.CancellationToken);
            }

            using (new AssertionScope())
            {
                authenticationResult.IsSuccess.Should().BeTrue();
                authenticationResult.Value.Should().Be(member.ID.Value);
            }
        }

        [Fact]
        public async Task Member_Should_Still_Log_In_With_The_Username_After_Changing_The_Nickname()
        {
            var member = await SaveMemberWithAccountAsync("testuser5", "password5");
            using (var session = _testFixture.OpenSession(_output))
            {
                var storedMember = await session.GetAsync<Member>(member.ID, TestContext.Current.CancellationToken);
                storedMember.ChangeNickname((Nickname)"renamed5");
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new TestMediatorBuilder(session).Build();

                using (new AssertionScope())
                {
                    (await sut.Send(new LogInCommand("testuser5", "password5"), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
                    (await sut.Send(new LogInCommand("renamed5", "password5"), TestContext.Current.CancellationToken)).IsSuccess.Should().BeFalse();
                }
            }
        }

        [Fact]
        public async Task Registered_Member_Should_Not_Be_Able_To_Log_In_With_Wrong_Password()
        {
            await SaveMemberWithAccountAsync("testuser3", "password3");
            Result<Guid> authenticationResult;

            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new TestMediatorBuilder(session).Build();
                authenticationResult = await sut.Send(new LogInCommand("testuser3", "xxx42"), TestContext.Current.CancellationToken);
            }

            authenticationResult.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Unknown_Username_Should_Not_Be_Able_To_Log_In()
        {
            Result<Guid> authenticationResult;

            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new TestMediatorBuilder(session).Build();
                authenticationResult = await sut.Send(new LogInCommand("not_existing_user", "xxx42"), TestContext.Current.CancellationToken);
            }

            authenticationResult.IsSuccess.Should().BeFalse();
        }

        /// <summary>
        /// Saves a member with an account that logs in with the nickname and the given password.
        /// </summary>
        private async Task<Member> SaveMemberWithAccountAsync(string nickname, string password)
        {
            var member = TestDataHelper.NewMember(nickname);
            using var session = _testFixture.OpenSession(_output);
            await session.SaveAsync(member, TestContext.Current.CancellationToken);
            await session.SaveAsync(TestDataHelper.NewAccountWithPassword(member, password), TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
            return member;
        }
    }
}
