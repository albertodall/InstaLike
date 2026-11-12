using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using Microsoft.Extensions.Hosting;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Media;

/// <summary>
/// Commands upload pictures before their transaction commits and never delete the pictures they replace,
/// so this job periodically deletes the pictures no entity uses.
/// </summary>
internal sealed class UnusedPictureCleanupService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    /// <summary>
    /// Leaves alone pictures whose transaction may still be running.
    /// </summary>
    private static readonly TimeSpan GracePeriod = TimeSpan.FromHours(1);

    private readonly ISessionFactory _sessionFactory;
    private readonly IPictureStore _pictureStore;
    private readonly ILogger _logger;

    public UnusedPictureCleanupService(ISessionFactory sessionFactory, IPictureStore pictureStore, ILogger logger)
    {
        _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        _pictureStore = pictureStore ?? throw new ArgumentNullException(nameof(pictureStore));
        _logger = logger?.ForContext<UnusedPictureCleanupService>() ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task CleanUpUploadedBeforeAsync(DateTimeOffset uploadedBefore, CancellationToken cancellationToken)
    {
        IList<PictureId> usedPostPictures;
        IList<PictureId> usedProfilePictures;
        using (var session = _sessionFactory.OpenStatelessSession())
        {
            usedPostPictures = await session.QueryOver<Post>()
                .Select(p => p.Picture)
                .ListAsync<PictureId>(cancellationToken);
            usedProfilePictures = await session.QueryOver<User>()
                .Select(u => u.ProfilePicture)
                .ListAsync<PictureId>(cancellationToken);
        }

        await DeletePicturesNotInUseAsync(PictureContainers.Posts, usedPostPictures, uploadedBefore, cancellationToken);
        await DeletePicturesNotInUseAsync(PictureContainers.Profiles, usedProfilePictures, uploadedBefore, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                DateTimeOffset cutOffDate = DateTimeOffset.UtcNow - GracePeriod;
                await CleanUpUploadedBeforeAsync(cutOffDate, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.Error(ex, "The unused picture cleanup failed; it will run again in {Interval}.", Interval);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DeletePicturesNotInUseAsync(string container, IList<PictureId> usedPictures, DateTimeOffset uploadedBefore, CancellationToken cancellationToken)
    {
        var used = usedPictures.ToHashSet();
        var deleted = 0;
        await foreach (var picture in _pictureStore.ListUploadedBeforeAsync(container, uploadedBefore, cancellationToken))
        {
            if (!used.Contains(picture))
            {
                await _pictureStore.TryDeleteAsync(container, picture);
                deleted++;
            }
        }

        _logger.Information("Deleted {Count} unused pictures from container {Container}.", deleted, container);
    }
}
