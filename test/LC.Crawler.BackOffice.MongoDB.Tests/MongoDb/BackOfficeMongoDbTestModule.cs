using System;
using Volo.Abp.Data;
using Volo.Abp.Modularity;

namespace LC.Crawler.BackOffice.MongoDB;

[DependsOn(
    typeof(BackOfficeTestBaseModule),
    typeof(BackOfficeMongoDbModule)
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
