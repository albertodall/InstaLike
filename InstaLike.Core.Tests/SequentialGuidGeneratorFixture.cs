using System;
using FluentAssertions;
using InstaLike.Core.Services;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class SequentialGuidGeneratorFixture
    {
        [Fact]
        public void Should_Generate_A_Sequential_Guid()
        {
            var sut = new SequentialGuidGenerator();
            sut.GetNextId().Should().NotBeEmpty();
        }

        [Fact]
        public void Guids_Should_Be_Sequential()
        {
            var guid1 = SequentialGuidGenerator.GetNextId(DateTime.UtcNow.Add(TimeSpan.FromSeconds(5)));
            var guid2 = SequentialGuidGenerator.GetNextId(DateTime.UtcNow);

            guid2.CompareTo(guid1).Should().BeGreaterThan(0);
        }
    }
}
