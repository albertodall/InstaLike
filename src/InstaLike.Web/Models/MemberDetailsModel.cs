using System.ComponentModel.DataAnnotations;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Web.Models;

public record MemberDetailsModel
{
    public string Nickname { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public PictureId? ProfilePicture { get; set; }
}
