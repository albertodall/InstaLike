using Azure.Core;
using Azure.Identity;

namespace InstaLike.Web.Infrastructure
{
    /// <summary>
    /// The Azure identity of the application: the managed identity in Azure (AZURE_CLIENT_ID), the developer's sign-in locally.
    /// </summary>
    internal static class AzureIdentity
    {
        public static TokenCredential Credential { get; } = new DefaultAzureCredential();
    }
}
