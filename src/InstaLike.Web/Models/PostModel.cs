using System;
using InstaLike.Core.SharedKernel;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Models;

public class PostModel
{
    [HiddenInput]
    public int PostID { get; set; }
    public string AuthorNickName { get; set; } = string.Empty;
    public PictureId? AuthorProfilePicture { get; set; }
    public DateTimeOffset PostDate { get; set; }
    public PictureId? Picture { get; set; }
    public string Text { get; set; } = string.Empty;
    public CommentModel[] Comments { get; set; } = Array.Empty<CommentModel>();
    public int LikesCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public bool CanBeLikedByCurrentUser { get; set; }
    public bool CanBeEditedByCurrentUser { get; set; }
}
