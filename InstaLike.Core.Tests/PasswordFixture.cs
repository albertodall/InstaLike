using FluentAssertions;
using InstaLike.Core.Domain;
using System;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class PasswordFixture
    {
        [Fact]
        public void Should_Create_Valid_Password()
        {
            var sut = Password.Create("abcd1234efgh5678");

            sut.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Should_Not_Create_A_Valid_Password_If_Too_Short()
        {
            var sut = Password.Create("ab12");

            sut.IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Empty_Password_Should_Not_Be_Valid()
        {
            var sut = Password.Create(string.Empty);

            sut.IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Should_Match_Password_Hash()
        {
            var sut = Password.Create("password123").Value;

            sut.HashMatches("password123").Should().BeTrue();
        }

        [Fact]
        public void Should_Not_Match_Password_Hash()
        {
            var sut = Password.Create("password123").Value;

            sut.HashMatches("password").Should().BeFalse();
        }

        [Fact]
        public void Should_Not_Convert_A_Non_Base64_String_To_Password()
        {
            string sut = "xyz123$$";

            Password ConvertStringToPassword(string x) => (Password)x;

            sut.Invoking(x => ConvertStringToPassword(x))
                .Should()
                    .Throw<InvalidCastException>()
                    .WithMessage("The specified password is not a valid base64 string");
        }
    }
}