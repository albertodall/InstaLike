using CSharpFunctionalExtensions;
using MediatR;

namespace InstaLike.Core.Commands
{
    public sealed class LikePostCommand : IRequest<Result>
    {
        public int PostID { get; }
        public int UserID { get; }

        public LikePostCommand(int postID, int userID)
        {
            PostID = postID;
            UserID = userID;
        }
    }
}
