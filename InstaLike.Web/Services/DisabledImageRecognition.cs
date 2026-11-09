using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace InstaLike.Web.Services;

internal sealed class DisabledImageRecognition : IImageRecognitionService
{
    public Task<Result<string[]>> AutoTagImage(Stream imageStream, string mediaType)
    {
        return Task.FromResult(Result.Failure<string[]>("Automatic tagging is not configured."));
    }
}
