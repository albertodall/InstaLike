using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace InstaLike.Web.Controllers;

[Authorize]
[Route("pictures")]
public sealed class PictureController : Controller
{
    private const string MissingPicturePath = "~/img/no_image_available.png";
    private const string PictureContentType = "image/jpeg";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(365);

    private readonly IPictureStore _pictureStore;

    public PictureController(IPictureStore pictureStore)
    {
        _pictureStore = pictureStore ?? throw new ArgumentNullException(nameof(pictureStore));
    }

    [HttpGet("posts/{id:guid}")]
    public Task<IActionResult> PostPicture(Guid id, CancellationToken cancellationToken) =>
        GetPicture(PictureContainers.Posts, id, cancellationToken);

    [HttpGet("profiles/{id:guid}")]
    public Task<IActionResult> ProfilePicture(Guid id, CancellationToken cancellationToken) =>
        GetPicture(PictureContainers.Profiles, id, cancellationToken);

    private async Task<IActionResult> GetPicture(string container, Guid id, CancellationToken cancellationToken)
    {
        var picture = await _pictureStore.OpenReadAsync(container, new PictureId(id), cancellationToken);
        if (picture is null)
        {
            Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue { NoCache = true };
            return File(MissingPicturePath, "image/png");
        }

        Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
        {
            Private = true,
            MaxAge = CacheDuration,
            Extensions = { new NameValueHeaderValue("immutable") }
        };
        return File(picture, PictureContentType);
    }
}
