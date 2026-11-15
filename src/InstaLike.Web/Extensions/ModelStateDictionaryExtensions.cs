using InstaLike.Core.SharedKernel;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InstaLike.Web.Extensions
{
    internal static class ModelStateDictionaryExtensions
    {
        public static void AddModelErrors(this ModelStateDictionary modelState, Error error)
        {
            var errors = error is MultipleErrors multiple ? multiple.Errors : [error];
            foreach (var single in errors)
            {
                modelState.AddModelError(string.Empty, single.Description);
            }
        }
    }
}
