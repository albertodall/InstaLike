using System;
using System.ComponentModel.DataAnnotations;

namespace InstaLike.Web.Models
{
    public record UserDetailsModel
    {
        public string Nickname { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        [Display(Name = "Profile picture")]
        public byte[] ProfilePicture { get; set; } = Array.Empty<byte>();
    }
}
