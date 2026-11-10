using CSharpFunctionalExtensions;
using MediatR;

namespace InstaLike.Core.Posts;

public sealed class DislikePostCommand : IRequest<Result>
{
    public int PostID { get; }
    public int UserID { get; }

    public DislikePostCommand(int postID, int userID)
    {
        PostID = postID;
        UserID = userID;
    }
}
