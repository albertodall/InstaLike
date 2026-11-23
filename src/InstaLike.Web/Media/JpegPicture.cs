using System;
using InstaLike.Core.SharedKernel;
using SkiaSharp;

namespace InstaLike.Web.Media
{
    /// <summary>
    /// Converts pictures to JPEG, the format every vision model accepts.
    /// </summary>
    internal static class JpegPicture
    {
        public const string MediaType = "image/jpeg";

        private const int QualityPercent = 85;

        /// <summary>
        /// Shrinks the picture, when needed, so that its longest side fits in the given size;
        /// smaller pictures keep their size. Transparent areas become white.
        /// </summary>
        public static Result<byte[]> ResizeTo(byte[] picture, int longestSide)
        {
            using var content = SKData.CreateCopy(picture);
            using var codec = SKCodec.Create(content);
            using var original = codec is null ? null : SKBitmap.Decode(codec);
            if (original is null)
            {
                return Result.Failure<byte[]>(Error.Validation("Picture.Unsupported", "The picture format is not supported."));
            }

            var scale = Math.Min(1d, (double)longestSide / Math.Max(original.Width, original.Height));
            var width = Math.Max(1, (int)Math.Round(original.Width * scale));
            var height = Math.Max(1, (int)Math.Round(original.Height * scale));

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            using var image = SKImage.FromBitmap(original);
            surface.Canvas.Clear(SKColors.White);
            surface.Canvas.DrawImage(
                image,
                SKRect.Create(width, height),
                new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));

            using var resized = surface.Snapshot();
            using var jpeg = resized.Encode(SKEncodedImageFormat.Jpeg, QualityPercent);
            return Result.Ok(jpeg.ToArray());
        }
    }
}
