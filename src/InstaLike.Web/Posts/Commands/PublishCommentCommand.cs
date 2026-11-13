using System;
using CSharpFunctionalExtensions;
using InstaLike.Core.Posts;
using InstaLike.Web.Data;

namespace InstaLike.Web.Posts.Commands;

public sealed class PublishCommentCommand : ICommand<Result<Guid>>
{
    public PostId PostID { get; }
    public string Text { get; }
    public Guid UserID { get; }

    public PublishCommentCommand(PostId postID, string text, Guid userID)
    {
        PostID = postID;
        Text = text;
        UserID = userID;
    }
}
