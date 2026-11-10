using System;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace InstaLike.Web.Infrastructure;

/// <summary>
/// Connects with a connection string (Azurite) or with the service URI and the application's Azure identity.
/// </summary>
internal sealed record AzureBlobStorageSettings(string? ConnectionString, string? ServiceUri)
{
    public const string SectionName = "ExternalStorage:AzureBlobStorage";

    public static AzureBlobStorageSettings FromConfiguration(IConfiguration configuration) =>
        configuration.GetSection(SectionName).Get<AzureBlobStorageSettings>() ?? new AzureBlobStorageSettings(null, null);

    public BlobServiceClient CreateClient()
    {
        if (!string.IsNullOrEmpty(ConnectionString))
        {
            return new BlobServiceClient(ConnectionString);
        }

        if (!string.IsNullOrEmpty(ServiceUri))
        {
            return new BlobServiceClient(new Uri(ServiceUri), AzureIdentity.Credential);
        }

        throw new InvalidOperationException($"Set '{SectionName}:ServiceUri' or '{SectionName}:ConnectionString'.");
    }
}
