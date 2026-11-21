using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Web.Accounts;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class AccountSettingsFixture : IntegrationTestFixture
    {
        public AccountSettingsFixture(TestInfrastructureFactory infrastructure)
            : base(infrastructure)
        { }

        [Fact]
        public async Task Should_Change_The_Email_Address()
        {
            var account = await SaveMemberWithAccountAsync("emailchanger");

            var result = await SendAsync(new ChangeEmailCommand(account.ID, "Changed@Acme.com"));

            using (new AssertionScope())
            {
                result.IsSuccess.Should().BeTrue();
                (await LoadAsync(account.ID)).Email.Value.Should().Be("changed@acme.com");
            }
        }

        [Fact]
        public async Task Should_Not_Change_The_Email_To_One_Used_By_Another_Account()
        {
            var first = await SaveMemberWithAccountAsync("firstowner");
            var second = await SaveMemberWithAccountAsync("secondowner");

            var result = await SendAsync(new ChangeEmailCommand(second.ID, first.Email.Value.ToUpperInvariant()));

            using (new AssertionScope())
            {
                result.Error.Should().Be(AccountErrors.EmailInUse);
                (await LoadAsync(second.ID)).Email.Should().Be(second.Email);
            }
        }

        [Fact]
        public async Task Should_Not_Change_The_Email_To_An_Invalid_Address()
        {
            var account = await SaveMemberWithAccountAsync("invalidemail");

            var result = await SendAsync(new ChangeEmailCommand(account.ID, "xyz"));

            using (new AssertionScope())
            {
                result.IsFailure.Should().BeTrue();
                (await LoadAsync(account.ID)).Email.Should().Be(account.Email);
            }
        }

        [Fact]
        public async Task Should_Change_The_Password_And_Log_In_With_The_New_One()
        {
            var account = await SaveMemberWithAccountAsync("passwordchanger");

            var result = await SendAsync(new ChangePasswordCommand(account.ID, "password", "newpassword"));

            using (new AssertionScope())
            {
                result.IsSuccess.Should().BeTrue();
                (await SendAsync(new LogInCommand("passwordchanger", "newpassword"))).IsSuccess.Should().BeTrue();
                (await SendAsync(new LogInCommand("passwordchanger", "password"))).IsSuccess.Should().BeFalse();
                result.Value.Should().NotBe(account.SecurityStamp);
                (await LoadAsync(account.ID)).SecurityStamp.Should().Be(result.Value);
            }
        }

        [Fact]
        public async Task Should_Not_Change_The_Password_With_A_Wrong_Current_Password()
        {
            var account = await SaveMemberWithAccountAsync("wrongcurrent");

            var result = await SendAsync(new ChangePasswordCommand(account.ID, "wrong", "newpassword"));

            using (new AssertionScope())
            {
                result.Error.Should().Be(AccountErrors.WrongCurrentPassword);
                (await LoadAsync(account.ID)).Password.HashMatches("password").Should().BeTrue();
                (await LoadAsync(account.ID)).SecurityStamp.Should().Be(account.SecurityStamp);
            }
        }

        /// <summary>
        /// Saves a member with an account that has the default password, and returns the account.
        /// </summary>
        private async Task<Account> SaveMemberWithAccountAsync(string nickname)
        {
            var member = TestDataHelper.NewMember(nickname);
            var account = TestDataHelper.NewAccount(member);
            using var session = Infrastructure.OpenSession();
            await session.SaveAsync(member, TestContext.Current.CancellationToken);
            await session.SaveAsync(account, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
            return account;
        }

        /// <summary>
        /// Loads the account in a new session, to see what was saved.
        /// </summary>
        private async Task<Account> LoadAsync(AccountId accountID)
        {
            using var session = Infrastructure.OpenSession();
            return await session.GetAsync<Account>(accountID, TestContext.Current.CancellationToken);
        }
    }
}
