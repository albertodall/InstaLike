using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using InstaLike.Core.Domain;
using InstaLike.Web.Infrastructure;
using InstaLike.Web.Services;

namespace InstaLike.IntegrationTests
{
    internal class InMemoryStorageConnectionProvider : IHybridStorageConnectionProvider
    {
        private readonly InMemoryPictureStorage _storage = new();

        public void Configure(IDictionary<string, string> settings) { }

        public IExternalStorageProvider GetProvider() => _storage;

        private sealed class InMemoryPictureStorage : IExternalStorageProvider
        {
            private readonly ConcurrentDictionary<(string Container, Guid Identifier), Picture> _pictures = new();

            public Picture LoadPicture(Guid identifier, string containerName)
            {
                return _pictures.TryGetValue((containerName, identifier), out var picture)
                    ? picture
                    : Picture.MissingPicture;
            }

            public void SavePicture(Picture picture, string containerName)
            {
                _pictures[(containerName, picture.Identifier)] = picture;
            }
        }
    }
}
