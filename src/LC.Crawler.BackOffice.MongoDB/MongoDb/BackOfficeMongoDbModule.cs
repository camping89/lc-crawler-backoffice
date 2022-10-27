using LC.Crawler.BackOffice.ArticleComments;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.ProductAttributes;
using LC.Crawler.BackOffice.ProductVariants;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.CrawlerCredentials;
using LC.Crawler.BackOffice.CrawlerProxies;
using LC.Crawler.BackOffice.CrawlerAccounts;
using LC.Crawler.BackOffice.TrackingDataSources;
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
using Volo.FileManagement.MongoDB;

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
[DependsOn(typeof(FileManagementMongoDbModule))]
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

            options.AddRepository<Category, Categories.MongoCategoryRepository>();

            options.AddRepository<Article, Articles.MongoArticleRepository>();

            options.AddRepository<Media, Medias.MongoMediaRepository>();

            options.AddRepository<Product, Products.MongoProductRepository>();

            options.AddRepository<ProductVariant, ProductVariants.MongoProductVariantRepository>();

            options.AddRepository<ProductAttribute, ProductAttributes.MongoProductAttributeRepository>();

            options.AddRepository<ProductReview, ProductReviews.MongoProductReviewRepository>();

            options.AddRepository<ProductComment, ProductComments.MongoProductCommentRepository>();

            options.AddRepository<ArticleComment, ArticleComments.MongoArticleCommentRepository>();
            options.AddRepository<TrackingDataSource, MongoTrackingDataSourceRepository>();

        });

        Configure<AbpUnitOfWorkDefaultOptions>(options =>
        {
            options.TransactionBehavior = UnitOfWorkTransactionBehavior.Disabled;
        });
    }
}