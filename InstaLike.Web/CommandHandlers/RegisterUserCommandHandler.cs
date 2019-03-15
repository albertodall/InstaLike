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
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var nicknameValidationResult = Nickname.Create(request.Nickname);
            var eMailValidationResult = Email.Create(request.Email);
            var passwordValidationResult = Password.Create(request.Password);

            var validationResult = Result.Combine(nicknameValidationResult, eMailValidationResult, passwordValidationResult);
            if (validationResult.IsFailure)
            {
                return Result.Fail(validationResult.Error);
            }

            var userToRegister = new User(
                nicknameValidationResult.Value, 
                request.Name, 
                request.Surname, 
                passwordValidationResult.Value, 
                eMailValidationResult.Value,
                request.Biography);

            if (request.ProfilePicture != null)
            {
                userToRegister.SetProfilePicture((Picture)request.ProfilePicture);
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
