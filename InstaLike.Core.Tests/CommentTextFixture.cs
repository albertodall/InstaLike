using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Domain;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class CommentTextFixture
    {
        [Fact]
        public void Should_Create_A_Valid_Text_For_A_Comment()
        {
            CommentText.Create("this is my text").IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Empty_Comment_Text_Is_Not_Valid()
        {
            CommentText.Create(string.Empty).IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Comment_Text_From_String_Too_Long_Is_Not_Valid()
        {
            var sut = new string('x', 1000);

            CommentText.Create(sut).IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Should_Convert_String_To_Comment_Text()
        {
            var text = "this is my text";
            var sut = (CommentText)text;

            sut.Value.Should().Be(text);
        }

        [Fact]
        public void Should_Convert_Comment_Text_To_String()
        {
            var text = "this is my text";
            string sut = CommentText.Create(text).Value;

            sut.Should().Be(text);
        }

        [Fact]
        public void Same_String_Should_Create_The_Same_Comment_Text()
        {
            var text1 = CommentText.Create("this is my text").Value;
            var text2 = CommentText.Create("this is my text").Value;

            using (new AssertionScope())
            {
                text1.Should().Be(text2);
                text1.GetHashCode().Equals(text2.GetHashCode()).Should().BeTrue();
            }
        }

        [Fact]
        public void Different_Strings_Should_Create_Different_Comment_Texts()
        {
            var text1 = CommentText.Create("this is my text").Value;
            var text2 = CommentText.Create("this is my other text").Value;

            using (new AssertionScope())
            {
                text1.Should().NotBe(text2);
                text1.GetHashCode().Equals(text2.GetHashCode()).Should().BeFalse();
            }
        }
    }
}