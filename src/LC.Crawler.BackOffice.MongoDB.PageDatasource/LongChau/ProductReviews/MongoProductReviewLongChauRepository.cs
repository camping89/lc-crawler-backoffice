using System;
using LC.Crawler.BackOffice.PageDatasource.LongChau.MongoDb;
using LC.Crawler.BackOffice.ProductReviews;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.LongChau.ProductReviews
{
    public class MongoProductReviewLongChauRepository : MongoDbRepository<LongChauMongoDbContext, ProductReview, Guid>, IProductReviewLongChauRepository
    {
        public MongoProductReviewLongChauRepository(IMongoDbContextProvider<LongChauMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

    }
}