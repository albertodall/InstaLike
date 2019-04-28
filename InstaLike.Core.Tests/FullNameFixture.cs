using FluentAssertions;
using InstaLike.Core.Domain;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class FullNameFixture
    {
        [Fact]
        public void Should_Create_Valid_FullName()
        {
            FullName.Create("first", "last").IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Should_Not_Allow_Empty_First_Name()
        {
            FullName.Create(string.Empty, "last").IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Should_Not_Allow_Empty_Last_Name()
        {
            FullName.Create("first", string.Empty).IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Should_Not_Allow_Empty_First_And_Last_Name()
        {
            FullName.Create(string.Empty, string.Empty).IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Should_Convert_String_to_FullName()
        {
            string fullname = "first last";
            var sut = (FullName)fullname;

            sut.ToString().Should().Be(fullname);
        }

        [Fact]
        public void Should_Convert_String_With_Spaces_to_FullName()
        {
            string fullname = "first   last";
            var sut = (FullName)fullname;

            sut.ToString().Should().Be("first last");
        }

        [Fact]
        public void Same_First_And_Last_Names_Should_Create_Same_FullName()
        {
            var first = "first";
            var last = "last;";
            var sut1 = FullName.Create(first, last).Value;
            var sut2 = FullName.Create(first, last).Value;

            sut1.Should().Be(sut2);
            sut1.GetHashCode().Equals(sut2.GetHashCode()).Should().BeTrue();
        }

        [Fact]
        public void Different_First_And_Last_Names_Should_Create_Different_FullNames()
        {
            var sut1 = FullName.Create("first1", "last1").Value;
            var sut2 = FullName.Create("first2", "last2").Value;

            sut1.Should().NotBe(sut2);
            sut1.GetHashCode().Equals(sut2.GetHashCode()).Should().BeFalse();
        }
    }
}
