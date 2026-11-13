using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Posts;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Posts.Queries;

public sealed class EditPostQuery : IRequest<EditPostModel>
{
    public PostId PostID { get; }

    public Guid CurrentUserID { get; }

    public EditPostQuery(PostId postID, Guid currentUserID)
    {
        PostID = postID;
        CurrentUserID = currentUserID;
    }
}

internal sealed class EditPostQueryHandler : IRequestHandler<EditPostQuery, EditPostModel>
{
    private readonly IStatelessSession _session;
    private readonly ILogger _logger;

    public EditPostQueryHandler(IStatelessSession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<EditPostQuery>();
    }

    public async Task<EditPostModel> Handle(EditPostQuery request, CancellationToken cancellationToken)
    {
        _logger.Debug("User {UserID} requested to edit post {PostID} with parameters {@Request}", request.CurrentUserID, request.PostID, request);

        using (_session.BeginTransaction())
        {
            var post = await _session.GetAsync<Post>(request.PostID, cancellationToken);

            return new EditPostModel()
            {
                PostID = post.ID,
                AuthorID = post.AuthorID,
                Picture = post.Picture,
                Text = post.Text
            };
        }
    }
}
