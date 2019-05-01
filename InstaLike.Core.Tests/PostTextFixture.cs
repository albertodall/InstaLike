using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Domain;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class PostTextFixture
    {
        [Fact]
        public void Should_Create_A_Valid_Text_For_A_Post()
        {
            PostText.Create("this is my text").IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Empty_Post_Text_Is_Not_Valid()
        {
            PostText.Create(string.Empty).IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Post_Text_From_String_Too_Long_Is_Not_Valid()
        {
            var sut = new string('x', 1000);

            PostText.Create(sut).IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Should_Convert_String_To_Post_Text()
        {
            var text = "this is my text";
            var sut = (PostText)text;

            sut.Value.Should().Be(text);
        }

        [Fact]
        public void Should_Convert_Post_Text_To_String()
        {
            var text = "this is my text";
            string sut = PostText.Create(text).Value;

            sut.Should().Be(text);
        }

        [Fact]
        public void Same_String_Should_Create_The_Same_Post_Text()
        {
            var text1 = PostText.Create("this is my text").Value;
            var text2 = PostText.Create("this is my text").Value;

            using (new AssertionScope())
            {
                text1.Should().Be(text2);
                text1.GetHashCode().Equals(text2.GetHashCode()).Should().BeTrue();
            }
        }

        [Fact]
        public void Different_Strings_Should_Create_Different_Post_Texts()
        {
            var text1 = PostText.Create("this is my text").Value;
            var text2 = PostText.Create("this is my other text").Value;

            using (new AssertionScope())
            {
                text1.Should().NotBe(text2);
                text1.GetHashCode().Equals(text2.GetHashCode()).Should().BeFalse();
            }
        }
    }
}