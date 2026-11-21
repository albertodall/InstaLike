using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Data;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Accounts
{
    public sealed class ChangePasswordModel
    {
        [Required(ErrorMessage = "Current password not specified.")]
        [Display(Name = "Current password")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password cannot be empty.")]
        [Display(Name = "New password")]
        [DataType(DataType.Password)]
        [StringLength(64, ErrorMessage = "Password should be at least {2} characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Confirm new password")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "New password and confirmation password don't match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Changes the password of an account; the result is the new security stamp of the account.
    /// </summary>
    public sealed class ChangePasswordCommand : ICommand<Result<Guid>>
    {
        public AccountId AccountID { get; }
        public string CurrentPassword { get; }
        public string NewPassword { get; }

        public ChangePasswordCommand(AccountId accountID, string currentPassword, string newPassword)
        {
            AccountID = accountID;
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
        }
    }

    internal sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<Guid>>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public ChangePasswordCommandHandler(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<ChangePasswordCommand>();
        }

        public async Task<Result<Guid>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var newPassword = Password.Create(request.NewPassword);
            if (newPassword.IsFailure)
            {
                return Result.Failure<Guid>(newPassword.Error);
            }

            var account = await _session.GetAsync<Account>(request.AccountID, cancellationToken);
            if (account is null)
            {
                return Result.Failure<Guid>(AccountErrors.NotFound(request.AccountID));
            }

            var change = account.ChangePassword(request.CurrentPassword, newPassword.Value);
            if (change.IsFailure)
            {
                _logger.Warning("Account [{Username}({AccountID})] could not change its password: {Reason}", account.Username, request.AccountID, change.Error.Description);
                return Result.Failure<Guid>(change.Error);
            }

            await _session.FlushAsync(cancellationToken);

            return Result.Ok(account.SecurityStamp);
        }
    }

    internal sealed class AccountPasswordChangedEventHandler : INotificationHandler<AccountPasswordChangedEvent>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public AccountPasswordChangedEventHandler(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<AccountPasswordChangedEvent>();
        }

        public async Task Handle(AccountPasswordChangedEvent notification, CancellationToken cancellationToken)
        {
            var account = await _session.GetAsync<Account>(notification.AccountID, cancellationToken);
            _logger.Information("Account [{Username}({AccountID})] changed its password.", account?.Username, notification.AccountID);
        }
    }
}
