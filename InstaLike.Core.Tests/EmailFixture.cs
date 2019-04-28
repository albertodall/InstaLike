using FluentAssertions;
using InstaLike.Core.Domain;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class EmailFixture
    {
        [Fact]
        public void Should_Create_A_Valid_Email()
        {
            Email.Create("my@email.it").IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Creating_Empty_Email_Should_Fail()
        {
            Email.Create(string.Empty).IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Should_Not_Allow_Invalid_Email_Strings()
        {
            Email.Create("abcdefg").IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Should_Convert_A_String_To_Email()
        {
            var email = "my@email.it";

            var sut = (Email)email;

            sut.Value.Should().Be(email);
        }

        [Fact]
        public void Should_Convert_An_Email_To_A_String()
        {
            var emailAddress = "my@email.it";

            string email = Email.Create(emailAddress).Value;

            email.Should().Be(emailAddress);
        }

        [Fact]
        public void Email_With_The_Same_Address_Should_Be_Equal()
        {
            var sut1 = Email.Create("user1@acme.com").Value;
            var sut2 = Email.Create("user1@acme.com").Value;

            sut1.Should().Be(sut2);
            sut1.GetHashCode().Equals(sut2.GetHashCode()).Should().BeTrue();
        }

        [Fact]
        public void Emails_With_Different_Addresses_Should_Be_Different()
        {
            var sut1 = Email.Create("user1@acme.com").Value;
            var sut2 = Email.Create("user2@acme.com").Value;

            sut1.Equals(sut2).Should().BeFalse();
            sut1.GetHashCode().Equals(sut2.GetHashCode()).Should().BeFalse();
        }
    }
}