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
            if (evt.Persister.ConcreteProxyClass == typeof(User))
            {
                var entity = evt.Entity as User;
                var profilePicture = _pictureStorageProvider.LoadPictureAsync($"{entity.ID}.jpg", "profiles").Result;
                entity.SetProfilePicture(profilePicture);
            }
        }

        public async Task OnPreLoadAsync(PreLoadEvent evt, CancellationToken cancellationToken)
        {
            if (evt.Persister.ConcreteProxyClass == typeof(User))
            {
                var entity = evt.Entity as User;
                var profilePicture = await _pictureStorageProvider.LoadPictureAsync($"{entity.ID}.jpg", "profiles");
                entity.SetProfilePicture(profilePicture);
            }
        }
    }
}
