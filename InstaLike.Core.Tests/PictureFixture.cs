using System;
using FluentAssertions;
using InstaLike.Core.Domain;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class PictureFixture
    {
        private const string Test_Picture_Base64 = "/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAMCAgICAgMCAgIDAwMDBAYEBAQEBAgGBgUGCQgKCgkICQkKDA8MCgsOCwkJDRENDg8QEBEQCgwSExIQEw8QEBD/2wBDAQMDAwQDBAgEBAgQCwkLEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBD/wAARCAAoACgDASIAAhEBAxEB/8QAGQABAAMBAQAAAAAAAAAAAAAAAAYHCQUI/8QAMhAAAQIFAgIHBwUAAAAAAAAAAgMEAAUGBxIBEwhCERQhIjEykhUjJEFSYYIWJlFicv/EABQBAQAAAAAAAAAAAAAAAAAAAAD/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwDVOEIpC8/Efb2gZS+lzmZCs4WRNHJJbQccw5dYC0mtYUs+mvsNlUMuWmGBno0ByGq2Ia9/uePZHdjJFjfqXSu9cguZSskP9tmofUd5zsrZgaJmZmZmGef+P6RptaK6tO3lodlXNM7oNnWpoqoLad9usHnTP76dkBOIQhAVtxBVapRNpJ9O2r0G7jZBFEz8CzPTPT0ZxkPUlYPqzm7iavl1jNc8wz+jkjUfjJt7UtxLIzOV0e0VeTNgsEwTZI+d2AAYqJhpzngZ6gPz1HT7RnhWV0LC1Ta2RqVVrM2NyKRk7WlWzZueCDhBqfud5HDPeAPcwEVRraeIyv2MDpbqhog12jWPDADMwDD6AMzP847dGcQNY2llbhjI5ybNo6PMwByYAZ/hEZ1trxAKuuqMuG26BmZYgZ0w8APWYYRNpDau9thajpO9F4bDvCp+XPjUWaHsv8E8MPiQRz2c8zwM+cNP5gPdXALcCsblWZfzuuZ/pM3o1C5TbfHi5VRa4I4Apzh397oA+3DohFZcCD79Y3luRcS29Fv6btdMWiCDRJZDZRWfb2p6bAeGADu+TsDMNPnCA91xFHVsbbvqmCtX9AU25qJHTIJqrKkCej4eC2GfIPphCAlcIQgEIQgP/9k=";

        [Fact]
        public void Should_Not_Return_Picture_From_Empty_Array()
        {
            Picture.Create(new byte[] { }).IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Should_Create_Picture_From_Byte_Array()
        {
            var bytes = Convert.FromBase64String(Test_Picture_Base64);
            Picture.Create(bytes).IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Should_Convert_From_Byte_Array()
        {
            var bytes = Convert.FromBase64String(Test_Picture_Base64);
            var sut = (Picture)bytes;
            sut.RawBytes.Should().BeEquivalentTo(bytes);
        }

        [Fact]
        public void Should_Convert_From_Base64_String()
        {
            var sut = (Picture)Test_Picture_Base64;
            var bytes = Convert.FromBase64String(Test_Picture_Base64);
            sut.RawBytes.Should().BeEquivalentTo(bytes);
        }

        [Fact]
        public void Pictures_From_Same_Bytes_Should_Be_Equal()
        {
            var bytes = Convert.FromBase64String(Test_Picture_Base64);
            var sut1 = Picture.Create(bytes).Value;
            var sut2 = Picture.Create(bytes).Value;
            sut1.Should().Be(sut2);
        }

        [Fact]
        public void Pictures_From_Same_Base64_String_Should_Be_Equal()
        {
            var sut1 = (Picture)Test_Picture_Base64;
            var sut2 = (Picture)Test_Picture_Base64;
            sut1.Should().Be(sut2);
        }
    }
}
