using System.Threading.Tasks;
using Azure.Storage.Blobs;
using InstaLike.Web.Media;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace InstaLike.Web.Extensions;

internal static class WebApplicationExtensions
{
    public static async Task EnsurePictureContainersExistAsync(this WebApplication app)
    {
        var blobServiceClient = app.Services.GetRequiredService<BlobServiceClient>();

        await AzureBlobPictureStore.EnsureContainersExistAsync(blobServiceClient, app.Lifetime.ApplicationStopping);
    }
}
