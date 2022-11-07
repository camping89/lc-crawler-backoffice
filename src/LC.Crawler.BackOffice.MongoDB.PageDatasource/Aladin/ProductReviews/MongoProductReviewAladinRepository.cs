using System;
using LC.Crawler.BackOffice.PageDatasource.Aladin.MongoDb;
using LC.Crawler.BackOffice.PageDatasource.LongChau.MongoDb;
using LC.Crawler.BackOffice.ProductReviews;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.Aladin.ProductReviews
{
    public class MongoProductReviewAladinRepository : MongoDbRepository<AladinMongoDbContext, ProductReview, Guid>, IProductReviewAladinRepository
    {
        public MongoProductReviewAladinRepository(IMongoDbContextProvider<AladinMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

    }
}