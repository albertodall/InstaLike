using CSharpFunctionalExtensions;
using MediatR;

namespace InstaLike.Core.Commands
{
    public sealed class EditPostCommand : IRequest<Result<int>>
    {
        public int UserID { get; }
        public int PostID { get; }
        public string Text { get; }
        public byte[] PictureRawBytes { get; }

        public EditPostCommand(int userID, int postID, string text, byte[] pictureRawBytes)
        {
            UserID = userID;
            PostID = postID;
            Text = text;
            PictureRawBytes = pictureRawBytes;
        }
    }
}
