using System.ComponentModel.DataAnnotations;

#nullable disable

namespace InstaLike.Web.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "User name not specified.")]
        [Display(Name = "User name")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password cannot be empty.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
        
        public string ReturnUrl { get; set; }
    }
}
