using LC.Crawler.BackOffice.MongoDB;
using LC.Crawler.BackOffice.PageDatasource;
using LC.Crawler.BackOffice.PageDatasource.Aladin.MongoDb;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace LC.Crawler.BackOffice.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(BackOfficeMongoDbModule),
    typeof(PageDataSourceMongoDbModule),
    typeof(BackOfficeApplicationContractsModule)
)]
public class BackOfficeDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = false;
        });
    }
}
