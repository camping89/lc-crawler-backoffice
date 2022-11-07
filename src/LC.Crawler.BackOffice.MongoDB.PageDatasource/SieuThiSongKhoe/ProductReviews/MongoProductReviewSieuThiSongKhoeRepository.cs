using System;
using LC.Crawler.BackOffice.PageDatasource.SieuThiSongKhoe.MongoDb;
using LC.Crawler.BackOffice.ProductReviews;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.SieuThiSongKhoe.ProductReviews
{
    public class MongoProductReviewSieuThiSongKhoeRepository : MongoDbRepository<SieuThiSongKhoeMongoDbContext, ProductReview, Guid>, IProductReviewSieuThiSongKhoeRepository
    {
        public MongoProductReviewSieuThiSongKhoeRepository(IMongoDbContextProvider<SieuThiSongKhoeMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

    }
}