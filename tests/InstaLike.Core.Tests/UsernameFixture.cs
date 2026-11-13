using FluentAssertions;
using InstaLike.Core.Accounts;
using Xunit;

namespace InstaLike.Core.Tests;

public sealed class UsernameFixture
{
    [Fact]
    public void Should_Create_A_Valid_Username()
    {
        Username.Create("user").IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Username_From_Empty_String_Is_Not_Valid()
    {
        Username.Create(string.Empty).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Username_From_String_Too_Long_Is_Not_Valid()
    {
        var sut = new string('x', 42);

        Username.Create(sut).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Should_Convert_String_To_Username()
    {
        var name = "user";
        var sut = (Username)name;

        sut.Value.Should().Be(name);
    }

    [Fact]
    public void Should_Convert_Username_To_String()
    {
        var name = "user";
        string sut = Username.Create(name).Value;

        sut.Should().Be(name);
    }

    [Fact]
    public void Same_String_Should_Create_The_Same_Username()
    {
        var name1 = Username.Create("user").Value;
        var name2 = Username.Create("user").Value;

        name1.Should().Be(name2);
        name1.GetHashCode().Equals(name2.GetHashCode()).Should().BeTrue();
    }

    [Fact]
    public void Different_Strings_Should_Create_Different_Usernames()
    {
        var name1 = Username.Create("name1").Value;
        var name2 = Username.Create("name2").Value;

        name1.Should().NotBe(name2);
        name1.GetHashCode().Equals(name2.GetHashCode()).Should().BeFalse();
    }
}
