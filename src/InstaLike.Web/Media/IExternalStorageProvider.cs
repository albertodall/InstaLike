using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Web.Media;

internal interface IExternalStorageProvider
{
    Picture LoadPicture(Guid identifier, string containerName);
    void SavePicture(Picture picture, string containerName);
}
