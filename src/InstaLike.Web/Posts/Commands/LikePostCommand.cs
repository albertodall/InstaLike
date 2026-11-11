using CSharpFunctionalExtensions;
using InstaLike.Web.Data;

namespace InstaLike.Web.Posts.Commands;

public sealed class LikePostCommand : ICommand<Result>
{
    public int PostID { get; }
    public int UserID { get; }

    public LikePostCommand(int postID, int userID)
    {
        PostID = postID;
        UserID = userID;
    }
}
