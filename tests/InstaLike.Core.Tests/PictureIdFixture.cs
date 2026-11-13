using System;
using FluentAssertions;
using InstaLike.Core.SharedKernel;
using Xunit;

namespace InstaLike.Core.Tests;

public sealed class PictureIdFixture
{
    [Fact]
    public void New_Pictures_Should_Have_Different_Identifiers()
    {
        PictureId.New().Should().NotBe(PictureId.New());
    }

    [Fact]
    public void Pictures_With_The_Same_Value_Should_Be_Equal()
    {
        var value = Guid.NewGuid();

        new PictureId(value).Should().Be(new PictureId(value));
    }

    [Fact]
    public void A_Picture_Identifier_Should_Be_Written_As_Its_Guid()
    {
        var value = Guid.NewGuid();

        new PictureId(value).ToString().Should().Be(value.ToString());
    }

    [Fact]
    public void A_Picture_Identifier_Should_Be_Parsed_From_Its_Text()
    {
        var picture = PictureId.New();

        PictureId.TryParse(picture.ToString(), null, out var parsed).Should().BeTrue();
        parsed.Should().Be(picture);
    }

    [Fact]
    public void Text_That_Is_Not_A_Guid_Should_Not_Be_Parsed_As_A_Picture_Identifier()
    {
        PictureId.TryParse("not a picture", null, out var parsed).Should().BeFalse();
        parsed.Should().BeNull();
    }
}
