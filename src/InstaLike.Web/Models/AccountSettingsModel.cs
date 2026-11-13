namespace InstaLike.Web.Models;

public sealed class AccountSettingsModel
{
    public ChangeEmailModel ChangeEmail { get; set; } = new();
    public ChangePasswordModel ChangePassword { get; set; } = new();
}
