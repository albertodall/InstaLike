using System;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Models
{
    public class PostModel
    {
        [HiddenInput]
        public int PostID { get; set; }
        public string AuthorNickName { get; set; } = string.Empty;
        public byte[] AuthorProfilePicture { get; set; } = Array.Empty<byte>();
        public DateTimeOffset PostDate { get; set; }
        public byte[] Picture { get; set; } = Array.Empty<byte>();
        public string Text { get; set; } = string.Empty;
        public CommentModel[] Comments { get; set; } = Array.Empty<CommentModel>();
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public bool CanBeEditedByCurrentUser { get; set; }
    }
}