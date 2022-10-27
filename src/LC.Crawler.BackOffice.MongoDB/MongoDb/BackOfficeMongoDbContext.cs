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
using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.MongoDB;

[ConnectionStringName("Default")]
public class BackOfficeMongoDbContext : AbpMongoDbContext
{
    public IMongoCollection<ArticleComment> ArticleComments => Collection<ArticleComment>();
    public IMongoCollection<ProductComment> ProductComments => Collection<ProductComment>();
    public IMongoCollection<ProductReview> ProductReviews => Collection<ProductReview>();
    public IMongoCollection<ProductAttribute> ProductAttributes => Collection<ProductAttribute>();
    public IMongoCollection<ProductVariant> ProductVariants => Collection<ProductVariant>();
    public IMongoCollection<Product> Products => Collection<Product>();
    public IMongoCollection<Media> Medias => Collection<Media>();
    public IMongoCollection<Article> Articles => Collection<Article>();
    public IMongoCollection<Category> Categories => Collection<Category>();
    public IMongoCollection<DataSource> DataSources => Collection<DataSource>();
    public IMongoCollection<CrawlerCredential> CrawlerCredentials => Collection<CrawlerCredential>();
    public IMongoCollection<CrawlerProxy> CrawlerProxies => Collection<CrawlerProxy>();
    public IMongoCollection<CrawlerAccount> CrawlerAccounts => Collection<CrawlerAccount>();
    public IMongoCollection<TrackingDataSource> TrackingDataSources => Collection<TrackingDataSource>();

    /* Add mongo collections here. Example:
     * public IMongoCollection<Question> Questions => Collection<Question>();
     */

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        //builder.Entity<YourEntity>(b =>
        //{
        //    //...
        //});

        modelBuilder.Entity<CrawlerAccount>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "CrawlerAccounts"; });

        modelBuilder.Entity<CrawlerProxy>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "CrawlerProxies"; });

        modelBuilder.Entity<CrawlerCredential>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "CrawlerCredentials"; });

        modelBuilder.Entity<DataSource>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "DataSources"; });

        modelBuilder.Entity<Category>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "Categories"; });

        modelBuilder.Entity<Article>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "Articles"; });

        modelBuilder.Entity<Media>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "Medias"; });

        modelBuilder.Entity<Product>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "Products"; });

        modelBuilder.Entity<ProductVariant>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "ProductVariants"; });

        modelBuilder.Entity<ProductAttribute>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "ProductAttributes"; });

        modelBuilder.Entity<ProductReview>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "ProductReviews"; });

        modelBuilder.Entity<ProductComment>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "ProductComments"; });

        modelBuilder.Entity<ArticleComment>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "ArticleComments"; });
        
        modelBuilder.Entity<TrackingDataSource>(b => { b.CollectionName = BackOfficeConsts.DbTablePrefix + "TrackingDataSources"; });
    }
}