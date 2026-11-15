using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class ModelStateErrorsFixture
    {
        private static readonly Error NicknameTaken = Error.Conflict("Nickname.Taken", "This nickname is already taken.");
        private static readonly Error EmailInUse = Error.Conflict("Email.InUse", "This email is already in use.");
        private static readonly Error InvalidCredentials = Error.Failure("Account.InvalidCredentials", "Username or password are not valid.");

        [Fact]
        public void Should_Add_An_Error_To_The_Field_Its_Code_Is_About()
        {
            var sut = FormWith("Nickname", "Email");

            sut.AddModelErrors(NicknameTaken);

            ErrorsOf(sut, "Nickname").Should().Equal("This nickname is already taken.");
        }

        [Fact]
        public void Should_Add_An_Error_To_A_Prefixed_Field()
        {
            var sut = FormWith("ChangeEmail.Email");

            sut.AddModelErrors(EmailInUse);

            ErrorsOf(sut, "ChangeEmail.Email").Should().Equal("This email is already in use.");
        }

        [Fact]
        public void Should_Add_An_Error_Without_A_Field_To_The_Summary()
        {
            var sut = FormWith("Username", "Password");

            sut.AddModelErrors(InvalidCredentials);

            ErrorsOf(sut, string.Empty).Should().Equal("Username or password are not valid.");
        }

        [Fact]
        public void Should_Add_Each_Grouped_Error_To_Its_Own_Field()
        {
            var sut = FormWith("Nickname", "Email");

            sut.AddModelErrors(Result.Combine(Result.Failure(NicknameTaken), Result.Failure(EmailInUse)).Error);

            using (new AssertionScope())
            {
                ErrorsOf(sut, "Nickname").Should().Equal("This nickname is already taken.");
                ErrorsOf(sut, "Email").Should().Equal("This email is already in use.");
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
