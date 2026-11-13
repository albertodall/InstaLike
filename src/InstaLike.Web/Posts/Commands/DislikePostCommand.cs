using System;
using CSharpFunctionalExtensions;
using InstaLike.Core.Posts;
using InstaLike.Web.Data;

namespace InstaLike.Web.Posts.Commands;

public sealed class DislikePostCommand : ICommand<Result>
{
    public PostId PostID { get; }
    public Guid UserID { get; }

    public DislikePostCommand(PostId postID, Guid userID)
    {
        PostID = postID;
        UserID = userID;
    }
}
