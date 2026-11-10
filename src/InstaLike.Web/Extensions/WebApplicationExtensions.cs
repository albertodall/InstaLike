using System.Threading.Tasks;
using InstaLike.Web.Infrastructure;
using InstaLike.Web.Media;
using Microsoft.AspNetCore.Builder;

namespace InstaLike.Web.Extensions;

internal static class WebApplicationExtensions
{
    public static async Task EnsurePictureContainersExistAsync(this WebApplication app)
    {
        var blobServiceClient = AzureBlobStorageSettings.FromConfiguration(app.Configuration).CreateClient();

        await AzureBlobStorageProvider.EnsureContainersExistAsync(blobServiceClient, app.Lifetime.ApplicationStopping);
    }
}
