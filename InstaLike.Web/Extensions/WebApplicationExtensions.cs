using System;
using System.Threading.Tasks;
using InstaLike.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace InstaLike.Web.Extensions
{
    internal static class WebApplicationExtensions
    {
        public static async Task EnsurePictureContainersExistAsync(this WebApplication app)
        {
            var storageConnectionString = app.Configuration.GetValue<string>("ExternalStorage:AzureBlobStorage:ConnectionString")
                ?? throw new InvalidOperationException("Setting 'ExternalStorage:AzureBlobStorage:ConnectionString' has not been configured.");

            await AzureBlobStorageProvider.EnsureContainersExistAsync(storageConnectionString, app.Lifetime.ApplicationStopping);
        }
    }
}
