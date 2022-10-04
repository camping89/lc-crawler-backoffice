using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.PageDatasource.Aladin.Articles;
using LC.Crawler.BackOffice.PageDatasource.Aladin.Categories;
using LC.Crawler.BackOffice.PageDatasource.Aladin.Medias;
using LC.Crawler.BackOffice.PageDatasource.Aladin.MongoDb;
using LC.Crawler.BackOffice.PageDatasource.Aladin.Products;
using LC.Crawler.BackOffice.PageDatasource.LongChau.Articles;
using LC.Crawler.BackOffice.PageDatasource.LongChau.Categories;
using LC.Crawler.BackOffice.PageDatasource.LongChau.Medias;
using LC.Crawler.BackOffice.PageDatasource.LongChau.MongoDb;
using LC.Crawler.BackOffice.PageDatasource.LongChau.Products;
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

namespace LC.Crawler.BackOffice.PageDatasource;

[DependsOn(
    typeof(BackOfficeDomainModule)
    //,
    // typeof(AbpPermissionManagementMongoDbModule),
    // typeof(AbpSettingManagementMongoDbModule),
    // typeof(AbpIdentityProMongoDbModule),
    // typeof(AbpIdentityServerMongoDbModule),
    // typeof(AbpBackgroundJobsMongoDbModule),
    // typeof(AbpAuditLoggingMongoDbModule),
    // typeof(AbpFeatureManagementMongoDbModule),
    // typeof(LanguageManagementMongoDbModule),
    // typeof(SaasMongoDbModule),
    // typeof(TextTemplateManagementMongoDbModule),
    // typeof(AbpGdprMongoDbModule),
    // typeof(BlobStoringDatabaseMongoDbModule)
)]
public class PageDataSourceMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<AladinMongoDbContext>(options =>
        {
            //options.AddDefaultRepositories();

            options.AddRepository<Category, MongoCategoryAladinRepository>();

            options.AddRepository<Article, MongoArticleAladinRepository>();

            options.AddRepository<Media, MongoMediaAladinRepository>();

            options.AddRepository<Product, MongoProductAladinRepository>();

        });
        
        context.Services.AddMongoDbContext<LongChauMongoDbContext>(options =>
        {
            // options.AddDefaultRepositories();

            options.AddRepository<Category, MongoCategoryLongChauRepository>();

            options.AddRepository<Article, MongoArticleLongChauRepository>();

            options.AddRepository<Media, MongoMediaLongChauRepository>();

            options.AddRepository<Product, MongoProductLongChauRepository>();

        });


        Configure<AbpUnitOfWorkDefaultOptions>(options =>
        {
            options.TransactionBehavior = UnitOfWorkTransactionBehavior.Disabled;
        });
    }
}