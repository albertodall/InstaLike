using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace InstaLike.Web.Services
{
    internal class AzureBlobStoragePictureLoader : IExternalStoragePictureLoader
    {
        private readonly string _storageConnectionString;
        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudBlobClient _client;

        public AzureBlobStoragePictureLoader(string storageConnectionString)
        {
            if (string.IsNullOrEmpty(storageConnectionString))
            {
                throw new ArgumentNullException(nameof(storageConnectionString));
            }
            _storageConnectionString = storageConnectionString;

            _storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            _client = _storageAccount.CreateCloudBlobClient();
        }

        public async Task<Picture> LoadPictureAsync(string blobFileName, string containerName)
        {
            var downloadBlobResult = await LoadPictureFromContainer(blobFileName, containerName);
            return downloadBlobResult.IsSuccess ?
                Picture.Create(downloadBlobResult.Value).Value :
                Picture.MissingPicture;
        }

        private async Task<Result<byte[]>> LoadPictureFromContainer(string blobName, string containerName)
        {
            var container = _client.GetContainerReference(containerName);
            if (!await container.ExistsAsync())
            {
                throw new StorageException($"Container {containerName} does not exist.");
            }

            var blob = container.GetBlobReference(blobName);
            if (!await blob.ExistsAsync())
            {
                return Result.Fail<byte[]>($"Blob {blobName} does not exist in container {containerName}.");
            }

            using (var downloadStream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(downloadStream);
                return Result.Ok(downloadStream.ToArray());
            }
        }
    }
}
