using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace InstaLike.Web.Services
{
    public interface IImageRecognitionService
    {
        Task<string[]> AutoTagImage(Stream imageStream);
    }
}
