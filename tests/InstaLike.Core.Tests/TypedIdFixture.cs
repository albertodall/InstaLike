using System;
using System.Data.SqlTypes;
using System.Linq;
using FluentAssertions;
using InstaLike.Core.Posts;
using Xunit;

namespace InstaLike.Core.Tests
{
    public sealed class TypedIdFixture
    {
        [Fact]
        public void New_Identifiers_Should_Be_Different()
        {
            PostId.New().Should().NotBe(PostId.New());
        }

        [Fact]
        public void New_Identifiers_Should_Follow_The_SQL_Server_Order()
        {
            var identifiers = Enumerable.Range(0, 100)
                .Select(_ => new SqlGuid(PostId.New().Value))
                .ToList();

            identifiers.Should().BeInAscendingOrder();
        }

        [Fact]
        public void Identifiers_With_The_Same_Value_Should_Be_Equal()
        {
            var value = Guid.NewGuid();

            PostId.From(value).Should().Be(PostId.From(value));
        }

        [Fact]
        public void An_Identifier_Should_Be_Written_As_Its_Guid()
        {
            var value = Guid.NewGuid();

            PostId.From(value).ToString().Should().Be(value.ToString());
        }

        [Fact]
        public void An_Identifier_Should_Be_Parsed_From_Its_Text()
        {
            var post = PostId.New();

            PostId.TryParse(post.ToString(), null, out var parsed).Should().BeTrue();
            parsed.Should().Be(post);
        }

        [Fact]
        public void Text_That_Is_Not_A_Guid_Should_Not_Be_Parsed_As_An_Identifier()
        {
            PostId.TryParse("not a post", null, out var parsed).Should().BeFalse();
            parsed.Should().BeNull();
        }
    }
}
