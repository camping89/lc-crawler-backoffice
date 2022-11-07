using System;
using LC.Crawler.BackOffice.PageDatasource.LongChau.MongoDb;
using LC.Crawler.BackOffice.ProductComments;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.LongChau.ProductComments
{
    public class MongoProductCommentLongChauRepository : MongoDbRepository<LongChauMongoDbContext, ProductComment, Guid>, IProductCommentLongChauRepository
    {
        public MongoProductCommentLongChauRepository(IMongoDbContextProvider<LongChauMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

    }
}