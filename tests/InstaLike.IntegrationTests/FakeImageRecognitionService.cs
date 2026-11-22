using System.IO;
using System.Threading.Tasks;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Media;

namespace InstaLike.IntegrationTests
{
    /// <summary>
    /// Suggests the same tags for every picture, instead of calling a vision model, and keeps the last picture it tagged.
    /// </summary>
    internal sealed class FakeImageRecognitionService : IImageRecognitionService
    {
        internal static readonly string[] ImageTags = ["#cat", "#grumpy"];

        public byte[] TaggedPicture { get; private set; } = [];

        public async Task<Result<string[]>> AutoTagImage(Stream imageStream)
        {
            using var picture = new MemoryStream();
            await imageStream.CopyToAsync(picture);
            TaggedPicture = picture.ToArray();

            return Result.Ok(ImageTags);
        }
    }
}
