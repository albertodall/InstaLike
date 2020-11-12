using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;
using InstaLike.Web.Extensions;

namespace InstaLike.Web.Services
{
    /// <summary>
    /// Stores and gets all pictures from an Azure Blob Storage container.
    /// </summary>
    internal class AzureBlobStorageProvider : IExternalStorageProvider
    {
        private readonly string _storageConnectionString;

        public AzureBlobStorageProvider(string storageConnectionString)
        {
            if (string.IsNullOrEmpty(storageConnectionString))
            {
                throw new ArgumentNullException(nameof(storageConnectionString));
            }

            _storageConnectionString = storageConnectionString;
        }

        public async Task<Picture> LoadPictureAsync(string blobFileName, string containerName)
        {
            var downloadBlobResult = await DownloadPictureFromContainerAsync(blobFileName, containerName);
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

        private async Task<Result<byte[]>> DownloadPictureFromContainerAsync(string blobName, string containerName)
        {
            var container = new BlobContainerClient(_storageConnectionString, containerName);
            
            await EnsureBlobContainerExists(container);

            var blob = container.GetBlobClient(blobName);
            if (!await blob.ExistsAsync())
            {
                return Result.Failure<byte[]>($"Blob {blobName} does not exist in container {containerName}.");
            }

            await using (var downloadStream = new MemoryStream())
            {
                await blob.DownloadToAsync(downloadStream);
                return Result.Success(downloadStream.ToArray());
            }
        }

        private async Task SavePictureToContainerAsync(byte[] byteArray, string blobName, string containerName)
        {
            var container = new BlobContainerClient(_storageConnectionString, containerName);

            await EnsureBlobContainerExists(container);

            var blob = container.GetBlobClient(blobName);
            await using (var stream = new MemoryStream(byteArray, false))
            {
                await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = "image/jpg" });
            }
        }

        private static async Task EnsureBlobContainerExists(BlobContainerClient container)
        {
            if (!await container.ExistsAsync())
            {
                throw new InvalidOperationException($"Container {container.Name} does not exist.");
            }
        }
    }
}
