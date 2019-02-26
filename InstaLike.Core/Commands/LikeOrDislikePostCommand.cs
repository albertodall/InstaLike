using CSharpFunctionalExtensions;
using MediatR;

namespace InstaLike.Core.Commands
{
    public class LikeOrDislikePostCommand : IRequest<Result>
    {
        public int PostID { get; }
        public int UserID { get; }

        public LikeOrDislikePostCommand(int postID, int userID)
        {
            PostID = postID;
            UserID = userID;
        }
    }
}
