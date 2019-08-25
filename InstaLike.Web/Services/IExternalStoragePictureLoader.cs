using System.Threading.Tasks;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Services
{
    internal interface IExternalStoragePictureLoader
    {
        Task<Picture> LoadPictureAsync(string blobFileName, string containerName);
    }
}