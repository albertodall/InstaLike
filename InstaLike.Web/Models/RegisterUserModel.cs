using System;
using System.ComponentModel.DataAnnotations;

namespace InstaLike.Web.Models
{
    public class RegisterUserModel
    {
        [Required(ErrorMessage = "Nickname not specified.")]
        [Display(Name = "Nickname")]
        public string Nickname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name not specified.")]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Surname")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password cannot be empty.")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [StringLength(64, ErrorMessage = "Password should be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Confirm password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation password don't match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-Mail address not specified.")]
        [Display(Name = "E-Mail")]
        [DataType(DataType.EmailAddress, ErrorMessage = "E-Mail address not valid.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Bio")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Biography { get; set; } = string.Empty;

        public byte[] ProfilePicture { get; set; } = Array.Empty<byte>();
    }
}