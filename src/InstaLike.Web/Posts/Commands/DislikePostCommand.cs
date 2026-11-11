using CSharpFunctionalExtensions;
using InstaLike.Web.Data;

namespace InstaLike.Web.Posts.Commands;

public sealed class DislikePostCommand : ICommand<Result>
{
    public int PostID { get; }
    public int UserID { get; }

    public DislikePostCommand(int postID, int userID)
    {
        PostID = postID;
        UserID = userID;
    }
}
