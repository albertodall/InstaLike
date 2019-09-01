using System.Collections.Generic;
using InstaLike.Web.Services;

namespace InstaLike.Web.Infrastructure
{
    internal class AzureBlobStorageConnectionProvider : IExternalStorageConnectionProvider
    {
        private string _storageConnectionString;

        public void Configure(IDictionary<string, string> settings)
        {
            if (settings.TryGetValue(ExternalStorageParameters.ConnectionStringProperty, out string externalStorageConnectionString))
            {
                _storageConnectionString = externalStorageConnectionString;
            }
        }

        public IExternalStorageProvider GetProvider()
        {
            return new AzureBlobStorageProvider(_storageConnectionString);
        }
    }
}