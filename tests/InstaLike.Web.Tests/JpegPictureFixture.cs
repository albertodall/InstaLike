using FluentAssertions;
using InstaLike.Web.Media;
using SkiaSharp;
using Xunit;

namespace InstaLike.Web.Tests
{
    public sealed class JpegPictureFixture
    {
        [Fact]
        public void Picture_Already_Fitting_In_The_Size_Should_Keep_Its_Size()
        {
            var jpeg = JpegPicture.ResizeTo(TestPicture.Content, 1000);

            using var picture = SKBitmap.Decode(jpeg.Value);
            (picture.Width, picture.Height).Should().Be((700, 933));
        }

        [Fact]
        public void Picture_Larger_Than_The_Size_Should_Be_Shrunk_Keeping_Its_Proportions()
        {
            var jpeg = JpegPicture.ResizeTo(TestPicture.Content, 300);

            using var picture = SKBitmap.Decode(jpeg.Value);
            (picture.Width, picture.Height).Should().Be((225, 300));
        }

        [Fact]
        public void Unsupported_Picture_Should_Return_A_Failure()
        {
            var jpeg = JpegPicture.ResizeTo([1, 2, 3], 300);

            jpeg.IsFailure.Should().BeTrue();
        }
    }
}
