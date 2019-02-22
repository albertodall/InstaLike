using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using NHibernate;

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class PublishNewPostCommandHandler : ICommandHandler<PublishNewPostCommand>
    {
        private readonly ISession _session;

        public PublishNewPostCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Result> HandleAsync(PublishNewPostCommand command)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    var author = await _session.LoadAsync<User>(command.UserID);

                    var post = new Post(author, (Picture)command.PictureRawBytes, command.Text);
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
