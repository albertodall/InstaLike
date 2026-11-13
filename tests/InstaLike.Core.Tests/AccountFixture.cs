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
            }
        }

        [Fact]
        public void Should_Not_Change_The_Password_When_The_Current_One_Does_Not_Match()
        {
            var sut = CreateAccountWithPassword("password1");

            using (new AssertionScope())
            {
                sut.ChangePassword("wrong", Password.Create("password2").Value).IsFailure.Should().BeTrue();
                sut.Password.HashMatches("password1").Should().BeTrue();
            }
        }

        [Fact]
        public void Should_Change_The_Email_Address()
        {
            var sut = CreateAccountWithPassword("password");

            sut.ChangeEmailAddress((Email)"user1@newdomain.com");

            sut.Email.Value.Should().Be("user1@newdomain.com");
        }

        private static Account CreateAccountWithPassword(string password)
        {
            return Account.Create(Guid.NewGuid(), (Username)"user1", Password.Create(password).Value, (Email)"user1@acme.com");
        }
    }
}
