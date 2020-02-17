using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Transform;
using Serilog;

namespace InstaLike.Web.Data.Query
{
    public class EditPostQuery : IRequest<PublishPostModel>
    {
        public int PostID { get; }

        public int CurrentUserID { get; }

        public EditPostQuery(int postID, int currentUserID)
        {
            PostID = postID;
            CurrentUserID = currentUserID;
        }
    }

    internal sealed class EditPostQueryHandler : IRequestHandler<EditPostQuery, PublishPostModel>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public EditPostQueryHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<EditPostQuery>() ?? throw new ArgumentNullException(nameof(logger)) ;
        }

        public async Task<PublishPostModel> Handle(EditPostQuery request, CancellationToken cancellationToken)
        {
            _logger.Debug("User {UserID} requested to edit post {PostID} with parameters {@Request}", request.CurrentUserID, request.PostID, request);

            using (_session.BeginTransaction())
            {
                var post = await _session.GetAsync<Post>(request.PostID, cancellationToken);

                return new PublishPostModel()
                {
                    AuthorID = post.Author.ID,
                    Picture = post.Picture.RawBytes,
                    Text = post.Text
                };
            }
        }
    }
}
