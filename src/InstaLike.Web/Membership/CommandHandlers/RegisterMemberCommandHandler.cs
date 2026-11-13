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

internal sealed class RegisterMemberCommandHandler : IRequestHandler<RegisterMemberCommand, Result<MemberId>>
{
    private const string NicknameTaken = "This nickname is already taken.";
    private const string EmailInUse = "This email is already used by another account.";

    private readonly ISession _session;
    private readonly ILogger _logger;
    private readonly IPictureStore _pictureStore;

    public RegisterMemberCommandHandler(ISession session, ILogger logger, IPictureStore pictureStore)
    {
        _session = session;
        _logger = logger.ForContext<RegisterMemberCommand>();
        _pictureStore = pictureStore;
    }

    public async Task<Result<MemberId>> Handle(RegisterMemberCommand request, CancellationToken cancellationToken)
    {
        var nicknameValidationResult = Nickname.Create(request.Nickname);
        var eMailValidationResult = Email.Create(request.Email);
        var passwordValidationResult = Password.Create(request.Password);
        var fullNameValidationResult = FullName.Create(request.Name, request.Surname);

        var validationResult = Result.Combine(Environment.NewLine, nicknameValidationResult, eMailValidationResult, passwordValidationResult, fullNameValidationResult);
        if (validationResult.IsFailure)
        {
            _logger.Warning("Error during registration of member {Nickname}: Error message: {ErrorMessage}",
                request.Nickname,
                validationResult.Error);
            return Result.Failure<MemberId>(validationResult.Error);
        }

        var availabilityResult = await CheckAvailabilityAsync(nicknameValidationResult.Value, eMailValidationResult.Value, cancellationToken);
        if (availabilityResult.IsFailure)
        {
            _logger.Warning("Error during registration of member {Nickname}: Error message: {ErrorMessage}",
                request.Nickname,
                availabilityResult.Error);
            return Result.Failure<MemberId>(availabilityResult.Error);
        }

        var memberToRegister = new Member(
            nicknameValidationResult.Value,
            fullNameValidationResult.Value,
            passwordValidationResult.Value,
            eMailValidationResult.Value,
            request.Biography);

        if (request.ProfilePicture is { Length: > 0 })
        {
            var profilePicture = await _pictureStore.SaveAsync(PictureContainers.Profiles, request.ProfilePicture, cancellationToken);
            memberToRegister.SetProfilePicture(profilePicture);
        }

        try
        {
            await _session.SaveAsync(memberToRegister, cancellationToken);
            await _session.FlushAsync(cancellationToken);
        }
        catch (ADOException ex)
        {
            _logger.Error("Error during registration of member {Nickname}. Error message: {ErrorMessage}",
                request.Nickname,
                ex.Message);

            return Result.Failure<MemberId>(ex.Message);
        }

        _logger.Information("Member [{Nickname}({MemberID})] has just registered.",
            request.Nickname,
            memberToRegister.ID);

        return Result.Success(memberToRegister.ID);
    }

    private async Task<Result> CheckAvailabilityAsync(Nickname nickname, Email email, CancellationToken cancellationToken)
    {
        var membersWithNickname = _session.QueryOver<Member>()
            .Where(Restrictions.Eq("Nickname", nickname.Value))
            .ToRowCountQuery()
            .FutureValue<int>();
        var membersWithEmail = _session.QueryOver<Member>()
            .Where(Restrictions.Eq("Email", email.Value))
            .ToRowCountQuery()
            .FutureValue<int>();

        return Result.Combine(
            Environment.NewLine,
            Result.FailureIf(await membersWithNickname.GetValueAsync(cancellationToken) > 0, NicknameTaken),
            Result.FailureIf(await membersWithEmail.GetValueAsync(cancellationToken) > 0, EmailInUse));
    }
}
