using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

public sealed class EditPostCommand : ICommand<Result<int>>
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
