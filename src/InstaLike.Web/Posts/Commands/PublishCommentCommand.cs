using CSharpFunctionalExtensions;
using InstaLike.Web.Data;

namespace InstaLike.Web.Posts.Commands;

public sealed class PublishCommentCommand : ICommand<Result<int>>
{
    public int PostID { get; }
    public string Text { get; }
    public int UserID { get; }

    public PublishCommentCommand(int postID, string text, int userID)
    {
        PostID = postID;
        Text = text;
        UserID = userID;
    }
}
