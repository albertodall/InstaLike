using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;

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

        public UserDetailsQueryHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<UserDetailsModel> Handle(UserDetailsQuery request, CancellationToken cancellationToken)
        {
            UserDetailsModel result = null;

            using (var tx = _session.BeginTransaction())
            {
                var user = await _session.GetAsync<User>(request.CurrentUserId);

                result = new UserDetailsModel()
                {
                    UserID = request.CurrentUserId,
                    Nickname = user.Nickname,
                    Name = user.Name,
                    Surname = user.Surname,
                    Bio = user.Biography,
                    ProfilePicture = user.ProfilePicture.RawBytes
                };
            }

            return result;
        }
    }
}
