using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using InstaLike.Web.Data.Mapping;
using InstaLike.Web.Extensions;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace InstaLike.IntegrationTests
{
    public abstract class DatabaseFixture
    {
        public const string Test_Database_Connection_String = "Data Source=InstaLike.db; Version=3; Pooling=True; Synchronous=Full;";
        public const string Test_Picture_Base64 = "/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAMCAgICAgMCAgIDAwMDBAYEBAQEBAgGBgUGCQgKCgkICQkKDA8MCgsOCwkJDRENDg8QEBEQCgwSExIQEw8QEBD/2wBDAQMDAwQDBAgEBAgQCwkLEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBD/wAARCAAoACgDASIAAhEBAxEB/8QAGQABAAMBAQAAAAAAAAAAAAAAAAYHCQUI/8QAMhAAAQIFAgIHBwUAAAAAAAAAAgMEAAUGBxIBEwhCERQhIjEykhUjJEFSYYIWJlFicv/EABQBAQAAAAAAAAAAAAAAAAAAAAD/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwDVOEIpC8/Efb2gZS+lzmZCs4WRNHJJbQccw5dYC0mtYUs+mvsNlUMuWmGBno0ByGq2Ia9/uePZHdjJFjfqXSu9cguZSskP9tmofUd5zsrZgaJmZmZmGef+P6RptaK6tO3lodlXNM7oNnWpoqoLad9usHnTP76dkBOIQhAVtxBVapRNpJ9O2r0G7jZBFEz8CzPTPT0ZxkPUlYPqzm7iavl1jNc8wz+jkjUfjJt7UtxLIzOV0e0VeTNgsEwTZI+d2AAYqJhpzngZ6gPz1HT7RnhWV0LC1Ta2RqVVrM2NyKRk7WlWzZueCDhBqfud5HDPeAPcwEVRraeIyv2MDpbqhog12jWPDADMwDD6AMzP847dGcQNY2llbhjI5ybNo6PMwByYAZ/hEZ1trxAKuuqMuG26BmZYgZ0w8APWYYRNpDau9thajpO9F4bDvCp+XPjUWaHsv8E8MPiQRz2c8zwM+cNP5gPdXALcCsblWZfzuuZ/pM3o1C5TbfHi5VRa4I4Apzh397oA+3DohFZcCD79Y3luRcS29Fv6btdMWiCDRJZDZRWfb2p6bAeGADu+TsDMNPnCA91xFHVsbbvqmCtX9AU25qJHTIJqrKkCej4eC2GfIPphCAlcIQgEIQgP/9k=";

        private static ISessionFactory SessionFactory;

        protected DatabaseFixture()
        {
            SessionFactory = Fluently.Configure()
                .Database(
                    SQLiteConfiguration.Standard
                        .ConnectionString(Test_Database_Connection_String)
                        .AdoNetBatchSize(20)
                        .ShowSql()
                        .FormatSql()
                        .UseReflectionOptimizer()
                )
                .Mappings(m =>
                    m.FluentMappings
                        .Conventions.Add(
                            LazyLoad.Always(),
                            DynamicUpdate.AlwaysTrue(),
                            new AssociationsMappingConvention(),
                            new DateTimeOffsetTypeConvention()
                        )
                        .Add<UserMapping>()
                        .Add<FollowMapping>()
                )
                .ExposeConfiguration(async cfg => 
                {
                    var schemaExport = new SchemaExport(cfg);
                    await schemaExport.DropAsync(true, true);
                    await schemaExport.CreateAsync(true, true);
                }).BuildSessionFactory();
        }

        protected ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}
