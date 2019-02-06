using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace InstaLike.Web.Services
{
    public interface IUserAuthenticationService
    {
        Task<Result> AuthenticateUser(string userName, string password);
    }
}
