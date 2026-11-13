using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace InstaLike.Web.Media
{
    public interface IImageRecognitionService
    {
        Task<Result<string[]>> AutoTagImage(Stream imageStream, string mediaType);
    }
}
