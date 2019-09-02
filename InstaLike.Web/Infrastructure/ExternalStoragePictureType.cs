using System;
using System.Collections;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Services;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace InstaLike.Web.Infrastructure
{
    internal class ExternalStoragePictureType : AbstractType
    {
        protected static readonly INHibernateLogger Logger = NHibernateLogger.For(typeof(ExternalStoragePictureType));

        public override bool IsMutable => true;

        public override string Name => "ExternalStoragePicture";

        public override Type ReturnedClass => typeof(Picture);

        public override object DeepCopy(object val, ISessionFactoryImplementor factory)
        {
            throw new NotImplementedException();
        }

        public override int GetColumnSpan(IMapping mapping)
        {
            return 1;
        }

        public override bool IsDirty(object old, object current, bool[] checkable, ISessionImplementor session)
        {
            return checkable[0] && IsDirty(old, current, session);
        }

        public override Task<bool> IsDirtyAsync(object old, object current, bool[] checkable, ISessionImplementor session, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            return NullSafeGet(rs, names[0], session, owner);
        }

        public override object NullSafeGet(DbDataReader rs, string name, ISessionImplementor session, object owner)
        {
            throw new NotImplementedException();
        }

        public override Task<object> NullSafeGetAsync(DbDataReader rs, string[] names, ISessionImplementor session, object owner, CancellationToken cancellationToken)
        {
            return NullSafeGetAsync(rs, names[0], session, owner, cancellationToken);
        }

        public override Task<object> NullSafeGetAsync(DbDataReader rs, string name, ISessionImplementor session, object owner, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<object>(cancellationToken);
            }
            try
            {
                return Task.FromResult(NullSafeGet(rs, name, session, owner));
            }
            catch (Exception e)
            {
                return Task.FromException<object>(e);
            }
        }

        public override void NullSafeSet(DbCommand st, object value, int index, bool[] settable, ISessionImplementor session)
        {
            if (settable[0])
            {
                NullSafeSet(st, value, index, session);
            }
        }

        public override void NullSafeSet(DbCommand st, object value, int index, ISessionImplementor session)
        {
            throw new NotImplementedException();
        }

        public override async Task NullSafeSetAsync(DbCommand st, object value, int index, bool[] settable, ISessionImplementor session, CancellationToken cancellationToken)
        {
            if (settable[0])
            {
                await NullSafeSetAsync(st, value, index, session, cancellationToken);
            }
        }

        public override Task NullSafeSetAsync(DbCommand st, object value, int index, ISessionImplementor session, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            try
            {
                NullSafeSet(st, value, index, session);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        public override object Replace(object original, object current, ISessionImplementor session, object owner, IDictionary copiedAlready)
        {
            return original;
        }

        public override Task<object> ReplaceAsync(object original, object current, ISessionImplementor session, object owner, IDictionary copiedAlready, CancellationToken cancellationToken)
        {
            return Task.FromResult(original);
        }

        public override SqlType[] SqlTypes(IMapping mapping)
        {
            return new SqlType[] { new BinaryBlobSqlType() };
        }

        public override bool[] ToColumnNullness(object value, IMapping mapping)
        {
            return value == null ? new[] { false } : new[] { true };
        }

        public override string ToLoggableString(object value, ISessionFactoryImplementor factory)
        {
            throw new NotImplementedException();
        }

        private IExternalStorageProvider GetExternalBlobConnection(ISessionImplementor session)
        {
            if (session.Connection == null)
            {
                throw new NullReferenceException($"{nameof(ExternalStoragePictureType)} requires an open connection.");
            }

            if (!(session.Connection is IExternalStorageProvider connection))
            {
                throw new Exception(
                    $"{nameof(ExternalStoragePictureType)} requires a {nameof(IExternalStorageProvider)}." +
                    $"Make sure you use {nameof(ExternalStorageDriverConnectionProvider)} as your connection provider and specify a {nameof(IExternalStorageConnectionProvider)} in your NHibernate configuration.");
            }

            return connection;
        }
    }
}