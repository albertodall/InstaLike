using System.Collections.Generic;
using InstaLike.Web.Services;

namespace InstaLike.Web.Infrastructure
{
    internal interface IHybridStorageConnectionProvider
    {
        void Configure(IDictionary<string, string> settings);

        IExternalStorageProvider? GetProvider();
    }
}