using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Models
{
    [Bind("PostID, AuthorID, Text")]
    public class EditPostModel
    {
        public int PostID { get; set; }

        public int AuthorID { get; set; }

        [Required(ErrorMessage = "Please, write a text for the picture you're publishing.")]
        public string Text { get; set; } = string.Empty;

        public byte[] Picture { get; set; } = Array.Empty<byte>();
    }
}
