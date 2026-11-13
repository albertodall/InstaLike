using System;
using CSharpFunctionalExtensions;
using InstaLike.Core.Posts;
using InstaLike.Web.Data;

namespace InstaLike.Web.Posts.Commands
{
    public sealed class EditPostCommand : ICommand<Result<PostId>>
    {
        public Guid UserID { get; }
        public PostId PostID { get; }
        public string Text { get; }
        public byte[] PictureRawBytes { get; }

        public EditPostCommand(Guid userID, PostId postID, string text, byte[] pictureRawBytes)
        {
            UserID = userID;
            PostID = postID;
            Text = text;
            PictureRawBytes = pictureRawBytes;
        }
    }
}
