using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using NHibernate;
using NHibernate.Criterion;

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class PublishCommentCommandHandler : ICommandHandler<PublishCommentCommand>
    {
        private readonly ISession _session;

        public PublishCommentCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Result> HandleAsync(PublishCommentCommand command)
        {
            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    var authorQuery = _session.QueryOver<User>()
                        .Where(Restrictions.Eq("NickName", command.AuthorNickName))
                        .Select(u => u.ID);

                    var author = await authorQuery.SingleOrDefaultAsync();

                    var post = await _session.LoadAsync<Post>(command.PostID);

                    var comment = new Comment(post, author, command.Text);
                    await _session.SaveAsync(comment);
                    await tx.CommitAsync();
                    return Result.Ok(comment.ID);
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
