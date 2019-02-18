using FluentAssertions;
using InstaLike.Core.Domain;
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
    }
}
