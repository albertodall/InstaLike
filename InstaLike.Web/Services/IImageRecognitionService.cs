using CSharpFunctionalExtensions;
using System.IO;
using System.Threading.Tasks;

namespace InstaLike.Web.Services
{
    public interface IImageRecognitionService
    {
        Task<Result<string[]>> AutoTagImage(Stream imageStream);
    }
}
