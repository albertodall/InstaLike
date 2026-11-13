using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Web.Membership.Commands;
using InstaLike.Web.Media;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Membership.CommandHandlers;

public sealed class EditMemberProfileCommandHandler : IRequestHandler<EditMemberProfileCommand, Result>
{
    private const string NicknameTaken = "This nickname is already taken.";
    private const string EmailInUse = "This email is already used by another account.";

    private readonly ISession _session;
    private readonly ILogger _logger;
    private readonly IPictureStore _pictureStore;

    public EditMemberProfileCommandHandler(ISession session, ILogger logger, IPictureStore pictureStore)
    {
        _session = session;
        _logger = logger.ForContext<EditMemberProfileCommand>();
        _pictureStore = pictureStore;
    }

    public async Task<Result> Handle(EditMemberProfileCommand request, CancellationToken cancellationToken)
    {
        var nicknameValidationResult = Nickname.Create(request.Nickname);
        var eMailValidationResult = Email.Create(request.Email);
        var fullNameValidationResult = FullName.Create(request.Name, request.Surname);

        var validationResult = Result.Combine(Environment.NewLine, nicknameValidationResult, eMailValidationResult, fullNameValidationResult);
        if (validationResult.IsFailure)
        {
            _logger.Warning("Tried to update the profile of member [{Nickname}({MemberID})] but some data were not valid: {WarningMessage}",
                request.Nickname,
                request.MemberID,
                validationResult.Error);
            return Result.Failure(validationResult.Error);
        }

        var availabilityResult = await CheckAvailabilityAsync(request.MemberID, nicknameValidationResult.Value, eMailValidationResult.Value, cancellationToken);
        if (availabilityResult.IsFailure)
        {
            _logger.Warning("Tried to update the profile of member [{Nickname}({MemberID})] but some data were already in use: {WarningMessage}",
                request.Nickname,
                request.MemberID,
                availabilityResult.Error);
            return availabilityResult;
        }

        var memberToUpdate = await _session.GetAsync<Member>(request.MemberID, cancellationToken);
        memberToUpdate.ChangeNickname(nicknameValidationResult.Value);
        memberToUpdate.ChangeEmailAddress(eMailValidationResult.Value);
        memberToUpdate.ChangeFullName(fullNameValidationResult.Value);
        memberToUpdate.UpdateBiography(request.Bio);
        if (request.ProfilePicture is { Length: > 0 })
        {
            var uploadedPicture = await _pictureStore.SaveAsync(PictureContainers.Profiles, request.ProfilePicture, cancellationToken);
            memberToUpdate.SetProfilePicture(uploadedPicture);
        }

        try
        {
            await _session.FlushAsync(cancellationToken);
        }
        catch (ADOException ex)
        {
            _logger.Error("Error updating the profile of member [{Nickname}({MemberID})]. Error message: {ErrorMessage}",
                request.Nickname,
                request.MemberID,
                ex.Message);

            return Result.Failure(ex.Message);
        }

        _logger.Information("Successfully updated the profile of member [{Nickname}({MemberID})]",
            request.Nickname,
            request.MemberID);

        return Result.Success();
    }

    private async Task<Result> CheckAvailabilityAsync(MemberId memberID, Nickname nickname, Email email, CancellationToken cancellationToken)
    {
        var otherMembersWithNickname = _session.QueryOver<Member>()
            .Where(Restrictions.Eq("Nickname", nickname.Value))
            .And(m => m.ID != memberID)
            .ToRowCountQuery()
            .FutureValue<int>();
        var otherMembersWithEmail = _session.QueryOver<Member>()
            .Where(Restrictions.Eq("Email", email.Value))
            .And(m => m.ID != memberID)
            .ToRowCountQuery()
            .FutureValue<int>();

        return Result.Combine(
            Environment.NewLine,
            Result.FailureIf(await otherMembersWithNickname.GetValueAsync(cancellationToken) > 0, NicknameTaken),
            Result.FailureIf(await otherMembersWithEmail.GetValueAsync(cancellationToken) > 0, EmailInUse));
    }
}
