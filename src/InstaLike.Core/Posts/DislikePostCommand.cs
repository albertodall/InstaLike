using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

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
