using InstaLike.Core.SharedKernel;

namespace InstaLike.Web.Data
{
    internal static class DataErrors
    {
        public static Error SaveFailed(string message) => Error.Failure("Data.SaveFailed", message);
    }
}
