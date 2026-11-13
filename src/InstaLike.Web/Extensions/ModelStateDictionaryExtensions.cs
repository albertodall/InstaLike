using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InstaLike.Web.Extensions
{
    internal static class ModelStateDictionaryExtensions
    {
        public static void AddModelErrors(this ModelStateDictionary modelState, string errorLines)
        {
            foreach (var error in errorLines.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                modelState.AddModelError(string.Empty, error);
            }
        }
    }
}
