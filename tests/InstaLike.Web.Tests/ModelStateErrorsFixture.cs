using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;

namespace InstaLike.Web.Tests
{
    public sealed class ModelStateErrorsFixture
    {
        [Fact]
        public void Should_Add_An_Error_To_The_Field_Its_Code_Is_About()
        {
            var sut = FormWith("Nickname", "Email");

            sut.AddModelErrors(MemberErrors.NicknameTaken);

            ErrorsOf(sut, "Nickname").Should().Equal(MemberErrors.NicknameTaken.Description);
        }

        [Fact]
        public void Should_Add_An_Error_To_A_Prefixed_Field()
        {
            var sut = FormWith("ChangeEmail.Email");

            sut.AddModelErrors(AccountErrors.EmailInUse);

            ErrorsOf(sut, "ChangeEmail.Email").Should().Equal(AccountErrors.EmailInUse.Description);
        }

        [Fact]
        public void Should_Add_An_Error_Without_A_Field_To_The_Summary()
        {
            var sut = FormWith("Username", "Password");

            sut.AddModelErrors(AccountErrors.InvalidCredentials);

            ErrorsOf(sut, string.Empty).Should().Equal(AccountErrors.InvalidCredentials.Description);
        }

        [Fact]
        public void Should_Add_Each_Grouped_Error_To_Its_Own_Field()
        {
            var sut = FormWith("Nickname", "Email");

            sut.AddModelErrors(Result.Combine(Result.Failure(MemberErrors.NicknameTaken), Result.Failure(AccountErrors.EmailInUse)).Error);

            using (new AssertionScope())
            {
                ErrorsOf(sut, "Nickname").Should().Equal(MemberErrors.NicknameTaken.Description);
                ErrorsOf(sut, "Email").Should().Equal(AccountErrors.EmailInUse.Description);
                ErrorsOf(sut, string.Empty).Should().BeEmpty();
            }
        }

        /// <summary>
        /// A model state with the given fields, as a posted form has.
        /// </summary>
        private static ModelStateDictionary FormWith(params string[] fields)
        {
            var modelState = new ModelStateDictionary();
            foreach (var field in fields)
            {
                modelState.SetModelValue(field, string.Empty, string.Empty);
            }

            return modelState;
        }

        /// <summary>
        /// The error messages of the field, or none when it has no errors.
        /// </summary>
        private static string[] ErrorsOf(ModelStateDictionary modelState, string key) =>
            modelState.TryGetValue(key, out var entry)
                ? entry.Errors.Select(e => e.ErrorMessage).ToArray()
                : [];
    }
}
