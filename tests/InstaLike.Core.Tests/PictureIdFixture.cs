using System;
using FluentAssertions;
using InstaLike.Core.SharedKernel;
using Xunit;

namespace InstaLike.Core.Tests;

public class PictureIdFixture
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
}
