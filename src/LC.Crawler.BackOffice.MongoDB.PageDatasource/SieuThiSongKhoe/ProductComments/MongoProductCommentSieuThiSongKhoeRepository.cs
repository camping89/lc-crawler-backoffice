using System;
using LC.Crawler.BackOffice.PageDatasource.SieuThiSongKhoe.MongoDb;
using LC.Crawler.BackOffice.ProductComments;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.SieuThiSongKhoe.ProductComments
{
    public class MongoProductCommentSieuThiSongKhoeRepository : MongoDbRepository<SieuThiSongKhoeMongoDbContext, ProductComment, Guid>, IProductCommentSieuThiSongKhoeRepository
    {
        public MongoProductCommentSieuThiSongKhoeRepository(IMongoDbContextProvider<SieuThiSongKhoeMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

    }
}