using System.Threading.Tasks;
using InstaLike.Core.Posts;
using InstaLike.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace InstaLike.Web.Posts.Security
{
    internal sealed class PostAuthorRequirementHandler : AuthorizationHandler<PostAuthorRequirement>
    {
        private readonly NHibernate.ISession _session;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PostAuthorRequirementHandler(NHibernate.ISession session, IHttpContextAccessor httpContextAccessor)
        {
            _session = session;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PostAuthorRequirement requirement)
        {
            var routeData = _httpContextAccessor.HttpContext?.GetRouteData();
            if (routeData?.Values["id"] is string requestedPostId)
            {
                if (!PostId.TryParse(requestedPostId, null, out var postId))
                {
                    return;
                }

                var post = await _session.GetAsync<Post>(postId);

                if (post is not null && post.CanBeEditedBy(context.User.GetIdentifier()))
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
