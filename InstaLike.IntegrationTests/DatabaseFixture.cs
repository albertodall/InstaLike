using System;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using InstaLike.IntegrationTests.Properties;
using InstaLike.Web.Data;
using InstaLike.Web.Data.Mapping;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Xunit.Abstractions;

namespace InstaLike.IntegrationTests
{
    public class DatabaseFixture : IDisposable
    {
        public DatabaseFixture()
        {
            var databaseFileName = $"InstaLikeTestDb-{Guid.NewGuid().ToString()}.sqlite";

            SessionFactory = Fluently.Configure()
                .Database(
                    SQLiteConfiguration.Standard
                        .ConnectionString(BuildTestDatabaseConnectionString(databaseFileName))
                        .AdoNetBatchSize(20)
                        .ShowSql()
                        .FormatSql()
                        .UseReflectionOptimizer()
                )
                .Mappings(m =>
                    m.FluentMappings
                        .Conventions.Add(
                            DefaultLazy.Always(),
                            DynamicInsert.AlwaysTrue(),
                            DynamicUpdate.AlwaysTrue(),
                            new AssociationsMappingConvention(),
                            new DateTimeOffsetTypeConvention()
                        )
                        .Add<UserMapping>()
                        .Add<FollowMapping>()
                        .Add<PostMapping>()
                        .Add<CommentMapping>()
                        .Add<LikeMapping>()
                        .Add<NotificationMapping>()
                )
                .ExposeConfiguration(async cfg =>
                {
                    var schemaExport = new SchemaExport(cfg);
                    schemaExport.SetOutputFile($"{databaseFileName}_Schema.sql");
                    await schemaExport.DropAsync(true, true);
                    await schemaExport.CreateAsync(true, true);
                })
                .BuildSessionFactory();
        }

        private ISessionFactory SessionFactory { get; }

        public ISession OpenSession(ITestOutputHelper output)
        {
            return SessionFactory
                .WithOptions()
                    .Interceptor(new XUnitSqlStatementOutputInterceptor(output))
                .OpenSession();
        }

        public static string GetTestPictureBase64()
        {
            return Convert.ToBase64String(Resources.GrumpyCat);
        }

        private static string BuildTestDatabaseConnectionString(string testDatabaseFileName)
        {
            return $"Data Source={testDatabaseFileName}; Version=3; Pooling=True; Synchronous=Full; BinaryGuid=False;";
        }

        public void Dispose()
        {
            SessionFactory?.Close();
            SessionFactory?.Dispose();
        }
    }
}
