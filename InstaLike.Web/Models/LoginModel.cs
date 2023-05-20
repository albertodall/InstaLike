using System.ComponentModel.DataAnnotations;

namespace InstaLike.Web.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "User name not specified.")]
        [Display(Name = "User name")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password cannot be empty.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
        
        public string ReturnUrl { get; set; } = string.Empty;
    }
}
