using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

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
