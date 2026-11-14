using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Membership;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Data;
using InstaLike.Web.Media;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Membership
{
    public record MemberDetailsModel
    {
        public string Nickname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public PictureId? ProfilePicture { get; set; }
    }

    internal sealed class MemberDetailsQuery : IRequest<MemberDetailsModel>
    {
        public MemberId MemberID { get; }

        public MemberDetailsQuery(MemberId memberID)
        {
            MemberID = memberID;
        }
    }

    internal sealed class MemberDetailsQueryHandler : IRequestHandler<MemberDetailsQuery, MemberDetailsModel>
    {
        private readonly IStatelessSession _session;
        private readonly ILogger _logger;

        public MemberDetailsQueryHandler(IStatelessSession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<MemberDetailsQuery>();
        }

        public async Task<MemberDetailsModel> Handle(MemberDetailsQuery request, CancellationToken cancellationToken)
        {
            MemberDetailsModel result;

            _logger.Information("Reading member details of member {MemberID} with parameters {@Request}", request.MemberID, request);

            using (var tx = _session.BeginTransaction())
            {
                MemberDetailsModel memberDetails = null!;
                result = await _session.QueryOver<Member>()
                    .Where(m => m.ID == request.MemberID)
                    .SelectList(list => list
                        .Select(m => m.Nickname).WithAlias(() => memberDetails.Nickname)
                        .Select(m => m.FullName.Name).WithAlias(() => memberDetails.Name)
                        .Select(m => m.FullName.Surname).WithAlias(() => memberDetails.Surname)
                        .Select(m => m.Biography).WithAlias(() => memberDetails.Bio)
                        .Select(m => m.ProfilePicture).WithAlias(() => memberDetails.ProfilePicture)
                    )
                    .TransformUsing(new EntityToModelResultTransformer<MemberDetailsModel>())
                    .SingleOrDefaultAsync<MemberDetailsModel>(cancellationToken);

                await tx.CommitAsync(cancellationToken);
            }

            return result;
        }
    }

    public sealed class EditMemberProfileCommand : ICommand<Result>
    {
        public MemberId MemberID { get; }
        public string Nickname { get; }
        public string Name { get; }
        public string Surname { get; }
        public string Bio { get; }
        /// <summary>The new profile picture, or <see langword="null"/> to keep the current one.</summary>
        public byte[]? ProfilePicture { get; }

        public EditMemberProfileCommand(MemberId memberID, string nickname, string name, string surname, string bio, byte[]? profilePicture)
        {
            MemberID = memberID;
            Nickname = nickname;
            Name = name;
            Surname = surname;
            Bio = bio;
            ProfilePicture = profilePicture;
        }
    }

    public sealed class EditMemberProfileCommandHandler : IRequestHandler<EditMemberProfileCommand, Result>
    {
        private const string NicknameTaken = "This nickname is already taken.";

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
            var fullNameValidationResult = FullName.Create(request.Name, request.Surname);

            var validationResult = Result.Combine(Environment.NewLine, nicknameValidationResult, fullNameValidationResult);
            if (validationResult.IsFailure)
            {
                _logger.Warning("Tried to update the profile of member [{Nickname}({MemberID})] but some data were not valid: {WarningMessage}",
                    request.Nickname,
                    request.MemberID,
                    validationResult.Error);
                return Result.Failure(validationResult.Error);
            }

            var availabilityResult = await CheckNicknameAvailabilityAsync(request.MemberID, nicknameValidationResult.Value, cancellationToken);
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

        private async Task<Result> CheckNicknameAvailabilityAsync(MemberId memberID, Nickname nickname, CancellationToken cancellationToken)
        {
            var otherMembersWithNickname = await _session.QueryOver<Member>()
                .Where(Restrictions.Eq("Nickname", nickname.Value))
                .And(m => m.ID != memberID)
                .RowCountAsync(cancellationToken);

            return Result.FailureIf(otherMembersWithNickname > 0, NicknameTaken);
        }
    }
}
