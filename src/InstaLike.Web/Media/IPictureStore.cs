using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Web.Media;

public interface IPictureStore
{
    Task<PictureId> SaveAsync(string container, byte[] content, CancellationToken cancellationToken);

    Task<Stream?> OpenReadAsync(string container, PictureId picture, CancellationToken cancellationToken);
}
