using System.IO;
using System.Threading.Tasks;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Media;

namespace InstaLike.Web.EndToEndTests
{
    /// <summary>
    /// Suggests the same tags for every picture, instead of calling a vision model.
    /// </summary>
    internal sealed class FakeImageRecognitionService : IImageRecognitionService
    {
        internal static readonly string[] ImageTags = ["#cat", "#grumpy"];

        public Task<Result<string[]>> AutoTagImage(Stream imageStream)
        {
            return Task.FromResult(Result.Ok(ImageTags));
        }
    }
}
