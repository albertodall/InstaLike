using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Media;

namespace InstaLike.IntegrationTests;

internal sealed class InMemoryPictureStore : IPictureStore
{
    private readonly ConcurrentDictionary<(string Container, PictureId Picture), byte[]> _pictures = new();

    public Task<PictureId> SaveAsync(string container, byte[] content, CancellationToken cancellationToken)
    {
        var picture = PictureId.New();
        _pictures[(container, picture)] = content;
        return Task.FromResult(picture);
    }

    public Task<Stream?> OpenReadAsync(string container, PictureId picture, CancellationToken cancellationToken)
    {
        Stream? stored = _pictures.TryGetValue((container, picture), out var content)
            ? new MemoryStream(content)
            : null;
        return Task.FromResult(stored);
    }

    public Task TryDeleteAsync(string container, PictureId? picture)
    {
        if (picture is not null)
        {
            _pictures.TryRemove((container, picture), out _);
        }

        return Task.CompletedTask;
    }

    public byte[]? GetContent(string container, PictureId picture) =>
        _pictures.TryGetValue((container, picture), out var content) ? content : null;
}
