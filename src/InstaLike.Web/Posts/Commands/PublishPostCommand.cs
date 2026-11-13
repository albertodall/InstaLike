using System;
using CSharpFunctionalExtensions;
using InstaLike.Core.Posts;
using InstaLike.Web.Data;

namespace InstaLike.Web.Posts.Commands;

public sealed class PublishPostCommand : ICommand<Result<PostId>>
{
    public Guid UserID { get; }
    public string Text { get; }
    public byte[] PictureRawBytes { get; }

    public PublishPostCommand(Guid userID, string text, byte[] pictureRawBytes)
    {
        UserID = userID;
        Text = text;
        PictureRawBytes = pictureRawBytes;
    }
}
