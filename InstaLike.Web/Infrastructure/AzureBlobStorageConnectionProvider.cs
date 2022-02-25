using System;
using System.Collections.Generic;
using InstaLike.Web.Services;

namespace InstaLike.Web.Infrastructure
{
    internal class AzureBlobStorageConnectionProvider : IHybridStorageConnectionProvider
    {
        private Lazy<IExternalStorageProvider>? _provider;

        public void Configure(IDictionary<string, string> settings)
        {
            if (settings.TryGetValue(ExternalStorageParameters.ConnectionStringProperty, out string? externalStorageConnectionString))
            {
                _provider = new Lazy<IExternalStorageProvider>(
                    () => new AzureBlobStorageProvider(externalStorageConnectionString));
            }
        }

        public IExternalStorageProvider? GetProvider()
        {
            return _provider?.Value;
        }
    }
}