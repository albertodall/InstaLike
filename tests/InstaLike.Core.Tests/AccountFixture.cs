using System;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using Xunit;

namespace InstaLike.Core.Tests
{
    public sealed class AccountFixture
    {
        [Fact]
        public void An_Account_Should_Share_The_Identifier_Of_Its_Member()
        {
            var memberID = Guid.NewGuid();

            var sut = Account.Create(memberID, (Username)"user1", Password.Create("password").Value, (Email)"user1@acme.com");

            using (new AssertionScope())
            {
                sut.ID.Should().Be(AccountId.From(memberID));
                sut.DomainEvents.Should().BeEmpty();
            }
        }

        [Fact]
        public void Logging_In_With_The_Right_Password_Should_Raise_An_Event()
        {
            var sut = CreateAccountWithPassword("password");

            using (new AssertionScope())
            {
                sut.LogIn("password").IsSuccess.Should().BeTrue();
                sut.DomainEvents.Should().ContainSingle().Which.Should().Be(new AccountLoggedInEvent(sut.ID));
            }
        }

        [Fact]
        public void Logging_In_With_A_Wrong_Password_Should_Fail_Without_Events()
        {
            var sut = CreateAccountWithPassword("password");

            using (new AssertionScope())
            {
                sut.LogIn("wrong").IsSuccess.Should().BeFalse();
                sut.DomainEvents.Should().BeEmpty();
            }
        }

        [Fact]
        public void Five_Failed_Logins_Should_Lock_The_Account()
        {
            var sut = CreateAccountWithPassword("password");

            LogInWithWrongPassword(sut, 5);

            using (new AssertionScope())
            {
                sut.LockedOutUntil.Should().BeAfter(DateTimeOffset.UtcNow);
                sut.FailedLogInAttempts.Should().Be(0);
                sut.DomainEvents.Should().ContainSingle().Which.Should().Be(new AccountLockedOutEvent(sut.ID));
            }
        }

        [Fact]
        public void Four_Failed_Logins_Should_Not_Lock_The_Account()
        {
            var sut = CreateAccountWithPassword("password");

            LogInWithWrongPassword(sut, 4);

            using (new AssertionScope())
            {
                sut.LockedOutUntil.Should().BeNull();
                sut.FailedLogInAttempts.Should().Be(4);
                sut.DomainEvents.Should().BeEmpty();
            }
        }

        [Fact]
        public void Locked_Out_Account_Should_Refuse_Even_The_Right_Password()
        {
            var sut = CreateAccountWithPassword("password");
            LogInWithWrongPassword(sut, 5);
            var lockedOutUntil = sut.LockedOutUntil;

            using (new AssertionScope())
            {
                sut.LogIn("password").Error.Should().Be(AccountErrors.LockedOut);
                sut.LogIn("wrong").Error.Should().Be(AccountErrors.LockedOut);
                sut.FailedLogInAttempts.Should().Be(0);
                sut.LockedOutUntil.Should().Be(lockedOutUntil);
            }
        }

        [Fact]
        public void Successful_Login_Should_Reset_The_Failed_Logins()
        {
            var sut = CreateAccountWithPassword("password");
            LogInWithWrongPassword(sut, 4);

            sut.LogIn("password");
            LogInWithWrongPassword(sut, 4);

            using (new AssertionScope())
            {
                sut.LockedOutUntil.Should().BeNull();
                sut.FailedLogInAttempts.Should().Be(4);
            }
        }

        [Fact]
        public void Logging_Out_Should_Raise_An_Event()
        {
            var sut = CreateAccountWithPassword("password");

            sut.LogOut();

            sut.DomainEvents.Should().ContainSingle().Which.Should().Be(new AccountLoggedOutEvent(sut.ID));
        }

        [Fact]
        public void Should_Change_The_Password_When_The_Current_One_Matches()
        {
            var sut = CreateAccountWithPassword("password1");

            using (new AssertionScope())
            {
                sut.ChangePassword("password1", Password.Create("password2").Value).IsSuccess.Should().BeTrue();
                sut.Password.HashMatches("password2").Should().BeTrue();
                sut.Password.HashMatches("password1").Should().BeFalse();
                sut.DomainEvents.Should().ContainSingle().Which.Should().Be(new AccountPasswordChangedEvent(sut.ID));
            }
        }

        [Fact]
        public void Should_Change_The_Security_Stamp_With_The_Password()
        {
            var sut = CreateAccountWithPassword("password1");
            var securityStamp = sut.SecurityStamp;

            sut.ChangePassword("password1", Password.Create("password2").Value);

            sut.SecurityStamp.Should().NotBe(securityStamp);
        }

        [Fact]
        public void Should_Not_Change_The_Password_When_The_Current_One_Does_Not_Match()
        {
            var sut = CreateAccountWithPassword("password1");
            var securityStamp = sut.SecurityStamp;

            using (new AssertionScope())
            {
                sut.ChangePassword("wrong", Password.Create("password2").Value).IsFailure.Should().BeTrue();
                sut.Password.HashMatches("password1").Should().BeTrue();
                sut.SecurityStamp.Should().Be(securityStamp);
                sut.DomainEvents.Should().BeEmpty();
            }
        }

        [Fact]
        public void Should_Change_The_Email_Address()
        {
            var sut = CreateAccountWithPassword("password");

            sut.ChangeEmailAddress((Email)"user1@newdomain.com");

            sut.Email.Value.Should().Be("user1@newdomain.com");
        }

        private static void LogInWithWrongPassword(Account account, int times)
        {
            for (var attempt = 0; attempt < times; attempt++)
            {
                account.LogIn("wrong");
            }
        }

        /// <summary>
        /// An account for user1 with the given password.
        /// </summary>
        private static Account CreateAccountWithPassword(string password)
        {
            return Account.Create(Guid.NewGuid(), (Username)"user1", Password.Create(password).Value, (Email)"user1@acme.com");
        }
    }
}
