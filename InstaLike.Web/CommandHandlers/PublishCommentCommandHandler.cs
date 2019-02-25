﻿using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using NHibernate;

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
                    var author = await _session.LoadAsync<User>(command.UserID);
                    var post = await _session.LoadAsync<Post>(command.PostID);

                    var comment = new Comment(post, author, (CommentText)command.Text);
                    post.AddComment(comment);
                    await _session.SaveAsync(post);
                    await tx.CommitAsync();
                    return Result.Ok(comment.ID);
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    return Result.Fail(ex.Message);
                }
            }
        }
    }
}
