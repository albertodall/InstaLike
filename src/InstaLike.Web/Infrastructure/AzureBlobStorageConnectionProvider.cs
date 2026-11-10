using System;
using System.Collections.Generic;
using InstaLike.Web.Media;

namespace InstaLike.Web.Infrastructure;

internal class AzureBlobStorageConnectionProvider : IHybridStorageConnectionProvider
{
    private Lazy<IExternalStorageProvider>? _provider;

    public void Configure(IDictionary<string, string> settings)
    {
        settings.TryGetValue(ExternalStorageParameters.ConnectionStringProperty, out string? connectionString);
        settings.TryGetValue(ExternalStorageParameters.ServiceUriProperty, out string? serviceUri);

        var storageSettings = new AzureBlobStorageSettings(connectionString, serviceUri);
        _provider = new Lazy<IExternalStorageProvider>(
            () => new AzureBlobStorageProvider(storageSettings.CreateClient()));
    }

    public IExternalStorageProvider GetProvider()
    {
        if (_provider is null)
        {
            throw new InvalidOperationException("External storage provider has not been configured correctly.");
        }

        return _provider.Value;
    }
}
