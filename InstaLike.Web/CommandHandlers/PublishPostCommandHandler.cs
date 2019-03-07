using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class PublishPostCommandHandler : IRequestHandler<PublishPostCommand, Result>
    {
        private readonly ISession _session;

        public PublishPostCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Result> Handle(PublishPostCommand request, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    var author = await _session.LoadAsync<User>(request.UserID);

                    var post = new Post(author, (Picture)request.PictureRawBytes, (PostText)request.Text);
                    await _session.SaveAsync(post);
                    await tx.CommitAsync();
                    return Result.Ok(post.ID);
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    return Result.Fail(ex.Message);
                }
            }
        }
    }
}
