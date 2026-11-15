using System.Collections.Generic;
using System.Linq;

namespace InstaLike.Core.SharedKernel
{
    /// <summary>
    /// Groups the errors of several failed results, so that all of them can be reported at once.
    /// </summary>
    /// <remarks>
    /// The description joins the descriptions of the grouped errors. The type is the one they share,
    /// or <see cref="ErrorType.Failure"/> when their types differ.
    /// </remarks>
    public sealed record MultipleErrors : Error
    {
        public IReadOnlyList<Error> Errors { get; }

        public MultipleErrors(IReadOnlyList<Error> errors)
            : base("Error.Multiple", string.Join(" ", errors.Select(e => e.Description)), CommonTypeOrFailure(errors))
        {
            Errors = errors;
        }

        private static ErrorType CommonTypeOrFailure(IReadOnlyList<Error> errors) =>
            errors.Select(e => e.Type).Distinct().Count() == 1 ? errors[0].Type : ErrorType.Failure;
    }
}
