using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;
using InstaLike.Web.Extensions;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace InstaLike.Web.Services
{
    /// <summary>
    /// Stores and gets all pictures from an Azure Blob Storage container.
    /// </summary>
    internal class AzureBlobStorageProvider : IExternalStorageProvider
    {
        private readonly CloudBlobClient _client;

        public AzureBlobStorageProvider(string storageConnectionString)
        {
            if (string.IsNullOrEmpty(storageConnectionString))
            {
                throw new ArgumentNullException(nameof(storageConnectionString));
            }

            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            _client = storageAccount.CreateCloudBlobClient();
        }

        public async Task<Picture> LoadPictureAsync(string blobFileName, string containerName)
        {
            var downloadBlobResult = await LoadPictureFromContainerAsync(blobFileName, containerName);
            if (downloadBlobResult.IsFailure || downloadBlobResult.Value == Array.Empty<byte>())
            {
                return Picture.MissingPicture;
            }

            return Picture.Create(downloadBlobResult.Value, new Guid(blobFileName.ExtractGuid())).Value;
        }

        public async Task SavePictureAsync(Picture picture, string containerName)
        {
            await SavePictureToContainerAsync(picture.RawBytes, $"{picture.Identifier}.jpg", containerName);
        }

        private async Task<Result<byte[]>> LoadPictureFromContainerAsync(string blobName, string containerName)
        {
            var container = _client.GetContainerReference(containerName);

            await EnsureBlobContainerExists(container);

            var blob = container.GetBlobReference(blobName);
            if (!await blob.ExistsAsync())
            {
                return Result.Failure<byte[]>($"Blob {blobName} does not exist in container {containerName}.");
            }

            await using (var downloadStream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(downloadStream);
                return Result.Success(downloadStream.ToArray());
            }
        }

        private async Task SavePictureToContainerAsync(byte[] byteArray, string blobName, string containerName)
        {
            var container = _client.GetContainerReference(containerName);

            await EnsureBlobContainerExists(container);

            var blob = container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = "image/jpeg";
            await blob.UploadFromByteArrayAsync(byteArray, 0, byteArray.Length);
        }

        private static async Task EnsureBlobContainerExists(CloudBlobContainer container)
        {
            if (!await container.ExistsAsync())
            {
                throw new StorageException($"Container {container.Name} does not exist.");
            }
        }
    }
}
