using System;
using FluentAssertions;
using InstaLike.Core.Domain;
using InstaLike.Core.Tests.Properties;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class PictureFixture
    {
        private readonly string _testPictureBase64 = Convert.ToBase64String(Resources.GrumpyCat);

        [Fact]
        public void Should_Return_Missing_Picture_From_Empty_Array()
        {
            Picture.Create(new byte[] { }).Value.Should().Be(Picture.MissingPicture);
        }

        [Fact]
        public void Should_Create_Picture_From_Byte_Array()
        {
            var bytes = Convert.FromBase64String(_testPictureBase64);

            Picture.Create(bytes).IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Should_Convert_From_Byte_Array()
        {
            var bytes = Convert.FromBase64String(_testPictureBase64);
            var sut = (Picture)bytes;

            sut.RawBytes.Should().BeEquivalentTo(bytes);
        }

        [Fact]
        public void Should_Convert_From_Base64_String()
        {
            var sut = (Picture)_testPictureBase64;
            var bytes = Convert.FromBase64String(_testPictureBase64);

            sut.RawBytes.Should().BeEquivalentTo(bytes);
        }

        [Fact]
        public void Pictures_From_Same_Bytes_Should_Be_Equal()
        {
            var bytes = Convert.FromBase64String(_testPictureBase64);
            var sut1 = Picture.Create(bytes).Value;
            var sut2 = Picture.Create(bytes).Value;

            sut1.Should().Be(sut2);
        }

        [Fact]
        public void Pictures_From_Same_Base64_String_Should_Be_Equal()
        {
            var sut1 = (Picture)_testPictureBase64;
            var sut2 = (Picture)_testPictureBase64;

            sut1.Should().Be(sut2);
        }
    }
}