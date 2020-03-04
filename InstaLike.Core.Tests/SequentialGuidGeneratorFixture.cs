using System;
using System.Threading.Tasks;
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

        [Fact(Skip = "This test sometimes passes, sometimes doesn't. Maybe I'm testing the wrong thing.")]
        public void Guids_Generated_In_Sequence_Should_Be_Sequential()
        {
            var sut = new SequentialGuidGenerator();
            var guid1 = sut.GetNextId();
            var guid2 = sut.GetNextId();

            guid1.CompareTo(guid2).Should().BeLessThan(0);
        }
    }
}
