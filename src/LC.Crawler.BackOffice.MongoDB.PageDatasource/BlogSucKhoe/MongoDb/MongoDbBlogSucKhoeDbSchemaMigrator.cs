using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Data;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace LC.Crawler.BackOffice.PageDatasource.BlogSucKhoe.MongoDb;

public class MongoDbBlogSucKhoeDbSchemaMigrator : IBackOfficeDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public MongoDbBlogSucKhoeDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        var dbContexts = _serviceProvider.GetServices<IAbpMongoDbContext>();
        var connectionStringResolver = _serviceProvider.GetRequiredService<IConnectionStringResolver>();

        if (_serviceProvider.GetRequiredService<ICurrentTenant>().IsAvailable)
        {
            dbContexts = dbContexts.Where(x => !x.GetType().IsDefined(typeof(IgnoreMultiTenancyAttribute)));
        }

        foreach (var dbContext in dbContexts)
        {
            var connectionString =
                await connectionStringResolver.ResolveAsync(
                    ConnectionStringNameAttribute.GetConnStringName(dbContext.GetType()));
            var mongoUrl = new MongoUrl(connectionString);
            var databaseName = mongoUrl.DatabaseName;
            var client = new MongoClient(mongoUrl);

            if (databaseName.IsNullOrWhiteSpace())
            {
                databaseName = ConnectionStringNameAttribute.GetConnStringName(dbContext.GetType());
            }

            (dbContext as AbpMongoDbContext)?.InitializeCollections(client.GetDatabase(databaseName));
        }
    }
}
