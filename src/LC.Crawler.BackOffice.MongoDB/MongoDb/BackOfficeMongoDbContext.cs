using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.CrawlerCredentials;
using LC.Crawler.BackOffice.CrawlerProxies;
using LC.Crawler.BackOffice.CrawlerAccounts;
using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.MongoDB;

[ConnectionStringName("Default")]
public class BackOfficeMongoDbContext : AbpMongoDbContext
{
    public IMongoCollection<DataSource> DataSources => Collection<DataSource>();
    public IMongoCollection<CrawlerCredential> CrawlerCredentials => Collection<CrawlerCredential>();
    public IMongoCollection<CrawlerProxy> CrawlerProxies => Collection<CrawlerProxy>();
    public IMongoCollection<CrawlerAccount> CrawlerAccounts => Collection<CrawlerAccount>();

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
    }
}