using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

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
