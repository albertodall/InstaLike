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
            return OnPreInsertAsync(evt, default).GetAwaiter().GetResult();
        }

        public async Task<bool> OnPreInsertAsync(PreInsertEvent evt, CancellationToken cancellationToken)
        {
            switch (evt.Entity)
            {
                case User user:
                    await _pictureStorageProvider.SavePictureAsync(user.ProfilePicture, $"{user.ID}.jpg", "profiles");
                    break;

                case Post post:
                    await _pictureStorageProvider.SavePictureAsync(post.Picture, $"{post.ID}.jpg", "pictures");
                    break;
            }

            return false;
        }

        public bool OnPreUpdate(PreUpdateEvent evt)
        {
            return OnPreUpdateAsync(evt, default).GetAwaiter().GetResult();
        }

        public async Task<bool> OnPreUpdateAsync(PreUpdateEvent evt, CancellationToken cancellationToken)
        {
            var dirtyProperties = evt.Persister.FindDirty(evt.State, evt.OldState, evt.Entity, evt.Session);
            switch (evt.Entity)
            {
                case User user:
                    foreach (var i in dirtyProperties)
                    {
                        if (evt.Persister.PropertyNames[i] == nameof(User.ProfilePicture))
                        {
                            await _pictureStorageProvider.SavePictureAsync(user.ProfilePicture, $"{user.ID}.jpg", "profiles");
                        }
                    }
                    break;

                case Post post:
                    foreach (var i in dirtyProperties)
                    {
                        if (evt.Persister.PropertyNames[i] == nameof(Post.Picture))
                        {
                            await _pictureStorageProvider.SavePictureAsync(post.Picture, $"{post.ID}.jpg", "pictures");
                        }
                    }
                    break;
            }
            
            return false;
        }
    }
}
