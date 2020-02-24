using System;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using NHibernate;

namespace InstaLike.Web.Security
{
    internal class PostAuthorRequirement : IAuthorizationRequirement
    {
    }

    internal class PostAuthorHandler : AuthorizationHandler<PostAuthorRequirement>
    {
        private readonly ISession _session;

        public PostAuthorHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PostAuthorRequirement requirement)
        {
            if (context.Resource is AuthorizationFilterContext authContext)
            {
                if (authContext.RouteData.Values["id"] is string requestedPostId)
                {
                    var post = await _session.LoadAsync<Post>(int.Parse(requestedPostId));
                    var user = await _session.LoadAsync<User>(context.User.GetIdentifier());
                    
                    if (post.CanBeEditedBy(user))
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail();
                    }
                }
            }
        }
    }
}
