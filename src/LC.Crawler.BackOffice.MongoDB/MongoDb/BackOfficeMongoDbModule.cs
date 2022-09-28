using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.CrawlerCredentials;
using LC.Crawler.BackOffice.CrawlerProxies;
using LC.Crawler.BackOffice.CrawlerAccounts;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AuditLogging.MongoDB;
using Volo.Abp.BackgroundJobs.MongoDB;
using Volo.Abp.FeatureManagement.MongoDB;
using Volo.Abp.Identity.MongoDB;
using Volo.Abp.IdentityServer.MongoDB;
using Volo.Abp.LanguageManagement.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.MongoDB;
using Volo.Abp.SettingManagement.MongoDB;
using Volo.Abp.TextTemplateManagement.MongoDB;
using Volo.Saas.MongoDB;
using Volo.Abp.BlobStoring.Database.MongoDB;
using Volo.Abp.Uow;
using Volo.Abp.Gdpr;

namespace LC.Crawler.BackOffice.MongoDB;

[DependsOn(
    typeof(BackOfficeDomainModule),
    typeof(AbpPermissionManagementMongoDbModule),
    typeof(AbpSettingManagementMongoDbModule),
    typeof(AbpIdentityProMongoDbModule),
    typeof(AbpIdentityServerMongoDbModule),
    typeof(AbpBackgroundJobsMongoDbModule),
    typeof(AbpAuditLoggingMongoDbModule),
    typeof(AbpFeatureManagementMongoDbModule),
    typeof(LanguageManagementMongoDbModule),
    typeof(SaasMongoDbModule),
    typeof(TextTemplateManagementMongoDbModule),
    typeof(AbpGdprMongoDbModule),
    typeof(BlobStoringDatabaseMongoDbModule)
)]
public class BackOfficeMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<BackOfficeMongoDbContext>(options =>
        {
            options.AddDefaultRepositories();
            options.AddRepository<CrawlerAccount, CrawlerAccounts.MongoCrawlerAccountRepository>();

            options.AddRepository<CrawlerProxy, CrawlerProxies.MongoCrawlerProxyRepository>();

            options.AddRepository<CrawlerCredential, CrawlerCredentials.MongoCrawlerCredentialRepository>();

            options.AddRepository<DataSource, DataSources.MongoDataSourceRepository>();

        });

        Configure<AbpUnitOfWorkDefaultOptions>(options =>
        {
            options.TransactionBehavior = UnitOfWorkTransactionBehavior.Disabled;
        });
    }
}