using System;
using System.Collections.Generic;
using System.Text;

namespace InstaLike.Core.Commands
{
    public class PublishNewPostCommand : ICommand
    {
        public string AuthorNickName { get; }
        public string Text { get; }
        public byte[] PictureRawBytes { get; }

        public PublishNewPostCommand(string authorNickName, string text, byte[] pictureRawBytes)
        {
            AuthorNickName = authorNickName;
            Text = text;
            PictureRawBytes = pictureRawBytes;
        }
    }
}
