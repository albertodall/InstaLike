using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.CommandHandlers
{
    internal sealed class EditPostCommandHandler : IRequestHandler<EditPostCommand, Result<int>>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public EditPostCommandHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<PublishPostCommand>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<int>> Handle(EditPostCommand request, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                var editor = await _session.LoadAsync<User>(request.UserID, cancellationToken);
                var postToEdit = await _session.LoadAsync<Post>(request.PostID, cancellationToken);

                if (!postToEdit.CanBeEditedBy(editor))
                {
                    _logger.Warning("User [{NickName}({UserID})] tried to edit post {PostID} but he wasn't allowed to.",
                        editor.Nickname, 
                        request.UserID, 
                        request.PostID);
                    return Result.Fail<int>($"You're not allowed to edit post {postToEdit.ID}.");
                }

                postToEdit.UpdateText(PostText.Create(request.Text).Value);
                postToEdit.UpdatePicture(Picture.Create(request.PictureRawBytes, postToEdit.Picture.Identifier).Value);
                try
                {
                    await _session.SaveAsync(postToEdit, cancellationToken);
                    await tx.CommitAsync(cancellationToken);

                    _logger.Information("User [{Nickname}({UserID})] edited post {PostID}.",
                        editor.Nickname,
                        request.UserID,
                        request.PostID);

                    return Result.Ok(postToEdit.ID);
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync(cancellationToken);

                    _logger.Error("Failed to edit post {PostID} for user [{Nickname}({UserID})]. Error message: {ErrorMessage}",
                        request.PostID,
                        editor.Nickname,
                        request.UserID,
                        ex.Message);

                    return Result.Fail<int>(ex.Message);
                }
            }
        }
    }
}
