using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

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
