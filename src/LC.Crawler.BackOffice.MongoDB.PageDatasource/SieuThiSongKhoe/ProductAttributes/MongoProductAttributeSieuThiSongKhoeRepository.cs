using System;
using LC.Crawler.BackOffice.PageDatasource.SieuThiSongKhoe.MongoDb;
using LC.Crawler.BackOffice.ProductAttributes;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.SieuThiSongKhoe.ProductAttributes;

public class MongoProductAttributeSieuThiSongKhoeRepository : MongoDbRepository<SieuThiSongKhoeMongoDbContext, ProductAttribute, Guid>, IProductAttributeSieuThiSongKhoeRepository
{
    public MongoProductAttributeSieuThiSongKhoeRepository(IMongoDbContextProvider<SieuThiSongKhoeMongoDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
}