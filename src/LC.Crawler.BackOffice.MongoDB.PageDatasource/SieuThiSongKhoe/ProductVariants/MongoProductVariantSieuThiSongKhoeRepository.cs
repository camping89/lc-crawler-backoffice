using System;
using LC.Crawler.BackOffice.PageDatasource.SieuThiSongKhoe.MongoDb;
using LC.Crawler.BackOffice.ProductVariants;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.SieuThiSongKhoe.ProductVariants;

public class MongoProductVariantSieuThiSongKhoeRepository : MongoDbRepository<SieuThiSongKhoeMongoDbContext, ProductVariant, Guid>, IProductVariantSieuThiSongKhoeRepository
{
    public MongoProductVariantSieuThiSongKhoeRepository(IMongoDbContextProvider<SieuThiSongKhoeMongoDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
}