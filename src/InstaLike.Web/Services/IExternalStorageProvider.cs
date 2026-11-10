using System;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Services;

internal interface IExternalStorageProvider
{
    Picture LoadPicture(Guid identifier, string containerName);
    void SavePicture(Picture picture, string containerName);
}
