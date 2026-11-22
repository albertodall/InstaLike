using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Web.Accounts;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class AccountAuthenticationFixture : IntegrationTestFixture
    {
        public AccountAuthenticationFixture(TestInfrastructureFactory infrastructure)
            : base(infrastructure)
        { }

        [Fact]
        public async Task Registered_Member_Should_Be_Able_To_Log_In_With_The_Username()
        {
            var member = await SaveMemberWithAccountAsync("testuser2", "password");
            var authenticationResult = await SendAsync(new LogInCommand("testuser2", "password"));

            using (new AssertionScope())
            {
                authenticationResult.IsSuccess.Should().BeTrue();
                authenticationResult.Value.MemberID.Should().Be(member.ID.Value);
                authenticationResult.Value.SecurityStamp.Should().Be((await LoadAccountAsync(member.ID.Value)).SecurityStamp);
            }
        }

        [Fact]
        public async Task Member_Should_Still_Log_In_With_The_Username_After_Changing_The_Nickname()
        {
            var member = await SaveMemberWithAccountAsync("testuser5", "password5");
            using (var session = Infrastructure.OpenSession())
            {
                var storedMember = await session.GetAsync<Member>(member.ID, TestContext.Current.CancellationToken);
                storedMember.ChangeNickname((Nickname)"renamed5");
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            using (new AssertionScope())
            {
                (await SendAsync(new LogInCommand("testuser5", "password5"))).IsSuccess.Should().BeTrue();
                (await SendAsync(new LogInCommand("renamed5", "password5"))).IsSuccess.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Registered_Member_Should_Not_Be_Able_To_Log_In_With_Wrong_Password()
        {
            await SaveMemberWithAccountAsync("testuser3", "password3");
            var authenticationResult = await SendAsync(new LogInCommand("testuser3", "xxx42"));

            authenticationResult.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Unknown_Username_Should_Not_Be_Able_To_Log_In()
        {
            var authenticationResult = await SendAsync(new LogInCommand("not_existing_user", "xxx42"));

            authenticationResult.IsSuccess.Should().BeFalse();
        }

        /// <summary>
        /// Saves a member with an account that logs in with the nickname and the given password.
        /// </summary>
        [Fact]
        public async Task Five_Failed_Logins_Should_Lock_The_Account()
        {
            var member = await SaveMemberWithAccountAsync("lockeduser1", "password");

            await LogInWithWrongPasswordAsync("lockeduser1", 5);
            var authenticationResult = await SendAsync(new LogInCommand("lockeduser1", "password"));

            using (new AssertionScope())
            {
                authenticationResult.Error.Should().Be(AccountErrors.LockedOut);
                (await LoadAccountAsync(member.ID.Value)).LockedOutUntil.Should().BeAfter(DateTimeOffset.UtcNow);
            }
        }

        [Fact]
        public async Task Locked_Out_Account_Should_Log_In_Again_When_The_Lockout_Ends()
        {
            var member = await SaveMemberWithAccountAsync("lockeduser2", "password");
            await LogInWithWrongPasswordAsync("lockeduser2", 5);

            await EndLockOutAsync(member.ID.Value);
            var authenticationResult = await SendAsync(new LogInCommand("lockeduser2", "password"));

            authenticationResult.IsSuccess.Should().BeTrue();
        }

        private async Task LogInWithWrongPasswordAsync(string username, int times)
        {
            for (var attempt = 0; attempt < times; attempt++)
            {
                (await SendAsync(new LogInCommand(username, "wrong"))).IsFailure.Should().BeTrue();
            }
        }

        /// <summary>
        /// Moves the end of the lockout into the past, as if its fifteen minutes had gone by.
        /// </summary>
        private async Task EndLockOutAsync(Guid memberID)
        {
            using var session = Infrastructure.OpenSession();
            await session.CreateSQLQuery("UPDATE [Account] SET [LockedOutUntil] = DATEADD(minute, -1, SYSDATETIMEOFFSET()) WHERE [ID] = :id")
                .SetGuid("id", memberID)
                .ExecuteUpdateAsync(TestContext.Current.CancellationToken);
        }

        private async Task<Member> SaveMemberWithAccountAsync(string nickname, string password)
        {
            var member = TestDataHelper.NewMember(nickname);
            using var session = Infrastructure.OpenSession();
            await session.SaveAsync(member, TestContext.Current.CancellationToken);
            await session.SaveAsync(TestDataHelper.NewAccountWithPassword(member, password), TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
            return member;
        }

        private async Task<Account> LoadAccountAsync(Guid memberID)
        {
            using var session = Infrastructure.OpenSession();
            return await session.GetAsync<Account>(AccountId.From(memberID), TestContext.Current.CancellationToken);
        }
    }
}
