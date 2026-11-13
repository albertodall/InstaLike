using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Web.Media
{
    /// <summary>
    /// Stores picture content outside the database, which keeps only the <see cref="PictureId"/>.
    /// </summary>
    /// <remarks>
    /// The store is not part of the database transaction: unused pictures are deleted by the unused picture cleanup.
    /// </remarks>
    public interface IPictureStore
    {
        /// <summary>
        /// Uploads a new picture under a new identifier; existing pictures are never overwritten.
        /// </summary>
        /// <param name="container">One of the <see cref="PictureContainers"/>.</param>
        /// <param name="content">The picture bytes; must not be empty.</param>
        /// <param name="cancellationToken">Cancels the upload.</param>
        /// <returns>The identifier to store on the entity that uses the picture.</returns>
        Task<PictureId> SaveAsync(string container, byte[] content, CancellationToken cancellationToken);

        /// <summary>
        /// Opens the content of a stored picture for reading.
        /// </summary>
        /// <param name="container">The container the picture was saved in.</param>
        /// <param name="picture">The picture to read.</param>
        /// <param name="cancellationToken">Cancels the download.</param>
        /// <returns>The picture content, or <see langword="null"/> when the picture does not exist. The caller disposes it.</returns>
        Task<Stream?> OpenReadAsync(string container, PictureId picture, CancellationToken cancellationToken);

        /// <summary>
        /// Lists the pictures in a container uploaded before the given time.
        /// </summary>
        /// <param name="container">The container to list.</param>
        /// <param name="uploadedBefore">Pictures uploaded at or after this time are skipped.</param>
        /// <param name="cancellationToken">Cancels the listing.</param>
        IAsyncEnumerable<PictureId> ListUploadedBeforeAsync(string container, DateTimeOffset uploadedBefore, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a picture if it exists.
        /// </summary>
        /// <remarks>
        /// Never throws: a failure is logged and the picture is left for the next cleanup.
        /// </remarks>
        /// <param name="container">The container the picture was saved in.</param>
        /// <param name="picture">The picture to delete.</param>
        Task TryDeleteAsync(string container, PictureId picture);
    }
}
