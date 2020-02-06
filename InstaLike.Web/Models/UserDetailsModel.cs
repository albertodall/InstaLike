using System.ComponentModel.DataAnnotations;

namespace InstaLike.Web.Models
{
    public class UserDetailsModel
    {
        public string Nickname { get; set; }
        public string Password { get; set; }
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        [Display(Name = "Profile picture")]
        public byte[] ProfilePicture { get; set; }
    }
}
