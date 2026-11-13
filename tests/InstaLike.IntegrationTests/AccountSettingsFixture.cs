using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Web.Accounts.Commands;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class AccountSettingsFixture : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _testFixture;
        private readonly ITestOutputHelper _output;

        public AccountSettingsFixture(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testFixture = fixture;
            _output = output;
        }

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
                result.Error.Should().Be("This email is already used by another account.");
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
            }
        }

        [Fact]
        public async Task Should_Not_Change_The_Password_With_A_Wrong_Current_Password()
        {
            var account = await SaveMemberWithAccountAsync("wrongcurrent");

            var result = await SendAsync(new ChangePasswordCommand(account.ID, "wrong", "newpassword"));

            using (new AssertionScope())
            {
                result.Error.Should().Be("The current password is not valid.");
                (await LoadAsync(account.ID)).Password.HashMatches("password").Should().BeTrue();
            }
        }

        private async Task<Account> SaveMemberWithAccountAsync(string nickname)
        {
            var member = TestDataHelper.NewMember(nickname);
            var account = TestDataHelper.NewAccount(member);
            using var session = _testFixture.OpenSession(_output);
            await session.SaveAsync(member, TestContext.Current.CancellationToken);
            await session.SaveAsync(account, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
            return account;
        }

        private async Task<Result> SendAsync(ChangeEmailCommand command)
        {
            using var session = _testFixture.OpenSession(_output);
            return await new TestMediatorBuilder(session).Build().Send(command, TestContext.Current.CancellationToken);
        }

        private async Task<Result> SendAsync(ChangePasswordCommand command)
        {
            using var session = _testFixture.OpenSession(_output);
            return await new TestMediatorBuilder(session).Build().Send(command, TestContext.Current.CancellationToken);
        }

        private async Task<Result<Guid>> SendAsync(LogInCommand command)
        {
            using var session = _testFixture.OpenSession(_output);
            return await new TestMediatorBuilder(session).Build().Send(command, TestContext.Current.CancellationToken);
        }

        private async Task<Account> LoadAsync(AccountId accountID)
        {
            using var session = _testFixture.OpenSession(_output);
            return await session.GetAsync<Account>(accountID, TestContext.Current.CancellationToken);
        }
    }
}
