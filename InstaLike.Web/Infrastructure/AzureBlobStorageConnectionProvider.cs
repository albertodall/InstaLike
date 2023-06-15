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
            if (_provider is null)
            {
                throw new InvalidOperationException("External storage provider has not been configured correctly.");
            }

            return _provider?.Value;
        }
    }
}