using System;
using LC.Crawler.BackOffice.LongChau.MongoDb;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;

namespace LC.Crawler.BackOffice.MongoDB;

[DependsOn(
    typeof(BackOfficeTestBaseModule),
    typeof(BackOfficeMongoDbModule),
    typeof(LongChauMongoDbModule)
)]
public class BackOfficeMongoDbTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var stringArray = BackOfficeMongoDbFixture.ConnectionString.Split('?');
        var connectionString = stringArray[0].EnsureEndsWith('/') +
                               "Db_" +
                               Guid.NewGuid().ToString("N") + "/?" + stringArray[1];

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = connectionString;
        });
    }
}
