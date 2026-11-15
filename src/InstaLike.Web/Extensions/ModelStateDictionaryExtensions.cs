using System.Linq;
using InstaLike.Core.SharedKernel;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InstaLike.Web.Extensions
{
    internal static class ModelStateDictionaryExtensions
    {
        /// <summary>
        /// Adds each error next to the form field its code is about, or to the summary when the form has no such field.
        /// </summary>
        /// <remarks>
        /// The field is the part of the code before the dot: "Nickname.Taken" goes to "Nickname",
        /// and "Email.InUse" goes to a prefixed field such as "ChangeEmail.Email".
        /// </remarks>
        public static void AddModelErrors(this ModelStateDictionary modelState, Error error)
        {
            var errors = error is MultipleErrors multiple ? multiple.Errors : [error];
            foreach (var single in errors)
            {
                var field = single.Code.Split('.')[0];
                var key = modelState.Keys.FirstOrDefault(k => k == field || k.EndsWith("." + field)) ?? string.Empty;

                modelState.AddModelError(key, single.Description);
            }
        }
    }
}
