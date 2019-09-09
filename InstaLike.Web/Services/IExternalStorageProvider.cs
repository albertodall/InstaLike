using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Services
{
    internal interface IExternalStorageProvider
    {
        Task<Picture> LoadPictureAsync(string blobFileName, string containerName);
        Task SavePictureAsync(Picture picture, string containerName);
    }
}