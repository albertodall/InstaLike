using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;
using MediatR;

namespace InstaLike.Core.Commands
{
    public class LikeOrDislikePostCommand : IRequest<Result<LikePostResult>>
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
