using System;
using System.ComponentModel.DataAnnotations;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Models
{
    [Bind("PostID, AuthorID, Text")]
    public sealed class EditPostModel
    {
        public required PostId PostID { get; set; }

        public Guid AuthorID { get; set; }

        [Required(ErrorMessage = "Please, write a text for the picture you're publishing.")]
        public string Text { get; set; } = string.Empty;

        public PictureId? Picture { get; set; }
    }
}
