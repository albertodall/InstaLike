using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

#nullable disable

namespace InstaLike.Web.Models
{
    [Bind("AuthorID, Text")]
    public class PublishPostModel
    {
        public int AuthorID { get; set; }

        [Required(ErrorMessage = "Please, write a text for the picture you're publishing.")]
        public string Text { get; set; }

        public byte[] Picture { get; set; }
    }
}
