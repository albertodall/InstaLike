using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Web.Media;

/// <summary>
/// Stores pictures in Azure Blob Storage.
/// </summary>
internal sealed class AzureBlobPictureStore : IPictureStore
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobPictureStore(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
    }

    public async Task<PictureId> SaveAsync(string container, byte[] content, CancellationToken cancellationToken)
    {
        if (content.Length == 0)
        {
            throw new ArgumentException("A picture cannot be empty.", nameof(content));
        }

        var picture = PictureId.New();
        await GetBlob(container, picture).UploadAsync(
            BinaryData.FromBytes(content),
            new BlobUploadOptions { Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All } },
            cancellationToken);

        return picture;
    }

    public async Task<Stream?> OpenReadAsync(string container, PictureId picture, CancellationToken cancellationToken)
    {
        try
        {
            var download = await GetBlob(container, picture).DownloadStreamingAsync(cancellationToken: cancellationToken);
            return download.Value.Content;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
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

    private BlobClient GetBlob(string container, PictureId picture) =>
        _blobServiceClient
            .GetBlobContainerClient(container)
            .GetBlobClient($"{picture.Value}.jpg");
}
