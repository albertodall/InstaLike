using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using InstaLike.Web.Data;

namespace InstaLike.Web.CommandHandlers
{
    internal class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand>
    {
        private readonly IRepository<User, int> _repository;

        public RegisterUserCommandHandler(IRepository<User, int> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Result> HandleAsync(RegisterUserCommand command)
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

            var userRegistrationResult = await _repository.Save(userToRegister);
            if (userRegistrationResult.IsFailure)
            {
                return Result.Fail(userRegistrationResult.Error);
            }

            return Result.Ok(userToRegister.ID);
        }
    }
}
