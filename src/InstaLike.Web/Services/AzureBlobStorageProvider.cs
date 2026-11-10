using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Services;

/// <summary>
/// Stores and gets all pictures from Azure Blob Storage containers.
/// </summary>
internal class AzureBlobStorageProvider : IExternalStorageProvider
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageProvider(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public Picture LoadPicture(Guid identifier, string containerName)
    {
        var blob = _blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(GetBlobName(identifier));

        try
        {
            var content = blob.DownloadContent();
            return Picture.Create(content.Value.Content.ToArray(), identifier).Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return Picture.MissingPicture;
        }
    }

    public void SavePicture(Picture picture, string containerName)
    {
        var blob = _blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(GetBlobName(picture.Identifier));

        blob.Upload(
            BinaryData.FromBytes(picture.RawBytes),
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = "image/jpeg" } });
    }

    public static async Task EnsureContainersExistAsync(BlobServiceClient blobServiceClient, CancellationToken cancellationToken = default)
    {
        foreach (var containerName in PictureContainers.All)
        {
            await blobServiceClient
                .GetBlobContainerClient(containerName)
                .CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        }
    }

    private static string GetBlobName(Guid identifier) => $"{identifier.ToString().ToLowerInvariant()}.jpg";
}
