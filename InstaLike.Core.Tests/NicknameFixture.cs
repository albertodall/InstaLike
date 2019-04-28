using FluentAssertions;
using InstaLike.Core.Domain;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class NicknameFixture
    {
        [Fact]
        public void Should_Create_A_Valid_Nickname()
        {
            Nickname.Create("nick").IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Nickname_From_Empty_String_Is_Not_Valid()
        {
            Nickname.Create(string.Empty).IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Nickname_From_String_Too_Long_Is_Not_Valid()
        {
            var sut = new string('x', 42);

            Nickname.Create(sut).IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Should_Convert_String_To_Nickname()
        {
            var nick = "nick";
            var sut = (Nickname)nick;

            sut.Value.Should().Be(nick);
        }

        [Fact]
        public void Should_Convert_Nickname_To_String()
        {
            var nick = "nick";
            string sut = Nickname.Create(nick).Value;

            sut.Should().Be(nick);
        }

        [Fact]
        public void Same_String_Should_Create_The_Same_Nickname()
        {
            var nick1 = Nickname.Create("nick").Value;
            var nick2 = Nickname.Create("nick").Value;

            nick1.Should().Be(nick2);
            nick1.GetHashCode().Equals(nick2.GetHashCode()).Should().BeTrue();
        }

        [Fact]
        public void Different_Strings_Should_Create_Different_Nicknames()
        {
            var nick1 = Nickname.Create("nick1").Value;
            var nick2 = Nickname.Create("nick2").Value;

            nick1.Should().NotBe(nick2);
            nick1.GetHashCode().Equals(nick2.GetHashCode()).Should().BeFalse();
        }
    }
}