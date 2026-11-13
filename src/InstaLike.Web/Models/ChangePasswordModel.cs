using System.ComponentModel.DataAnnotations;

namespace InstaLike.Web.Models
{
    public sealed class ChangePasswordModel
    {
        [Required(ErrorMessage = "Current password not specified.")]
        [Display(Name = "Current password")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password cannot be empty.")]
        [Display(Name = "New password")]
        [DataType(DataType.Password)]
        [StringLength(64, ErrorMessage = "Password should be at least {2} characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Confirm new password")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "New password and confirmation password don't match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
