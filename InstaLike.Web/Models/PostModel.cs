﻿using Microsoft.AspNetCore.Mvc;
using System;

namespace InstaLike.Web.Models
{
    public class PostModel
    {
        [HiddenInput]
        public int PostID { get; set; }
        public string AuthorNickName { get; set; }
        public byte[] AuthorProfilePicture { get; set; }
        public DateTimeOffset PostDate { get; set; }
        public byte[] Picture { get; set; }
        public string Text { get; set; }
        public CommentModel[] Comments { get; set; } = { };
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public bool CanBeEditedByCurrentUser { get; set; }
    }
}