using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Models
{
    [Bind("AuthorNickName, Text")]
    public class PublishNewPostModel
    {
        public string AuthorNickName { get; set; }

        [Required(ErrorMessage = "Please, write a text for the picture you're publishing.")]
        public string Text { get; set; }

        public byte[] Picture { get; set; }
    }
}
