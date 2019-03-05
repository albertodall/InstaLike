using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;

namespace InstaLike.Web.CommandHandlers
{
    internal class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result>
    {
        private readonly ISession _session;

        public RegisterUserCommandHandler(ISession session)
        {
            this._session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Result> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var nicknameValidationResult = Nickname.Create(command.Nickname);
            var eMailValidationResult = Email.Create(command.Email);
            var passwordValidationResult = Password.Create(command.Password);

            var validationResult = Result.Combine(nicknameValidationResult, eMailValidationResult, passwordValidationResult);
            if (validationResult.IsFailure)
            {
                return Result.Fail(validationResult.Error);
            }

            var userToRegister = new User(
                nicknameValidationResult.Value, 
                command.Name, 
                command.Surname, 
                passwordValidationResult.Value, 
                eMailValidationResult.Value,
                command.Biography);

            if (command.ProfilePicture != null)
            {
                userToRegister.SetProfilePicture((Picture)command.ProfilePicture);
            }
            else
            {
                userToRegister.SetDefaultProfilePicture();
            }

            try
            {
                await _session.SaveAsync(userToRegister);
                return Result.Ok(userToRegister.ID);
            }
            catch (ADOException ex)
            {
                return Result.Fail(ex.Message);
            } 
        }
    }
}
