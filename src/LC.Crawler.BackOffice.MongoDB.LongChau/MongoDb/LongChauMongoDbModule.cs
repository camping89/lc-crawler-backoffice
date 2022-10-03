using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Products;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AuditLogging.MongoDB;
using Volo.Abp.BackgroundJobs.MongoDB;
using Volo.Abp.BlobStoring.Database.MongoDB;
using Volo.Abp.FeatureManagement.MongoDB;
using Volo.Abp.Gdpr;
using Volo.Abp.Identity.MongoDB;
using Volo.Abp.IdentityServer.MongoDB;
using Volo.Abp.LanguageManagement.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.MongoDB;
using Volo.Abp.SettingManagement.MongoDB;
using Volo.Abp.TextTemplateManagement.MongoDB;
using Volo.Abp.Uow;
using Volo.Saas.MongoDB;

namespace LC.Crawler.BackOffice.LongChau.MongoDb;

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
public class LongChauMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<LongChauMongoDbContext>(options =>
        {
            options.AddDefaultRepositories();

            options.AddRepository<Category, Categories.MongoCategoryLongChauRepository>();

            options.AddRepository<Article, Articles.MongoArticleLongChauRepository>();

            options.AddRepository<Media, Medias.MongoMediaLongChauRepository>();

            options.AddRepository<Product, Products.MongoProductLongChauRepository>();

        });

        Configure<AbpUnitOfWorkDefaultOptions>(options =>
        {
            options.TransactionBehavior = UnitOfWorkTransactionBehavior.Disabled;
        });
    }
}