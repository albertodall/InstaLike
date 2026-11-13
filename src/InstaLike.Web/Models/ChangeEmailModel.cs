using System.ComponentModel.DataAnnotations;

namespace InstaLike.Web.Models;

public sealed class ChangeEmailModel
{
    [Required(ErrorMessage = "E-Mail address not specified.")]
    [Display(Name = "E-Mail")]
    [DataType(DataType.EmailAddress, ErrorMessage = "E-Mail address not valid.")]
    public string Email { get; set; } = string.Empty;
}
