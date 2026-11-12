using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Media;

namespace InstaLike.IntegrationTests;

internal sealed class InMemoryPictureStore : IPictureStore
{
    private readonly ConcurrentDictionary<(string Container, PictureId Picture), StoredPicture> _pictures = new();

    public Task<PictureId> SaveAsync(string container, byte[] content, CancellationToken cancellationToken)
    {
        var picture = PictureId.New();
        _pictures[(container, picture)] = new StoredPicture(content, DateTimeOffset.UtcNow);
        return Task.FromResult(picture);
    }

    public Task<Stream?> OpenReadAsync(string container, PictureId picture, CancellationToken cancellationToken)
    {
        Stream? stored = _pictures.TryGetValue((container, picture), out var storedPicture)
            ? new MemoryStream(storedPicture.Content)
            : null;
        return Task.FromResult(stored);
    }

    public IAsyncEnumerable<PictureId> ListUploadedBeforeAsync(string container, DateTimeOffset uploadedBefore, CancellationToken cancellationToken) =>
        _pictures
            .Where(p => p.Key.Container == container && p.Value.UploadedOn < uploadedBefore)
            .Select(p => p.Key.Picture)
            .ToList()
            .ToAsyncEnumerable();

    public Task TryDeleteAsync(string container, PictureId picture)
    {
        _pictures.TryRemove((container, picture), out _);
        return Task.CompletedTask;
    }

    public byte[]? GetContent(string container, PictureId picture) =>
        _pictures.TryGetValue((container, picture), out var storedPicture) ? storedPicture.Content : null;

    private sealed record StoredPicture(byte[] Content, DateTimeOffset UploadedOn);
}
