using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;
using NHibernate;

namespace InstaLike.Web.Services
{
    internal class DatabaseAuthenticationService : IUserAuthenticationService
    {
        private readonly ISessionFactory _sessionFactory;

        public DatabaseAuthenticationService(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        }

        public async Task<Result> AuthenticateUser(string userName, string password)
        {
            bool authenticated = false;

            using (var session = _sessionFactory.OpenStatelessSession())
            {
                var authQuery = session.CreateQuery("select u.Password from User u where u.Nickname = :nick")
                    .SetParameter("nick", userName);

                var storedPassword = (Password)Convert.FromBase64String((string)await authQuery.UniqueResultAsync());
                authenticated = storedPassword.HashMatches(password);
            }

            return authenticated ? 
                Result.Ok(authenticated) : 
                Result.Fail("Username or password are not valid.");
        }      
    }
}
