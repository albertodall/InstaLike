using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Services;
using NHibernate.Event;

namespace InstaLike.Web.Infrastructure
{
    internal class ExternalStorageSaveEventListener : IPreInsertEventListener, IPreUpdateEventListener
    {
        private readonly IExternalStoragePictureProvider _pictureStorageProvider;

        public ExternalStorageSaveEventListener(IExternalStoragePictureProvider pictureStorageProvider)
        {
            _pictureStorageProvider = pictureStorageProvider ?? throw new ArgumentNullException(nameof(pictureStorageProvider));
        }

        public bool OnPreInsert(PreInsertEvent evt)
        {
            if (evt.Persister.ConcreteProxyClass == typeof(User))
            {
                var user = evt.Entity as User;
                _pictureStorageProvider.SavePictureAsync(user.ProfilePicture, $"{user.ID}.jpg", "profiles").GetAwaiter().GetResult();
            }

            return false;
        }

        public async Task<bool> OnPreInsertAsync(PreInsertEvent evt, CancellationToken cancellationToken)
        {
            if (evt.Persister.ConcreteProxyClass == typeof(User))
            {
                var user = evt.Entity as User;
                await _pictureStorageProvider.SavePictureAsync(user.ProfilePicture, $"{user.ID}.jpg", "profiles");
            }

            return false;
        }

        public bool OnPreUpdate(PreUpdateEvent evt)
        {
            if (evt.Persister.ConcreteProxyClass == typeof(User))
            {
                var user = evt.Entity as User;
                var dirtyProperties = evt.Persister.FindDirty(evt.State, evt.OldState, evt.Entity, evt.Session);
                foreach (var i in dirtyProperties)
                {
                    if (evt.Persister.PropertyNames[i].ToUpper() == nameof(User.ProfilePicture).ToUpper())
                    {
                        _pictureStorageProvider.SavePictureAsync(user.ProfilePicture, $"{user.ID}.jpg", "profiles").GetAwaiter().GetResult();
                    }
                }
            }

            return false;
        }

        public async Task<bool> OnPreUpdateAsync(PreUpdateEvent evt, CancellationToken cancellationToken)
        {
            if (evt.Persister.ConcreteProxyClass == typeof(User))
            {
                var user = evt.Entity as User;
                var dirtyProperties = evt.Persister.FindDirty(evt.State, evt.OldState, evt.Entity, evt.Session);
                foreach (var i in dirtyProperties)
                {
                    if (evt.Persister.PropertyNames[i] == nameof(User.ProfilePicture))
                    {
                        await _pictureStorageProvider.SavePictureAsync(user.ProfilePicture, $"{user.ID}.jpg", "profiles");
                    }
                }
            }

            return false;
        }
    }
}
