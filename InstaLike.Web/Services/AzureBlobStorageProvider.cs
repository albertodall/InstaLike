using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace InstaLike.Web.Services
{
    /// <summary>
    /// Stores and gets all pictures from an Azure Blob Storage container.
    /// </summary>
    internal class AzureBlobStorageProvider : IExternalStorageProvider
    {
        private const string GuidRegex = 
            @"([0-9a-fA-F]{8})\-([0-9a-fA-F]{4})\-([0-9a-fA-F]{4})\-([0-9a-fA-F]{4})\-([0-9a-fA-F]{12})";

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
            string blobFileGuid = string.Empty;
            var downloadBlobResult = await LoadPictureFromContainerAsync(blobFileName, containerName);
            
            if (Regex.IsMatch(blobFileName, GuidRegex))
            {
                blobFileGuid = Regex.Match(blobFileName, GuidRegex).Value;
            }

            if (downloadBlobResult.IsFailure)
            {
                return Picture.MissingPicture;
            }

            return string.IsNullOrEmpty(blobFileGuid) ?
                Picture.Create(downloadBlobResult.Value).Value :
                Picture.Create(downloadBlobResult.Value, new Guid(blobFileGuid)).Value;
        }

        public async Task SavePictureAsync(Picture picture, string containerName)
        {
            await SavePictureToContainerAsync(picture.RawBytes, $"{picture.Identifier}.jpg", containerName);
        }

        private async Task<Result<byte[]>> LoadPictureFromContainerAsync(string blobName, string containerName)
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

        private async Task SavePictureToContainerAsync(byte[] byteArray, string blobName, string containerName)
        {
            var container = _client.GetContainerReference(containerName);
            if (!await container.ExistsAsync())
            {
                throw new StorageException($"Container {containerName} does not exist.");
            }

            var blob = container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = "image/jpeg";
            await blob.UploadFromByteArrayAsync(byteArray, 0, byteArray.Length);
        }
    }
}
