using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Services;
using NHibernate.Event;

namespace InstaLike.Web.Infrastructure
{
    internal class ExternalStorageLoadEventListener : IPreLoadEventListener
    {
        private readonly IExternalStoragePictureProvider _pictureStorageProvider;

        public ExternalStorageLoadEventListener(IExternalStoragePictureProvider pictureStorageProvider)
        {
            _pictureStorageProvider = pictureStorageProvider ?? throw new ArgumentNullException(nameof(pictureStorageProvider));
        }

        public void OnPreLoad(PreLoadEvent evt)
        {
            OnPreLoadAsync(evt, default).GetAwaiter().GetResult();
        }

        public async Task OnPreLoadAsync(PreLoadEvent evt, CancellationToken cancellationToken)
        {
            switch (evt.Entity)
            {
                case User user:
                    var profilePicture = await _pictureStorageProvider.LoadPictureAsync($"{user.ID}.jpg", "profiles");
                    user.SetProfilePicture(profilePicture);
                    break;

                case Post post:
                    var postPicture = await _pictureStorageProvider.LoadPictureAsync($"{post.ID}.jpg", "pictures");
                    post.GetType().GetProperty("Picture").SetValue(post, postPicture);
                    break;
            }
        }
    }
}