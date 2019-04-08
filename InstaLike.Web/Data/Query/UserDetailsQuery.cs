using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Data.Query
{
    internal sealed class UserDetailsQuery : IRequest<UserDetailsModel>
    {
        public int CurrentUserId { get; }

        public UserDetailsQuery(int currentUserId)
        {
            CurrentUserId = currentUserId;
        }
    }

    internal sealed class UserDetailsQueryHandler : IRequestHandler<UserDetailsQuery, UserDetailsModel>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public UserDetailsQueryHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            this._logger = logger?.ForContext<UserDetailsQuery>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserDetailsModel> Handle(UserDetailsQuery request, CancellationToken cancellationToken)
        {
            UserDetailsModel result = null;

            _logger.Information("readong user details fo user {UserID} with parameters {@Request}", request.CurrentUserId, request);

            using (var tx = _session.BeginTransaction())
            {
                var user = await _session.GetAsync<User>(request.CurrentUserId);

                result = new UserDetailsModel()
                {
                    Nickname = user.Nickname,
                    Name = user.FullName.Name,
                    Surname = user.FullName.Surname,
                    Email = user.Email,
                    Bio = user.Biography,
                    ProfilePicture = user.ProfilePicture.RawBytes
                };
            }

            return result;
        }
    }
}
