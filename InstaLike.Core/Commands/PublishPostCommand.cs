﻿using CSharpFunctionalExtensions;
using MediatR;

namespace InstaLike.Core.Commands
{
    public sealed class PublishPostCommand : IRequest<Result<int>>
    {
        public int UserID { get; }
        public string Text { get; }
        public byte[] PictureRawBytes { get; }

        public PublishPostCommand(int userID, string text, byte[] pictureRawBytes)
        {
            UserID = userID;
            Text = text;
            PictureRawBytes = pictureRawBytes;
        }
    }
}
