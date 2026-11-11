using CSharpFunctionalExtensions;
using InstaLike.Web.Data;

namespace InstaLike.Web.Posts.Commands;

public sealed class PublishPostCommand : ICommand<Result<int>>
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
