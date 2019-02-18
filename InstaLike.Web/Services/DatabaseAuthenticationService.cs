using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;
using NHibernate;
using NHibernate.Criterion;

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
                var authQuery = session.QueryOver<User>()
                    .Where(Restrictions.Eq("Nickname", userName)) // Compares the private field
                    .Select(u => u.Password);

                var storedHash = await authQuery.SingleOrDefaultAsync<string>();
                if (storedHash != null)
                {
                    var storedPassword = (Password)Convert.FromBase64String(storedHash);
                    authenticated = storedPassword.HashMatches(password);
                }
            }

            return authenticated ? 
                Result.Ok(authenticated) : 
                Result.Fail("Username or password are not valid.");
        }      
    }
}
