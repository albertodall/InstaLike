using System.Collections.Generic;
using InstaLike.Web.Services;

namespace InstaLike.Web.Infrastructure
{
    internal class AzureBlobStorageConnectionProvider : IExternalStorageConnectionProvider
    {
        private readonly string _storageConnectionString;

        public AzureBlobStorageConnectionProvider(string storageConnectionString)
        {
            _storageConnectionString = storageConnectionString;
        }

        public void Configure(IDictionary<string, string> settings)
        {
            // TODO: Set _storageConnectionString using the "settings" parameter.
        }

        public IExternalStoragePictureProvider GetProvider()
        {
            return new AzureBlobStoragePictureProvider(_storageConnectionString);
        }
    }
}