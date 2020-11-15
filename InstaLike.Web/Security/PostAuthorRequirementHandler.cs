using System;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NHibernate;

namespace InstaLike.Web.Security
{
    internal class PostAuthorRequirementHandler : AuthorizationHandler<PostAuthorRequirement>
    {
        private readonly NHibernate.ISession _session;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PostAuthorRequirementHandler(NHibernate.ISession session, IHttpContextAccessor httpContextAccessor)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PostAuthorRequirement requirement)
        {
            var routeData = _httpContextAccessor.HttpContext.GetRouteData();
            if (routeData.Values["id"] is string requestedPostId)
            {
                if (!int.TryParse(requestedPostId, out var postId))
                {
                    return;
                }

                var post = await _session.QueryOver<Post>()
                    .Fetch(SelectMode.ChildFetch, p => p.Author)
                    .Where(p => p.ID == postId)
                    .SingleOrDefaultAsync();

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