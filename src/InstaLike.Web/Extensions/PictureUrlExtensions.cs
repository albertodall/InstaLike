using InstaLike.Core.SharedKernel;
using InstaLike.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Extensions;

public static class PictureUrlExtensions
{
    private const string PictureControllerName = "Picture";
    private const string DefaultProfilePicturePath = "~/img/default_profile_picture.jpg";
    private const string PicturePlaceholderPath = "~/img/picture_placeholder.png";

    /// <returns>
    /// The default profile picture when <paramref name="picture"/> is <see langword="null"/>.
    /// </returns>
    public static string ProfilePicture(this IUrlHelper url, PictureId? picture) =>
        picture is null
            ? url.Content(DefaultProfilePicturePath)
            : url.Action(nameof(PictureController.ProfilePicture), PictureControllerName, new { id = picture.Value })!;

    /// <returns>
    /// A placeholder when <paramref name="picture"/> is <see langword="null"/>.
    /// </returns>
    public static string PostPicture(this IUrlHelper url, PictureId? picture) =>
        picture is null
            ? url.Content(PicturePlaceholderPath)
            : url.Action(nameof(PictureController.PostPicture), PictureControllerName, new { id = picture.Value })!;
}
