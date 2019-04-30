using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace InstaLike.Web.Services
{
    public interface IImageRecognitionService
    {
        Task<Result<string[]>> AutoTagImage(Stream imageStream);
    }
}
