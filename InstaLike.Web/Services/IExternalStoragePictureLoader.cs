using System.Threading.Tasks;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Services
{
    internal interface IExternalStoragePictureLoader
    {
        Task<Picture> LoadUserProfilePictureAsync(User user, string containerName);
        Task<Picture> LoadPostPictureAsync(Post post, string containerName);
    }
}