using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Data;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Accounts
{
    public sealed class LoginModel
    {
        [Required(ErrorMessage = "User name not specified.")]
        [Display(Name = "User name")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password cannot be empty.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Logs an account in; the result is the member the account belongs to and the security stamp of the account.
    /// </summary>
    public sealed class LogInCommand : ICommand<Result<(Guid MemberID, Guid SecurityStamp)>>
    {
        public string Username { get; }
        public string Password { get; }

        public LogInCommand(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    internal sealed class LogInCommandHandler : IRequestHandler<LogInCommand, Result<(Guid MemberID, Guid SecurityStamp)>>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public LogInCommandHandler(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<LogInCommand>();
        }

        public async Task<Result<(Guid MemberID, Guid SecurityStamp)>> Handle(LogInCommand request, CancellationToken cancellationToken)
        {
            var account = await _session.QueryOver<Account>()
                .Where(Restrictions.Eq("Username", request.Username))
                .SingleOrDefaultAsync(cancellationToken);
            if (account is null)
            {
                _logger.Debug("Account {Username} does not exist.", request.Username);
                return Result.Failure<(Guid MemberID, Guid SecurityStamp)>(AccountErrors.InvalidCredentials);
            }

            var logIn = account.LogIn(request.Password);
            if (logIn.IsFailure)
            {
                _logger.Debug("Account {Username} did not authenticate correctly.", request.Username);
                return Result.Failure<(Guid MemberID, Guid SecurityStamp)>(logIn.Error);
            }

            return Result.Ok((account.ID.Value, account.SecurityStamp));
        }
    }

    internal sealed class AccountLoggedInEventHandler : INotificationHandler<AccountLoggedInEvent>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public AccountLoggedInEventHandler(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<AccountLoggedInEvent>();
        }

        public async Task Handle(AccountLoggedInEvent notification, CancellationToken cancellationToken)
        {
            var account = await _session.GetAsync<Account>(notification.AccountID, cancellationToken);
            _logger.Information("Account [{Username}({AccountID})] has just logged in.", account?.Username, notification.AccountID);
        }
    }
}
