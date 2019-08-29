using System.Collections.Generic;
using InstaLike.Web.Services;

namespace InstaLike.Web.Infrastructure
{
    internal interface IExternalStorageConnectionProvider
    {
        void Configure(IDictionary<string, string> settings);

        IExternalStoragePictureProvider GetProvider();
    }
}