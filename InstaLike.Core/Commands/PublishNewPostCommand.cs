using System;
using System.Collections.Generic;
using System.Text;

namespace InstaLike.Core.Commands
{
    public class PublishNewPostCommand : ICommand
    {
        public int UserID { get; }
        public string Text { get; }
        public byte[] PictureRawBytes { get; }

        public PublishNewPostCommand(int userID, string text, byte[] pictureRawBytes)
        {
            UserID = userID;
            Text = text;
            PictureRawBytes = pictureRawBytes;
        }
    }
}
