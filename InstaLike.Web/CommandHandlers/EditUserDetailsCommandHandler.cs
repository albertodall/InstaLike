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
    public sealed class EditUserDetailsCommandHandler : IRequestHandler<EditUserDetailsCommand, Result>
    {
        private readonly ISession _session;

        public EditUserDetailsCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Result> Handle(EditUserDetailsCommand request, CancellationToken cancellationToken)
        {
            var nicknameValidationResult = Nickname.Create(request.Nickname);
            var eMailValidationResult = Email.Create(request.Email);
            var fullNameValidationResult = FullName.Create(request.Name, request.Surname);

            var validationResult = Result.Combine(nicknameValidationResult, eMailValidationResult, fullNameValidationResult);
            if (validationResult.IsFailure)
            {
                return Result.Fail(validationResult.Error);
            }

            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    var userToUpdate = await _session.GetAsync<User>(request.UserID);
                    userToUpdate.ChangeNickname(nicknameValidationResult.Value);
                    userToUpdate.ChangeEmailAddress(eMailValidationResult.Value);
                    userToUpdate.ChangeFullName(fullNameValidationResult.Value);
                    userToUpdate.UpdateBiography(request.Bio);
                    userToUpdate.SetProfilePicture((Picture)request.ProfilePicture);

                    await _session.UpdateAsync(userToUpdate);
                    await tx.CommitAsync();
                    return Result.Ok(userToUpdate.ID);
                }
                catch (ADOException)
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
