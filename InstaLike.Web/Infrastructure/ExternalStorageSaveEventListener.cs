using System;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Event;

namespace InstaLike.Web.Infrastructure
{
    internal class ExternalStorageSaveEventListener : ISaveOrUpdateEventListener
    {
        public void OnSaveOrUpdate(SaveOrUpdateEvent evt)
        {
            throw new NotImplementedException();
        }

        public Task OnSaveOrUpdateAsync(SaveOrUpdateEvent evt, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
